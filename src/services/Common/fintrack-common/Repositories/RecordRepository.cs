using fintrack_common.DTO.CategoryDTO;
using fintrack_common.DTO.RecordDTO;
using fintrack_common.DTO.SankeyDTO;
using fintrack_common.Exceptions;
using fintrack_common.Repositories.Generic;
using fintrack_database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace fintrack_common.Repositories
{
    public class RecordRepository : GenericRepository<Record>, IRecordRepository
    {
        public RecordRepository(FinTrackDatabaseContext context) : base(context)
        {
        }

        public async Task<List<Record>> GetRecordsByUserId (uint userId, CancellationToken cancellationToken)
        {
            return await context.Records
                .Include(i => i.Category)
                .Where(r => r.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Record>> GetRecordByUserIdWhereCategoryIsNull (uint userId, CancellationToken cancellationToken)
        {
            return await context.Records
                .Where(r => r.UserId == userId && r.CategoryId == null)
                .ToListAsync(cancellationToken);
        }

        public async Task<GetSankeyDataResponse?> GetSankeyDataWithDateFilter(uint categoryId, Func<Record, bool> recordFilter, CancellationToken cancellationToken)
        {
            GetSankeyDataResponse response = new GetSankeyDataResponse();

            Category? category = await context
                .Categories
                .Include(i => i.Records)
                .Include(i => i.ChildCategories)
                .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

            if (category == null)
            {
                throw new RecordNotFoundException($"category not found with id {categoryId}");
            }

            category.Records = category.Records.Where(recordFilter).ToList();

            foreach (Category childCategory in category.ChildCategories)
            {
                GetSankeyDataResponse? sankeyData = await GetSankeyDataWithDateFilter(childCategory.Id, recordFilter, cancellationToken);

                if (sankeyData != null)
                {
                    response.Nodes.AddRange(sankeyData.Nodes);
                    response.Links.AddRange(sankeyData.Links);
                }
            }

            int incomeSum = category.Records.Where(r => r.Amount > 0 && recordFilter(r)).Sum(r => r.Amount);
            int expenseSum = category.Records.Where(r => r.Amount < 0 && recordFilter(r)).Sum(r => -r.Amount);

            string incomeNodeId = $"{category.Id}-i";
            var incomeConnectingLinks = response.Links.Where(l => l.TargetText == incomeNodeId);

            if (incomeSum > 0 || incomeConnectingLinks.Any())
            {
                SankeyNode incomeNode = new SankeyNode()
                {
                    IdText = incomeNodeId,
                    Name = category.Name,
                };

                SankeyLink link = new SankeyLink()
                {
                    SourceText = incomeNodeId,
                    TargetText = category.ParentCategoryId is null ? "b" : category.ParentCategoryId.ToString() + "-i",
                    Value = incomeSum + incomeConnectingLinks.Sum(l => l.Value)
                };

                response.Nodes.Add(incomeNode);
                response.Links.Add(link);

                if (incomeConnectingLinks.Any() && incomeSum > 0)
                {
                    string uncategorisedNodeId = $"{incomeNodeId}u";

                    SankeyNode uncategorisedNode = new SankeyNode()
                    {
                        IdText = uncategorisedNodeId,
                        Name = "Uncategorised"
                    };

                    SankeyLink uncategorisedLink = new SankeyLink()
                    {
                        SourceText = uncategorisedNodeId,
                        TargetText = incomeNodeId,
                        Value = incomeSum
                    };

                    response.Nodes.Add(uncategorisedNode);
                    response.Links.Add(uncategorisedLink);
                }
            }
            
            string expenseNodeId = $"{category.Id}-x";
            var expenseConnectingLinks = response.Links.Where(l => l.SourceText == expenseNodeId);

            if (expenseSum > 0 || expenseConnectingLinks.Any())
            {
                SankeyNode expenseNode = new SankeyNode()
                {
                    IdText = expenseNodeId,
                    Name = category.Name
                };

                SankeyLink link = new SankeyLink()
                {
                    SourceText = category.ParentCategoryId is null ? "b" : category.ParentCategoryId.ToString() + "-x",
                    TargetText = expenseNodeId,
                    Value = (expenseSum + expenseConnectingLinks.Sum(l => l.Value))
                };

                response.Nodes.Add(expenseNode);
                response.Links.Add(link);

                if (expenseConnectingLinks.Any() && expenseSum > 0)
                {
                    string uncategorisedNodeId = $"{expenseNodeId}u";

                    SankeyNode uncategorised = new SankeyNode()
                    {
                        IdText = uncategorisedNodeId,
                        Name = "Uncategorised"
                    };

                    SankeyLink uncategorisedLink = new SankeyLink()
                    {
                        SourceText = expenseNodeId,
                        TargetText = uncategorisedNodeId,
                        Value = expenseSum
                    };

                    response.Nodes.Add(uncategorised);
                    response.Links.Add(uncategorisedLink);
                }
            }

            return response;
        }

        public async Task<List<Record>> GetRecordsByCategoryId (uint categoryId, CancellationToken cancellationToken)
        {
            return await context.Records
                .Where(r => r.CategoryId == categoryId)
                .ToListAsync(cancellationToken);
        }
    }
}
