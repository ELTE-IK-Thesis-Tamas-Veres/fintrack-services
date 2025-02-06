using fintrack_common.DTO.CategoryDTO;
using fintrack_common.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_api_business_logic.Handlers.CategoryHandlers
{
    public class GetCategoriesCommand : IRequest<List<GetCategoryResponse>>
    {
        public uint UserId { get; set; }
    }

    public class GetCategoriesCommandHandler : IRequestHandler<GetCategoriesCommand, List<GetCategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        public GetCategoriesCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<List<GetCategoryResponse>> Handle(GetCategoriesCommand request, CancellationToken cancellationToken)
        {
            return await _categoryRepository.GetCategoriesByUser(request.UserId, cancellationToken);
        }
    }
}
