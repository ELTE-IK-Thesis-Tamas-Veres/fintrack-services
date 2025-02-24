using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_common.DTO.RecordDTO
{
    public class EditRecordRequest
    {
        public required string Description { get; set; } = "";
        public required int Amount { get; set; }
        public required DateOnly Date { get; set; }
        public uint? CategoryId { get; set; }
    }
}
