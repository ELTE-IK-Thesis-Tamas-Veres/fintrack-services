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
        Task<List<Category>> GetCategoriesByCategoryIdList(List<uint> categoryIds, CancellationToken cancellationToken);
        Task<List<GetCategoryResponse>> GetCategoriesByUser(uint userId, CancellationToken cancellationToken);
        Task<List<Category>> GetCategoriesByUserIdWhereParentIsNull(uint userId, CancellationToken cancellationToken);
        Task<Category?> GetCategoryByIdWithChildCategories(uint categoryId, CancellationToken cancellationToken);
        Task<List<GetCategoryTreeNodeResponse>> GetCategoryTree(uint userId, CancellationToken cancellationToken);
        Task<Category?> GetCategoryWithRecordsById(uint categoryId, CancellationToken cancellationToken);
        Task<int> GetNetOfCategoryByRecordFilter(uint categoryId, Func<Record, bool> filter, CancellationToken cancellationToken);
    }
}
