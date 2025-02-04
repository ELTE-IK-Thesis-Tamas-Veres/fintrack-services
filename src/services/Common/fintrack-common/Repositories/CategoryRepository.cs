using fintrack_common.DTO.CategoryDTO;
using fintrack_common.Repositories.Generic;
using fintrack_database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_common.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(FinTrackDatabaseContext context) : base(context)
        {
        }

        public async Task<List<GetCategoryResponse>> GetCategoriesByUser(uint userId, CancellationToken cancellationToken)
        {
            return await context.Categories
                .Where(c => c.UserId == userId)
                .Select(c => new GetCategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                })
                .ToListAsync(cancellationToken);
        }
    }
}
