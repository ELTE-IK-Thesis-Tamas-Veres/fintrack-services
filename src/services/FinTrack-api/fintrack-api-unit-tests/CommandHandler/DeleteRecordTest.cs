using fintrack_api_business_logic.Handlers.RecordHandlers;
using fintrack_common.Exceptions;
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
    public class DeleteRecordTest
    {
        private readonly IRecordRepository _recordRepository;
        private readonly DeleteRecordCommandHandler _handler;

        public DeleteRecordTest()
        {
            _recordRepository = Substitute.For<IRecordRepository>();
            _handler = new DeleteRecordCommandHandler(_recordRepository);
        }

        [Fact]
        public async Task Handle_WithValidRecord_DeletesRecord()
        {
            // Arrange
            uint userId = 1;
            uint recordId = 10;
            var record = new Record { Id = recordId, UserId = userId };
            _recordRepository.FindAsync(recordId, Arg.Any<CancellationToken>()).Returns(record);
            var command = new DeleteRecordCommand { UserId = userId, RecordId = recordId };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _recordRepository.Received(1).Delete(record);
            await _recordRepository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_RecordNotFound_ThrowsRecordNotFoundException()
        {
            // Arrange
            uint userId = 1;
            uint recordId = 10;
            _recordRepository.FindAsync(recordId, Arg.Any<CancellationToken>()).Returns((Record?)null);
            var command = new DeleteRecordCommand { UserId = userId, RecordId = recordId };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RecordNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Record not found", ex.Message);
        }

        [Fact]
        public async Task Handle_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            uint userId = 1;
            uint recordId = 10;
            // Record belongs to a different user
            var record = new Record { Id = recordId, UserId = 2 };
            _recordRepository.FindAsync(recordId, Arg.Any<CancellationToken>()).Returns(record);
            var command = new DeleteRecordCommand { UserId = userId, RecordId = recordId };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains($"userId {userId} cannot delete recordId {recordId}", ex.Message);
        }
    }
}
