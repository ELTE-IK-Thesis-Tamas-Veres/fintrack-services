using fintrack_common.DTO.RecordDTO;
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
    public class EditRecordCommand : IRequest
    {
        public uint UserId { get; set; }
        public uint RecordId { get; set; }
        public EditRecordRequest Request { get; set; }

        public EditRecordCommand(EditRecordRequest request)
        {
            Request = request;
        }
    }

     public class EditRecordCommandHandler : IRequestHandler<EditRecordCommand>
    {
        private readonly IRecordRepository _recordRepository;
        private readonly ICategoryRepository _categoryRepository;

        public EditRecordCommandHandler(IRecordRepository recordRepository, ICategoryRepository categoryRepository)
        {
            _recordRepository = recordRepository;
            _categoryRepository = categoryRepository;
        }
        public async Task Handle(EditRecordCommand request, CancellationToken cancellationToken)
        {
            Record? record = await _recordRepository.FindAsync(request.RecordId, cancellationToken);

            if (record is null)
            {
                throw new RecordNotFoundException("Record not found");
            }

            if (record.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException($"userId {request.UserId} cannot edit recordId {request.RecordId}");
            }

            if (request.Request.CategoryId != null)
            {
                Category? category = await _categoryRepository.FindAsync(request.Request.CategoryId.Value, cancellationToken);

                if (category is null)
                {
                    throw new RecordNotFoundException("Category not found");
                }
            }

            record.Description = request.Request.Description;
            record.Amount = request.Request.Amount;
            record.Date = request.Request.Date;
            record.CategoryId = request.Request.CategoryId;

            await _recordRepository.SaveAsync(cancellationToken);
        }
    }
}
