using fintrack_common.Exceptions;
using fintrack_common.Repositories;
using fintrack_database.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_api_business_logic.Handlers.RecordHandlers
{
    public class DeleteRecordCommand : IRequest
    {
        public uint UserId;
        public uint RecordId;
    }

    public class DeleteRecordCommandHandler : IRequestHandler<DeleteRecordCommand>
    {
        private readonly IRecordRepository _recordRepository;

        public DeleteRecordCommandHandler(IRecordRepository recordRepository)
        {
            _recordRepository = recordRepository;
        }

        public async Task Handle(DeleteRecordCommand request, CancellationToken cancellationToken)
        {
            Record? record = await _recordRepository.FindAsync(request.RecordId, cancellationToken);

            if (record is null)
            {
                throw new RecordNotFoundException("Record not found");
            }

            if (record.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException($"userId {request.UserId} cannot delete recordId {request.RecordId}");
            }

            _recordRepository.Delete(record);
            await _recordRepository.SaveAsync(cancellationToken);
        }
    }
}
