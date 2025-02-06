using fintrack_common.DTO.CategoryDTO;
using fintrack_common.Repositories.Generic;
using fintrack_database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_common.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<List<GetCategoryResponse>> GetCategoriesByUser(uint userId, CancellationToken cancellationToken);
    }
}
