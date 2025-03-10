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

        public async Task<int> GetNetOfCategoryByRecordFilter (uint categoryId, Func<Record, bool> filter, CancellationToken cancellationToken)
        {
            Category? category = await context.Categories
                .Include(c => c.Records)
                .Include(c => c.ChildCategories)
                .Where(c => c.Id == categoryId)
                .FirstOrDefaultAsync(cancellationToken);

            if (category == null)
            {
                return 0;
            }

            int sum = category.Records
                .Where(filter)
                .Sum(r => r.Amount);

            foreach (Category childCategory in category.ChildCategories)
            {
                sum += await GetNetOfCategoryByRecordFilter(childCategory.Id, filter, cancellationToken);
            }

            return sum;
        }

        public async Task<Category?> GetCategoryWithRecordsById(uint categoryId, CancellationToken cancellationToken)
        {
            return await context.Categories
                .Include(c => c.Records)
                .Where(c => c.Id == categoryId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Category>> GetCategoriesByUserIdWhereParentIsNull(uint userId, CancellationToken cancellationToken)
        {
            return await context.Categories
                .Where(c => c.UserId == userId && c.ParentCategoryId == null)
                .ToListAsync(cancellationToken);
        }

        public async Task<Category?> GetCategoryByIdWithChildCategories(uint categoryId, CancellationToken cancellationToken)
        {
            return await context.Categories
                .Include(i => i.ChildCategories)
                .Where(c => c.Id == categoryId)
                .FirstOrDefaultAsync(cancellationToken);
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
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Category>> GetCategoriesByCategoryIdList(List<uint> categoryIds, CancellationToken cancellationToken)
        {
            return await context.Categories.Where(c => categoryIds.Contains(c.Id)).ToListAsync(cancellationToken);
        }

        public async Task<List<GetCategoryTreeNodeResponse>> GetCategoryTree(uint userId, CancellationToken cancellationToken)
        {
            List<Category> rootCategories = await context.Categories
                .Where(c => c.UserId == userId && c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
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
                .OrderBy(c => c.Name)
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

            return childCategoryNodes;
        }
    }
}
