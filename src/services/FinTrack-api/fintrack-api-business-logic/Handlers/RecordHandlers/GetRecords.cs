using fintrack_common.DTO.CategoryDTO;
using fintrack_common.DTO.RecordDTO;
using fintrack_common.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_api_business_logic.Handlers.RecordHandlers
{
    public class GetRecordsCommand : IRequest<List<GetRecordResponse>>
    {
        public uint UserId { get; set; }
    }

    public class GetRecordsCommandHandler : IRequestHandler<GetRecordsCommand, List<GetRecordResponse>>
    {
        private readonly IRecordRepository _recordRepository;

        public GetRecordsCommandHandler (IRecordRepository recordRepository)
        {
            _recordRepository = recordRepository;
        }
        
        public Task<List<GetRecordResponse>> Handle(GetRecordsCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(_recordRepository.GetRecordsByUserId(command.UserId, cancellationToken).Result.Select(record => new GetRecordResponse()
            {
                Id = record.Id,
                Amount = record.Amount,
                Date = record.Date,
                Description = record.Description,
                Category = record.Category == null ? null : (new GetCategoryResponse()
                {
                    Id = record.Category.Id,
                    Name = record.Category.Name
                })
            }).ToList());
        }
    }
}
