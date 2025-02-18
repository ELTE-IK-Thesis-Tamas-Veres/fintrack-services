using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_common.DTO.CategoryDTO
{
    public class EditParentOfCategoriesRequest
    {
        public required List<uint> CategoryIds { get; set; }

        public uint? ParentId { get; set; }
    }
}
