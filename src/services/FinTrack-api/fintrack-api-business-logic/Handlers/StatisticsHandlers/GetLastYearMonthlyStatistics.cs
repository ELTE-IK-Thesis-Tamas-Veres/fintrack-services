using fintrack_common.DTO.StatisticsDTO;
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
    public class GetLastYearMonthlyStatisticsCommand : IRequest<List<MonthlyStatistics>>
    {
        public uint UserId;
    }

    public class GetLastYearMonthlyStatisticsCommandHandler : IRequestHandler<GetLastYearMonthlyStatisticsCommand, List<MonthlyStatistics>>
    {
        private readonly IRecordRepository _recordRepository;
        public GetLastYearMonthlyStatisticsCommandHandler(IRecordRepository recordRepository)
        {
            _recordRepository = recordRepository;
        }
        public async Task<List<MonthlyStatistics>> Handle(GetLastYearMonthlyStatisticsCommand request, CancellationToken cancellationToken)
        {
            List<Record> records = await _recordRepository.GetRecordsByUserId(request.UserId, cancellationToken);
            List<MonthlyStatistics> monthlyStatistics = new List<MonthlyStatistics>();

            DateTime currentDate = DateTime.Now;

            for (int i = 11; i >= 0; i--)
            {
                DateTime monthDate = currentDate.AddMonths(-i);
                string month = monthDate.ToString("MMM");

                int totalIncome = records
                    .Where(r => r.Date.Month == monthDate.Month && r.Date.Year == monthDate.Year && r.Amount > 0)
                    .Sum(r => r.Amount);

                int totalExpense = records
                    .Where(r => r.Date.Month == monthDate.Month && r.Date.Year == monthDate.Year && r.Amount < 0)
                    .Sum(r => -r.Amount);

                monthlyStatistics.Add(new MonthlyStatistics
                {
                    Month = month,
                    Income = totalIncome,
                    Expense = totalExpense
                });
            }

            return monthlyStatistics;
        }

    }
}
