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

        public async Task<List<GetCategoryTreeNodeResponse>> GetCategoryTree(uint userId, CancellationToken cancellationToken)
        {
            List<Category> rootCategories = await context.Categories
                .Where(c => c.UserId == userId && c.ParentCategoryId == null)
                .ToListAsync(cancellationToken);

            List<GetCategoryTreeNodeResponse> response = new List<GetCategoryTreeNodeResponse>();

            foreach (var node in rootCategories)
            {
                response.Add(new GetCategoryTreeNodeResponse()
                {
                    Id = node.Id.ToString(),
                    Name = node.Name,
                    Children = await GetChildCategoryNodes(node.Id, cancellationToken)
                });
            }

            return response;
        }

        private async Task<List<GetCategoryTreeNodeResponse>?> GetChildCategoryNodes(uint parentId, CancellationToken cancellationToken)
        {
            List<Category> childCategories = await context.Categories
                .Where(c => c.ParentCategoryId == parentId)
                .ToListAsync(cancellationToken);
            List<GetCategoryTreeNodeResponse> childCategoryNodes = new List<GetCategoryTreeNodeResponse>();
            foreach (Category category in childCategories)
            {
                GetCategoryTreeNodeResponse childCategoryNode = new GetCategoryTreeNodeResponse
                {
                    Id = category.Id.ToString(),
                    Name = category.Name,
                    Children = await GetChildCategoryNodes(category.Id, cancellationToken)
                };
                childCategoryNodes.Add(childCategoryNode);
            }

            return childCategoryNodes.Count > 0 ? childCategoryNodes : null;
        }
    }
}
