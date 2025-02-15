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
    public class CreateCategoryCommand : IRequest
    {
        public uint UserId { get; set; }
        public string Name { get; set; } = "";
    }

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryCommandHandler(IUserRepository userRepository, ICategoryRepository categoryRepository)
        {
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            User? user = await _userRepository.FindAsync(request.UserId, cancellationToken);

            if (user == null)
            {
                throw new RecordNotFoundException("User not found");
            }

            Category category = new Category
            {
                User = user,
                Name = request.Name,
                ParentCategory = null
            };

            _categoryRepository.Insert(category);
            await _categoryRepository.SaveAsync(cancellationToken);
        }
    }
}
