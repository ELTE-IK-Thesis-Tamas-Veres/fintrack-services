using fintrack_api_business_logic.Handlers.StatisticsHandlers;
using fintrack_common.DTO.StatisticsDTO;
using fintrack_common.Repositories;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Record = fintrack_database.Entities.Record;

namespace fintrack_api_unit_tests.CommandHandler
{
    public class GetLastYearMonthlyStatisticsTest
    {
        private readonly IRecordRepository _recordRepository;
        private readonly GetLastYearMonthlyStatisticsCommandHandler _handler;

        public GetLastYearMonthlyStatisticsTest ()
        {
            _recordRepository = Substitute.For<IRecordRepository>();
            _handler = new GetLastYearMonthlyStatisticsCommandHandler(_recordRepository);
        }

        [Fact]
        public async Task Handle_NoRecords_Returns12EmptyMonthlyStatistics()
        {
            // Arrange
            uint userId = 1;
            var command = new GetLastYearMonthlyStatisticsCommand { UserId = userId };
            _recordRepository.GetRecordsByUserId(userId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<Record>()));

            // Act
            List<MonthlyStatistics> result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(12, result.Count);
            DateTime currentDate = DateTime.Now;
            // The handler iterates for i = 11 down to 0 (oldest to newest),
            // so the first element corresponds to currentDate.AddMonths(-11)
            for (int i = 11; i >= 0; i--)
            {
                DateTime monthDate = currentDate.AddMonths(-i);
                string expectedMonth = monthDate.ToString("MMM");
                int index = 11 - i;
                MonthlyStatistics stat = result[index];
                Assert.Equal(expectedMonth, stat.Month);
                Assert.Equal(0, stat.Income);
                Assert.Equal(0, stat.Expense);
            }
        }

        [Fact]
        public async Task Handle_WithRecords_ReturnsCorrectMonthlyStatistics()
        {
            // Arrange
            uint userId = 1;
            var command = new GetLastYearMonthlyStatisticsCommand { UserId = userId };
            DateTime now = DateTime.Now;

            // Create records in specific months (using day 15 for clarity)
            var records = new List<Record>();

            // For month = now.AddMonths(-2): two income records (100 and 200) and one expense record (-50)
            DateTime targetMonth1 = now.AddMonths(-2);
            records.Add(new Record { Amount = 100, Date = new DateOnly(targetMonth1.Year, targetMonth1.Month, 15) });
            records.Add(new Record { Amount = 200, Date = new DateOnly(targetMonth1.Year, targetMonth1.Month, 20) });
            records.Add(new Record { Amount = -50, Date = new DateOnly(targetMonth1.Year, targetMonth1.Month, 10) });

            // For month = now.AddMonths(-5): one expense record (-100)
            DateTime targetMonth2 = now.AddMonths(-5);
            records.Add(new Record { Amount = -100, Date = new DateOnly(targetMonth2.Year, targetMonth2.Month, 5) });

            // For the current month: one income record (300)
            DateTime targetMonth3 = now;
            records.Add(new Record { Amount = 300, Date = new DateOnly(targetMonth3.Year, targetMonth3.Month, 1) });

            _recordRepository.GetRecordsByUserId(userId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(records));

            // Act
            List<MonthlyStatistics> result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(12, result.Count);
            // For each month in the last 12 months, compute expected income and expense sums.
            for (int i = 11; i >= 0; i--)
            {
                DateTime monthDate = now.AddMonths(-i);
                string expectedMonth = monthDate.ToString("MMM");

                int expectedIncome = records
                    .Where(r => r.Date.Month == monthDate.Month && r.Date.Year == monthDate.Year && r.Amount > 0)
                    .Sum(r => r.Amount);
                int expectedExpense = records
                    .Where(r => r.Date.Month == monthDate.Month && r.Date.Year == monthDate.Year && r.Amount < 0)
                    .Sum(r => -r.Amount);

                int index = 11 - i;
                MonthlyStatistics stat = result[index];
                Assert.Equal(expectedMonth, stat.Month);
                Assert.Equal(expectedIncome, stat.Income);
                Assert.Equal(expectedExpense, stat.Expense);
            }
        }
    }
}
