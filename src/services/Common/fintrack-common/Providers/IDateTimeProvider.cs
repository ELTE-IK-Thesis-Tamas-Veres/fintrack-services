using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_common.Providers
{
    public interface IDateTimeProvider
    {
        DateTime GetNow();
        DateTime GetUTCNow();
        DateTimeOffset GetUTCNowWithOffset();
    }
}
