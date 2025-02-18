using fintrack_common.DTO.CategoryDTO;
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
    public class EditParentOfCategoriesCommand : IRequest
    {
        public uint UserId;
        public EditParentOfCategoriesRequest Request;

        public EditParentOfCategoriesCommand(EditParentOfCategoriesRequest request)
        {
            Request = request;
        }
    }

    public class EditParentOfCategoriesCommandHandler : IRequestHandler<EditParentOfCategoriesCommand>
    {
        ICategoryRepository _categoryRepository;

        public EditParentOfCategoriesCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task Handle(EditParentOfCategoriesCommand command, CancellationToken cancellationToken)
        {
            List<Category> categories = await _categoryRepository.GetCategoriesByCategoryIdList(command.Request.CategoryIds, cancellationToken);

            if (categories.Any(c => c.UserId != command.UserId))
            {
                throw new UnauthorizedAccessException($"userId {command.UserId} does not have access to at least 1 categoryId");
            }

            Category? parentCategory = command.Request.ParentId is null ? null : await _categoryRepository.FindAsync((uint) command.Request.ParentId, cancellationToken);

            if (command.Request.ParentId is not null && parentCategory is null)
            {
                throw new RecordNotFoundException($"category not found with id {command.Request.ParentId}");
            }

            foreach (Category category in categories)
            {
                category.ParentCategory = parentCategory;

                _categoryRepository.Update(category);
            }

            await _categoryRepository.SaveAsync(cancellationToken);
        }
    }
}
