using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_common.Providers
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime GetNow()
        {
            return DateTime.Now;
        }

        public DateTime GetUTCNow()
        {
            return DateTime.UtcNow;
        }

        public DateTimeOffset GetUTCNowWithOffset()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
