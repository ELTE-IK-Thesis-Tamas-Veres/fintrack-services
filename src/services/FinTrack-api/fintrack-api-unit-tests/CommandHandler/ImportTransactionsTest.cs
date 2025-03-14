using fintrack_api_business_logic.Handlers.ImportHandlers;
using fintrack_common.DTO.ImportDTO;
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
    public class ImportTransactionsCommandHandlerTests
    {
        private IRecordRepository _recordRepository;
        private ICategoryRepository _categoryRepository;
        private ImportTransactionsCommandHandler _handler;
        public ImportTransactionsCommandHandlerTests ()
        {
            _recordRepository = Substitute.For<IRecordRepository>();
            _categoryRepository = Substitute.For<ICategoryRepository>();
            _handler = new ImportTransactionsCommandHandler(_recordRepository, _categoryRepository);
        }

        [Fact]
        public async Task Handle_WithExistingCategory_InsertsRecordWithCategoryId()
        {
            // Arrange
            uint userId = 1;
            var existingCategory = new Category { Id = 100, Name = "Groceries", UserId = userId };
            // Simulate that the user already has the "Groceries" category.
            _categoryRepository
                .GetCategoriesByUserId(userId, Arg.Any<CancellationToken>())
                .Returns(new List<Category> { existingCategory });

            var transaction = new ImportTransaction
            {
                Amount = new AmountModel () { Value = 50 },
                TransactionDateTime = DateTimeOffset.Now,
                Booking = DateTimeOffset.Now.AddDays(-1),
                Note = "Test transaction",
                Categories = new List<string> { "Groceries" }
            };

            var command = new ImportTransactionsCommand
            {
                UserId = userId,
                Transactions = new List<ImportTransaction> { transaction }
            };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            // No new category is inserted because "Groceries" already exists.
            _categoryRepository.DidNotReceive().Insert(Arg.Any<Category>());
            // A record is inserted using the existing category id.
            _recordRepository.Received(1).Insert(Arg.Is<Record>(r =>
                r.Amount == 50.0m &&
                r.CategoryId == 100 &&
                r.Description == "Test transaction" &&
                r.UserId == userId
            ));
            // Verify that SaveAsync is called on both repositories.
            await _categoryRepository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
            await _recordRepository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WithNewCategory_InsertsCategoryAndRecordWithNewCategoryId()
        {
            // Arrange
            uint userId = 1;
            // First call returns an empty list, meaning the category doesn't exist.
            // After insertion, assume the category repository returns a new category with Id 200.
            _categoryRepository
                .GetCategoriesByUserId(userId, Arg.Any<CancellationToken>())
                .Returns(new List<Category>(), new List<Category> { new Category { Id = 200, Name = "Transport", UserId = userId } });

            var transaction = new ImportTransaction
            {
                Amount = new AmountModel () { Value = 25 },
                TransactionDateTime = null, // Will use Booking instead.
                Booking = DateTimeOffset.Now,
                Note = "Bus fare",
                Categories = new List<string> { "Transport" }
            };

            var command = new ImportTransactionsCommand
            {
                UserId = userId,
                Transactions = new List<ImportTransaction> { transaction }
            };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            // Verify that a new category "Transport" is inserted.
            _categoryRepository.Received(1).Insert(Arg.Is<Category>(c =>
                c.Name == "Transport" && c.UserId == userId
            ));
            await _categoryRepository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
            // Verify that a record is inserted with the new category id (200).
            _recordRepository.Received(1).Insert(Arg.Is<Record>(r =>
                r.Amount == 25.0m &&
                r.CategoryId == 200 &&
                r.Description == "Bus fare" &&
                r.UserId == userId
            ));
            await _recordRepository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WithNullCategory_InsertsRecordWithNullCategoryId()
        {
            // Arrange
            uint userId = 1;
            // Return an empty list for categories.
            _categoryRepository
                .GetCategoriesByUserId(userId, Arg.Any<CancellationToken>())
                .Returns(new List<Category>());

            var transaction = new ImportTransaction
            {
                Amount = new AmountModel () { Value = 10 },
                TransactionDateTime = DateTimeOffset.Now,
                Booking = DateTimeOffset.Now, // Not used because TransactionDateTime exists.
                Note = "No category provided",
                Categories = new List<string>() // No categories.
            };

            var command = new ImportTransactionsCommand
            {
                UserId = userId,
                Transactions = new List<ImportTransaction> { transaction }
            };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            // No new category is inserted.
            _categoryRepository.DidNotReceive().Insert(Arg.Any<Category>());
            // Record is inserted with a null CategoryId.
            _recordRepository.Received(1).Insert(Arg.Is<Record>(r =>
                r.Amount == 10.0m &&
                r.CategoryId == null &&
                r.Description == "No category provided" &&
                r.UserId == userId
            ));
            await _recordRepository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WithMultipleTransactions_InsertsRecordsForEachTransaction()
        {
            // Arrange
            uint userId = 1;
            // First call returns one existing category ("Food").
            var existingCategory = new Category { Id = 300, Name = "Food", UserId = userId };
            // When a new category ("Drinks") is needed, the repository is refreshed.
            _categoryRepository
                .GetCategoriesByUserId(userId, Arg.Any<CancellationToken>())
                .Returns(new List<Category> { existingCategory },
                         new List<Category> { existingCategory, new Category { Id = 400, Name = "Drinks", UserId = userId } });

            var transaction1 = new ImportTransaction
            {
                Amount = new AmountModel() { Value = 10 },
                TransactionDateTime = DateTimeOffset.Now,
                Booking = DateTimeOffset.Now,
                Note = "Lunch",
                Categories = new List<string> { "Food" }
            };

            var transaction2 = new ImportTransaction
            {
                Amount = new AmountModel () { Value = 5 },
                TransactionDateTime = null, // Will use Booking.
                Booking = DateTimeOffset.Now,
                Note = "Evening drink",
                Categories = new List<string> { "Drinks" }
            };

            var command = new ImportTransactionsCommand
            {
                UserId = userId,
                Transactions = new List<ImportTransaction> { transaction1, transaction2 }
            };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            // Verify that the new category "Drinks" is inserted.
            _categoryRepository.Received(1).Insert(Arg.Is<Category>(c =>
                c.Name == "Drinks" && c.UserId == userId
            ));
            await _categoryRepository.Received(1).SaveAsync(Arg.Any<CancellationToken>());

            // Verify record insertions for both transactions.
            _recordRepository.Received(1).Insert(Arg.Is<Record>(r =>
                r.Amount == 10 &&
                r.CategoryId == 300 &&
                r.Description == "Lunch" &&
                r.UserId == userId
            ));
            _recordRepository.Received(1).Insert(Arg.Is<Record>(r =>
                r.Amount == 5 &&
                r.CategoryId == 400 &&
                r.Description == "Evening drink" &&
                r.UserId == userId
            ));
            await _recordRepository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }
    }
}
