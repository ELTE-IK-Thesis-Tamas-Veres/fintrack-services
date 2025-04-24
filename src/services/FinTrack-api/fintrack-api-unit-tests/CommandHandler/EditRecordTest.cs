using fintrack_api_business_logic.Handlers.RecordHandlers;
using fintrack_common.DTO.RecordDTO;
using fintrack_common.Exceptions;
using fintrack_common.Repositories;
using fintrack_database.Entities;
using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Record = fintrack_database.Entities.Record;

namespace fintrack_api_unit_tests.CommandHandler
{
    public class EditRecordTest
    {
        private readonly IRecordRepository _recordRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly EditRecordCommandHandler _handler;

        public EditRecordTest()
        {
            _recordRepository = Substitute.For<IRecordRepository>();
            _categoryRepository = Substitute.For<ICategoryRepository>();
            _handler = new EditRecordCommandHandler(_recordRepository, _categoryRepository);
        }

        [Fact]
        public async Task Handle_WithValidRecordAndValidCategory_UpdatesRecordSuccessfully()
        {
            // Arrange
            uint userId = 1;
            uint recordId = 20;
            uint categoryId = 5;
            var editRequest = new EditRecordRequest
            {
                Description = "Updated record",
                Amount = 200,
                Date = DateOnly.FromDateTime(DateTime.Now.Date),
                CategoryId = categoryId
            };
            var command = new EditRecordCommand(editRequest)
            {
                UserId = userId,
                RecordId = recordId
            };

            var record = new Record
            {
                Id = recordId,
                UserId = userId,
                Description = "Old description",
                Amount = 100,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                CategoryId = null
            };
            _recordRepository.FindAsync(recordId, Arg.Any<CancellationToken>()).Returns(record);

            var category = new Category { Id = categoryId, UserId = userId, Name = "TestCategory" };
            _categoryRepository.FindAsync(categoryId, Arg.Any<CancellationToken>()).Returns(category);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert that the record has been updated.
            Assert.Equal(editRequest.Description, record.Description);
            Assert.Equal(editRequest.Amount, record.Amount);
            Assert.Equal(editRequest.Date, record.Date);
            Assert.Equal(editRequest.CategoryId, record.CategoryId);

            await _recordRepository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WithValidRecordAndNullCategory_UpdatesRecordSuccessfully()
        {
            // Arrange
            uint userId = 1;
            uint recordId = 21;
            var editRequest = new EditRecordRequest
            {
                Description = "Updated record without category",
                Amount = 150,
                Date = DateOnly.FromDateTime(DateTime.Now.Date),
                CategoryId = null
            };
            var command = new EditRecordCommand(editRequest)
            {
                UserId = userId,
                RecordId = recordId
            };

            var record = new Record
            {
                Id = recordId,
                UserId = userId,
                Description = "Old description",
                Amount = 100,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                CategoryId = 10
            };
            _recordRepository.FindAsync(recordId, Arg.Any<CancellationToken>()).Returns(record);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(editRequest.Description, record.Description);
            Assert.Equal(editRequest.Amount, record.Amount);
            Assert.Equal(editRequest.Date, record.Date);
            Assert.Null(record.CategoryId);

            await _recordRepository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_RecordNotFound_ThrowsRecordNotFoundException()
        {
            // Arrange
            uint userId = 1;
            uint recordId = 22;
            var editRequest = new EditRecordRequest
            {
                Description = "Attempted update",
                Amount = 300,
                Date = DateOnly.FromDateTime(DateTime.Now.Date),
                CategoryId = null
            };
            var command = new EditRecordCommand(editRequest)
            {
                UserId = userId,
                RecordId = recordId
            };

            _recordRepository.FindAsync(recordId, Arg.Any<CancellationToken>()).Returns((Record?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RecordNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Record not found", ex.Message);
        }

        [Fact]
        public async Task Handle_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            uint userId = 1;
            uint recordId = 23;
            var editRequest = new EditRecordRequest
            {
                Description = "Unauthorized update attempt",
                Amount = 250,
                Date = DateOnly.FromDateTime(DateTime.Now.Date),
                CategoryId = null
            };
            var command = new EditRecordCommand(editRequest)
            {
                UserId = userId,
                RecordId = recordId
            };

            var record = new Record
            {
                Id = recordId,
                UserId = 2,
                Description = "Old description",
                Amount = 100,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                CategoryId = null
            };
            _recordRepository.FindAsync(recordId, Arg.Any<CancellationToken>()).Returns(record);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains($"userId {userId} cannot edit recordId {recordId}", ex.Message);
        }

        [Fact]
        public async Task Handle_WithInvalidCategory_ThrowsRecordNotFoundException()
        {
            // Arrange
            uint userId = 1;
            uint recordId = 24;
            uint categoryId = 50;
            var editRequest = new EditRecordRequest
            {
                Description = "Update with invalid category",
                Amount = 400,
                Date = DateOnly.FromDateTime(DateTime.Now.Date),
                CategoryId = categoryId
            };
            var command = new EditRecordCommand(editRequest)
            {
                UserId = userId,
                RecordId = recordId
            };

            var record = new Record
            {
                Id = recordId,
                UserId = userId,
                Description = "Old description",
                Amount = 100,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                CategoryId = null
            };
            _recordRepository.FindAsync(recordId, Arg.Any<CancellationToken>()).Returns(record);

            _categoryRepository.FindAsync(categoryId, Arg.Any<CancellationToken>()).Returns((Category?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RecordNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Category not found", ex.Message);
        }
    }
}
