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
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(FinTrackDatabaseContext context) : base(context)
        {
        }

        public async Task<User?> FindBySubAsync(string sub, CancellationToken cancellationToken)
        {
            return await dbSet.FirstOrDefaultAsync(u => u.Sub == sub, cancellationToken);
        }
    }
}
