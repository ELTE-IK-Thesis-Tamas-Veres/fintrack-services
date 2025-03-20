using fintrack_common.DTO.CategoryDTO;
using fintrack_common.DTO.RecordDTO;
using fintrack_common.DTO.SankeyDTO;
using fintrack_common.Exceptions;
using fintrack_common.Repositories.Generic;
using fintrack_database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace fintrack_common.Repositories
{
    public class RecordRepository : GenericRepository<Record>, IRecordRepository
    {
        public RecordRepository(FinTrackDatabaseContext context) : base(context)
        {
        }

        public async Task<List<Record>> GetRecordsByUserId (uint userId, CancellationToken cancellationToken)
        {
            return await context.Records
                .Include(i => i.Category)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Record>> GetRecordByUserIdWhereCategoryIsNull (uint userId, CancellationToken cancellationToken)
        {
            return await context.Records
                .Where(r => r.UserId == userId && r.CategoryId == null)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Record>> GetRecordsByCategoryId (uint categoryId, CancellationToken cancellationToken)
        {
            return await context.Records
                .Where(r => r.CategoryId == categoryId)
                .ToListAsync(cancellationToken);
        }
    }
}
