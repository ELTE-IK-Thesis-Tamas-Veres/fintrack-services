using fintrack_common.DTO.CategoryDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_common.DTO.RecordDTO
{
    public class GetRecordResponse
    {
        public uint Id { get; set; }
        public int Amount { get; set; }
        public DateOnly Date { get; set; }
        public string Description { get; set; } = "";
        public GetCategoryResponse? Category { get; set; } = new ();
    }
}
