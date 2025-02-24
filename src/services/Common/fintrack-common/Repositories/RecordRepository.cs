using fintrack_common.DTO.CategoryDTO;
using fintrack_common.DTO.RecordDTO;
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
    public class RecordRepository : GenericRepository<Record>, IRecordRepository
    {
        public RecordRepository(FinTrackDatabaseContext context) : base(context)
        {
        }

        public async Task<List<GetRecordResponse>> GetRecordsByUserId (uint userId, CancellationToken cancellationToken)
        {
            return await context.Records
                .Include(i => i.Category)
                .Where(r => r.UserId == userId)
                .Select(record => new GetRecordResponse()
                {
                    Id = record.Id,
                    Amount = record.Amount,
                    Date = record.Date,
                    Description = record.Description,
                    Category = record.Category == null ? null : (new GetCategoryResponse()
                    {
                        Id = record.Category.Id,
                        Name = record.Category.Name
                    })
                }).ToListAsync(cancellationToken);
        }

        public async Task<List<Record>> GetRecordsByCategoryId (uint categoryId, CancellationToken cancellationToken)
        {
            return await context.Records
                .Where(r => r.CategoryId == categoryId)
                .ToListAsync(cancellationToken);
        }
    }
}
