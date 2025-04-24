using fintrack_api_business_logic.Handlers.StatisticsHandlers;
using fintrack_common.DTO.StatisticsDTO;
using fintrack_common.Exceptions;
using fintrack_common.Repositories;
using fintrack_database.Entities;
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
    public class GetLastYearCategoryMonthlyStatisticsTest
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly GetLastYearCategoryMonthlyStatisticsCommandHandler _handler;

        public GetLastYearCategoryMonthlyStatisticsTest()
        {
            _categoryRepository = Substitute.For<ICategoryRepository>();
            _handler = new GetLastYearCategoryMonthlyStatisticsCommandHandler(_categoryRepository);
        }

        [Fact]
        public async Task Handle_ValidRequest_Returns12MonthlyStatistics()
        {
            // Arrange
            uint userId = 1;
            uint categoryId = 100;
            var command = new GetLastYearCategoryMonthlyStatisticsCommand
            {
                UserId = userId,
                CategoryId = categoryId
            };

            var category = new Category { Id = categoryId, UserId = userId };
            _categoryRepository.GetCategoryWithRecordsById(categoryId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Category?>(category));

            _categoryRepository
                .GetNetOfCategoryByRecordFilter(categoryId, Arg.Any<Func<Record, bool>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(10));

            DateTime now = DateTime.Now;
            var expectedStatistics = new List<CategoryMonthlyStatistics>();
            for (int i = 11; i >= 0; i--)
            {
                DateTime monthDate = now.AddMonths(-i);
                expectedStatistics.Add(new CategoryMonthlyStatistics
                {
                    Month = monthDate.ToString("MMM"),
                    Amount = 10
                });
            }

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(12, result.Count);
            for (int i = 0; i < 12; i++)
            {
                Assert.Equal(expectedStatistics[i].Month, result[i].Month);
                Assert.Equal(expectedStatistics[i].Amount, result[i].Amount);
            }

            await _categoryRepository.Received(12)
                .GetNetOfCategoryByRecordFilter(categoryId, Arg.Any<Func<Record, bool>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_CategoryNotFound_ThrowsRecordNotFoundException()
        {
            // Arrange
            uint userId = 1;
            uint categoryId = 200;
            var command = new GetLastYearCategoryMonthlyStatisticsCommand
            {
                UserId = userId,
                CategoryId = categoryId
            };

            _categoryRepository.GetCategoryWithRecordsById(categoryId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Category?>(null));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RecordNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Category not found", ex.Message);
        }

        [Fact]
        public async Task Handle_UnauthorizedAccess_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            uint userId = 1;
            uint categoryId = 300;
            var command = new GetLastYearCategoryMonthlyStatisticsCommand
            {
                UserId = userId,
                CategoryId = categoryId
            };

            var category = new Category { Id = categoryId, UserId = 2 };
            _categoryRepository.GetCategoryWithRecordsById(categoryId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Category?>(category));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Unauthorized access", ex.Message);
        }
    }
}