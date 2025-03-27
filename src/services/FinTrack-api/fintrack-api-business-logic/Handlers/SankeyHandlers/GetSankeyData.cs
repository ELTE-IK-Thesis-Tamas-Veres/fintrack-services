using fintrack_common.DTO.SankeyDTO;
using fintrack_common.Exceptions;
using fintrack_common.Repositories;
using fintrack_database.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_api_business_logic.Handlers.SankeyHandlers
{
    public class GetSankeyDataCommand : IRequest<GetSankeyDataResponse>
    {
        public uint UserId;
        public int? Month;
        public int? Year;
    }

    public class GetSankeyDataCommandHandler : IRequestHandler<GetSankeyDataCommand, GetSankeyDataResponse>
    {
        private readonly IRecordRepository _recordRepository;
        private readonly ICategoryRepository _categoryRepository;

        public GetSankeyDataCommandHandler(IRecordRepository recordRepository, ICategoryRepository categoryRepository)
        {
            _recordRepository = recordRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<GetSankeyDataResponse> Handle(GetSankeyDataCommand request, CancellationToken cancellationToken)
        {
            Func<Record, bool> recordFilter = r => true;

            if (request.Month is not null && request.Year is not null)
            {
                recordFilter = r => r.Date.Month == request.Month && r.Date.Year == request.Year;
            }
            else if (request.Year is not null)
            {
                recordFilter = r => r.Date.Year == request.Year;
            }

            List<Category> categories = await _categoryRepository.GetCategoriesByUserIdWhereParentIsNull(request.UserId, cancellationToken);

            GetSankeyDataResponse sankeyData = new GetSankeyDataResponse();

            foreach (Category category in categories)
            {
                GetSankeyDataResponse? sd = await this.BuildSankeyDataWithRecordFilter(category.Id, recordFilter, cancellationToken);

                if (sd is not null)
                {
                    sankeyData.Nodes.AddRange(sd.Nodes);
                    sankeyData.Links.AddRange(sd.Links);
                }
            }

            List<Record> uncategorisedRecords = await _recordRepository.GetRecordByUserIdWhereCategoryIsNull(request.UserId, cancellationToken);

            int uncategorisedBudgetIncome = uncategorisedRecords.Where(r => r.Amount > 0).Sum(r => r.Amount);

            if (uncategorisedBudgetIncome > 0)
            {
                sankeyData.Nodes.Add(new SankeyNode()
                {
                    IdText = "b-iu",
                    Name = "[Uncategorised]"
                });

                sankeyData.Links.Add(new SankeyLink()
                {
                    SourceText = "b-iu",
                    TargetText = "b",
                    Value = uncategorisedBudgetIncome
                });                
            }

            int uncategorisedBudgetExpense = uncategorisedRecords.Where(r => r.Amount < 0).Sum(r => -r.Amount);

            if (uncategorisedBudgetExpense > 0)
            {
                sankeyData.Nodes.Add(new SankeyNode()
                {
                    IdText = "b-xu",
                    Name = "[Uncategorised]"
                });

                sankeyData.Links.Add(new SankeyLink()
                {
                    SourceText = "b",
                    TargetText = "b-xu",
                    Value = uncategorisedBudgetExpense
                });
            }

            int netValueOfBudget = sankeyData.Links.Where(link => link.TargetText == "b").Sum(link => link.Value) - sankeyData.Links.Where(link => link.SourceText == "b").Sum(link => link.Value);

            if (netValueOfBudget != 0)
            {
                SankeyNode extraNode = new SankeyNode()
                {
                    IdText = "b-extra",
                };

                if (netValueOfBudget > 0)
                {
                    extraNode.Name = "[not spent]";
                    sankeyData.Links.Add(new SankeyLink()
                    {
                        SourceText = "b",
                        TargetText = "b-extra",
                        Value = netValueOfBudget
                    });
                }
                else
                {
                    extraNode.Name = "[overspent]";
                    sankeyData.Links.Add(new SankeyLink()
                    {
                        SourceText = "b-extra",
                        TargetText = "b",
                        Value = -netValueOfBudget
                    });
                }

                sankeyData.Nodes.Add(extraNode);
            }

            if (sankeyData.Nodes.Count > 0)
            {
                sankeyData.Nodes.Add(new SankeyNode()
                {
                    IdText = "b",
                    Name = "[budget]"
                });
            }

            foreach (SankeyLink link in sankeyData.Links)
            {
                link.Source = sankeyData.Nodes.FindIndex(n => n.IdText == link.SourceText);
                link.Target = sankeyData.Nodes.FindIndex(n => n.IdText == link.TargetText);
            }

            return sankeyData;
        }

        public async Task<GetSankeyDataResponse?> BuildSankeyDataWithRecordFilter(uint categoryId, Func<Record, bool> recordFilter, CancellationToken cancellationToken)
        {
            GetSankeyDataResponse response = new GetSankeyDataResponse();

            Category? category = await _categoryRepository.GetCategoryByIdWithRecordsAndChildren(categoryId, cancellationToken);

            if (category == null)
            {
                throw new RecordNotFoundException($"category not found with id {categoryId}");
            }

            category.Records = category.Records.Where(recordFilter).ToList();

            foreach (Category childCategory in category.ChildCategories)
            {
                GetSankeyDataResponse? sankeyData = await BuildSankeyDataWithRecordFilter(childCategory.Id, recordFilter, cancellationToken);

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
                        Name = "[Uncategorised]"
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
                        Name = "[Uncategorised]"
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
    }
}
