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
    public class CreateRecordCommand(CreateRecordRequest request, uint userId) : IRequest
    {
        public CreateRecordRequest Request = request;
        public uint UserId = userId;
    }

    public class CreateRecordCommandHandler : IRequestHandler<CreateRecordCommand>
    {
        private readonly IRecordRepository _recordRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;

        public CreateRecordCommandHandler(IRecordRepository recordRepository, ICategoryRepository categoryRepository, IUserRepository userRepository)
        {
            _recordRepository = recordRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
        }

        public async Task Handle(CreateRecordCommand command, CancellationToken cancellationToken)
        {
            Category? category = null;

            if (command.Request.CategoryId is not null)
            {
                category = await _categoryRepository.FindAsync((uint)command.Request.CategoryId, cancellationToken);
                
                if (category is null)
                {
                    throw new RecordNotFoundException($"category not found with id {command.Request.CategoryId}");
                }

                if (category.UserId != command.UserId)
                {
                    throw new UnauthorizedAccessException($"userId {command.UserId} does not have access to categoryId {command.Request.CategoryId}");
                }
            }

            User? user = await _userRepository.FindAsync(command.UserId, cancellationToken);

            if (user is null)
            {
                throw new RecordNotFoundException($"user not found with id {command.UserId}");
            }

            Record record = new Record()
            {
                Amount = command.Request.Amount,
                Date = command.Request.Date,
                Category = category,
                User = user,
                Description = command.Request.Description
            };

            _recordRepository.Insert(record);

            await _recordRepository.SaveAsync(cancellationToken);
        }
    }
}
