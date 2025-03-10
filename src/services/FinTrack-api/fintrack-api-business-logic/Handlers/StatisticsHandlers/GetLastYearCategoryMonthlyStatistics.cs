using fintrack_common.DTO.StatisticsDTO;
using fintrack_common.Exceptions;
using fintrack_common.Repositories;
using fintrack_database.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_api_business_logic.Handlers.StatisticsHandlers
{
    public class GetLastYearCategoryMonthlyStatisticsCommand : IRequest<List<CategoryMonthlyStatistics>>
    {
        public uint UserId { get; set; }
        public uint CategoryId { get; set; }
    }

    public class GetLastYearCategoryMonthlyStatisticsCommandHandler : IRequestHandler<GetLastYearCategoryMonthlyStatisticsCommand, List<CategoryMonthlyStatistics>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetLastYearCategoryMonthlyStatisticsCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<CategoryMonthlyStatistics>> Handle(GetLastYearCategoryMonthlyStatisticsCommand request, CancellationToken cancellationToken)
        {
            Category? category = await _categoryRepository.GetCategoryWithRecordsById(request.CategoryId, cancellationToken);

            if (category == null)
            {
                throw new RecordNotFoundException("Category not found");
            }

            if (category.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("Unauthorized access");
            }
            List<CategoryMonthlyStatistics> monthlyStatistics = new List<CategoryMonthlyStatistics>();

            DateTime currentDate = DateTime.Now;

            for (int i = 11; i >= 0; i--)
            {
                DateTime monthDate = currentDate.AddMonths(-i);
                string month = monthDate.ToString("MMM");

                int total = await _categoryRepository.GetNetOfCategoryByRecordFilter(category.Id, r => r.Date.Month == monthDate.Month && r.Date.Year == monthDate.Year, cancellationToken);

                monthlyStatistics.Add(new CategoryMonthlyStatistics
                {
                    Month = month,
                    Amount = total
                });
            }

            return monthlyStatistics;
        }
    }
}
