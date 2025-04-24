using fintrack_api_business_logic.Handlers.RecordHandlers;
using fintrack_common.DTO.RecordDTO;
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
    public class CreateRecordTest
    {
        private readonly IRecordRepository _recordRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly CreateRecordCommandHandler _handler;

        public CreateRecordTest()
        {
            _recordRepository = Substitute.For<IRecordRepository>();
            _categoryRepository = Substitute.For<ICategoryRepository>();
            _userRepository = Substitute.For<IUserRepository>();
            _handler = new CreateRecordCommandHandler(_recordRepository, _categoryRepository, _userRepository);
        }

        [Fact]
        public async Task Handle_WithValidCategoryAndUser_InsertsRecord()
        {
            // Arrange
            uint userId = 1;
            uint categoryId = 10;
            var request = new CreateRecordRequest
            {
                Amount = 100,
                Date = DateOnly.FromDateTime(DateTime.Now),
                CategoryId = categoryId,
                Description = "Test record with category"
            };
            var command = new CreateRecordCommand(request, userId);

            var category = new Category { Id = categoryId, UserId = userId, Name = "TestCategory" };
            var user = new User { Id = userId };

            _categoryRepository.FindAsync(categoryId, Arg.Any<CancellationToken>()).Returns(category);
            _userRepository.FindAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _recordRepository.Received(1).Insert(Arg.Is<Record>(r =>
                r.Amount == request.Amount &&
                r.Date == request.Date &&
                r.Description == request.Description &&
                r.Category == category &&
                r.User == user
            ));
            await _recordRepository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WithoutCategory_InsertsRecordWithNullCategory()
        {
            // Arrange
            uint userId = 1;
            var request = new CreateRecordRequest
            {
                Amount = 50,
                Date = DateOnly.FromDateTime(DateTime.Now),
                CategoryId = null,
                Description = "Test record without category"
            };
            var command = new CreateRecordCommand(request, userId);

            var user = new User { Id = userId };

            _userRepository.FindAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _recordRepository.Received(1).Insert(Arg.Is<Record>(r =>
                r.Amount == request.Amount &&
                r.Date == request.Date &&
                r.Description == request.Description &&
                r.Category == null &&
                r.User == user
            ));
            await _recordRepository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WithInvalidCategory_ThrowsRecordNotFoundException()
        {
            // Arrange
            uint userId = 1;
            uint categoryId = 10;
            var request = new CreateRecordRequest
            {
                Amount = 100,
                Date = DateOnly.FromDateTime(DateTime.Now),
                CategoryId = categoryId,
                Description = "Test record with invalid category"
            };
            var command = new CreateRecordCommand(request, userId);

            _categoryRepository.FindAsync(categoryId, Arg.Any<CancellationToken>()).Returns((Category?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RecordNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains($"category not found with id {categoryId}", ex.Message);
        }

        [Fact]
        public async Task Handle_WithUnauthorizedCategory_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            uint userId = 1;
            uint categoryId = 10;
            var request = new CreateRecordRequest
            {
                Amount = 100,
                Date = DateOnly.FromDateTime(DateTime.Now),
                CategoryId = categoryId,
                Description = "Test record with unauthorized category"
            };
            var command = new CreateRecordCommand(request, userId);

            var category = new Category { Id = categoryId, UserId = 2, Name = "OtherUserCategory" };
            _categoryRepository.FindAsync(categoryId, Arg.Any<CancellationToken>()).Returns(category);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains($"userId {userId} does not have access to categoryId {categoryId}", ex.Message);
        }

        [Fact]
        public async Task Handle_WithNonexistentUser_ThrowsRecordNotFoundException()
        {
            // Arrange
            uint userId = 1;
            var request = new CreateRecordRequest
            {
                Amount = 75,
                Date = DateOnly.FromDateTime(DateTime.Now),
                CategoryId = null,
                Description = "Test record with nonexistent user"
            };
            var command = new CreateRecordCommand(request, userId);

            _userRepository.FindAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RecordNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains($"user not found with id {userId}", ex.Message);
        }
    }
}
