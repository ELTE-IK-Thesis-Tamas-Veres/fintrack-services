using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_common.DTO.CategoryDTO
{
    public class GetCategoryTreeNodeResponse
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public List<GetCategoryTreeNodeResponse>? Children { get; set; }
    }
}
