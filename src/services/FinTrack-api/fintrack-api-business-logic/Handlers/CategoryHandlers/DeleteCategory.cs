using fintrack_common.Exceptions;
using fintrack_common.Repositories;
using fintrack_database.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_api_business_logic.Handlers.CategoryHandlers
{
    public class DeleteCategoryCommand : IRequest
    {
        public uint CategoryId { get; set; }
        public uint UserId { get; set; }
    }

    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;

        public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
        {
            Category? category = await _categoryRepository.GetCategoryByIdWithChildCategories(command.CategoryId, cancellationToken);

            if (category == null)
            {
                throw new RecordNotFoundException("Category not found");
            }

            if (category.UserId != command.UserId)
            {
                throw new UnauthorizedAccessException($"userId {command.UserId} does not have access to categoryId {command.CategoryId}");
            }

            foreach(Category childCategory in category.ChildCategories)
            {
                childCategory.ParentCategory = null;
                _categoryRepository.Update(childCategory);
            }

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveAsync(cancellationToken);
        }
    }
}
