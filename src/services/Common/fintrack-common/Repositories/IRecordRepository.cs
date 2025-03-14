using fintrack_common.DTO.RecordDTO;
using fintrack_common.DTO.SankeyDTO;
using fintrack_common.Repositories.Generic;
using fintrack_database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_common.Repositories
{
    public interface IRecordRepository : IGenericRepository<Record>
    {
        Task<List<Record>> GetRecordByUserIdWhereCategoryIsNull(uint userId, CancellationToken cancellationToken);
        Task<List<Record>> GetRecordsByCategoryId(uint categoryId, CancellationToken cancellationToken);
        Task<List<Record>> GetRecordsByUserId(uint userId, CancellationToken cancellationToken);
    }
}
