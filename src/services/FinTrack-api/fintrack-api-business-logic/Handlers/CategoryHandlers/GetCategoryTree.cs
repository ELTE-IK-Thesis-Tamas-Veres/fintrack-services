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
    public class GetCategoryTreeCommand : IRequest<List<GetCategoryTreeNodeResponse>>
    {
        public uint UserId { get; set; }
    }

    public class GetCategoryTreeCommandHandler : IRequestHandler<GetCategoryTreeCommand, List<GetCategoryTreeNodeResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        public GetCategoryTreeCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<List<GetCategoryTreeNodeResponse>> Handle(GetCategoryTreeCommand request, CancellationToken cancellationToken)
        {
            return await _categoryRepository.GetCategoryTree(request.UserId, cancellationToken);
        }
    }
}
