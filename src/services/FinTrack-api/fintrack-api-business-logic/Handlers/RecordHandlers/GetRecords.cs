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
        
        public async Task<List<GetRecordResponse>> Handle(GetRecordsCommand command, CancellationToken cancellationToken)
        {
            return await _recordRepository.GetRecordsByUserId(command.UserId, cancellationToken);
        }
    }
}
