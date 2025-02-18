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
    public class EditCategoryCommand : IRequest
    {
        public uint CategoryId;
        public uint UserId;
        public EditCategoryRequest Request;

        public EditCategoryCommand(EditCategoryRequest request)
        {
            Request = request;
        }
    }

    public class EditCategoryCommandHandler : IRequestHandler<EditCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;

        public EditCategoryCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task Handle(EditCategoryCommand command, CancellationToken cancellationToken)
        {
            Category? category = await _categoryRepository.FindAsync(command.CategoryId, cancellationToken);

            if (category is null)
            {
                throw new RecordNotFoundException($"category not found with id {command.CategoryId}");
            }

            if (category.UserId != command.UserId)
            {
                throw new UnauthorizedAccessException($"userId {command.UserId} does not have access to categoryId {command.CategoryId}");
            }

            category.Name = command.Request.Name;

            _categoryRepository.Update(category);

            await _categoryRepository.SaveAsync(cancellationToken);
        }
    }
}
