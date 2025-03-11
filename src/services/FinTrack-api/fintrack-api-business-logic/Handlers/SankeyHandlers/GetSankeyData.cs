using fintrack_common.DTO.SankeyDTO;
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
                GetSankeyDataResponse? sd = await _recordRepository.GetSankeyDataWithDateFilter(category.Id, recordFilter, cancellationToken);

                if (sd is not null)
                {
                    sankeyData.Nodes.AddRange(sd.Nodes);
                    sankeyData.Links.AddRange(sd.Links);
                }
            }

            if (sankeyData.Nodes.Count > 0)
            {
                sankeyData.Nodes.Add(new SankeyNode()
                {
                    IdText = "b",
                    Name = "budget"
                });
            }

            List<Record> uncategorisedRecords = await _recordRepository.GetRecordByUserIdWhereCategoryIsNull(request.UserId, cancellationToken);

            int uncategorisedBudgetIncome = uncategorisedRecords.Where(r => r.Amount > 0).Sum(r => r.Amount);

            if (uncategorisedBudgetIncome > 0)
            {
                sankeyData.Nodes.Add(new SankeyNode()
                {
                    IdText = "b-iu",
                    Name = "Uncategorised"
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
                    Name = "Uncategorised"
                });

                sankeyData.Links.Add(new SankeyLink()
                {
                    SourceText = "b-xu",
                    TargetText = "b",
                    Value = uncategorisedBudgetExpense
                });
            }

            int netValueOfBudget = uncategorisedBudgetIncome + sankeyData.Links.Where(link => link.TargetText == "b").Sum(link => link.Value) - uncategorisedBudgetExpense - sankeyData.Links.Where(link => link.SourceText == "b").Sum(link => link.Value);

            if (netValueOfBudget != 0)
            {
                SankeyNode extraNode = new SankeyNode()
                {
                    IdText = "b-extra",
                };

                if (netValueOfBudget > 0)
                {
                    extraNode.Name = "income not spent";
                    sankeyData.Links.Add(new SankeyLink()
                    {
                        SourceText = "b",
                        TargetText = "b-extra",
                        Value = netValueOfBudget
                    });
                }
                else
                {
                    extraNode.Name = "overspent";
                    sankeyData.Links.Add(new SankeyLink()
                    {
                        SourceText = "b-extra",
                        TargetText = "b",
                        Value = -netValueOfBudget
                    });
                }

                sankeyData.Nodes.Add(extraNode);
            }
            
            foreach (SankeyLink link in sankeyData.Links)
            {
                link.Source = sankeyData.Nodes.FindIndex(n => n.IdText == link.SourceText);
                link.Target = sankeyData.Nodes.FindIndex(n => n.IdText == link.TargetText);
            }

            return sankeyData;
        }
    }
}
