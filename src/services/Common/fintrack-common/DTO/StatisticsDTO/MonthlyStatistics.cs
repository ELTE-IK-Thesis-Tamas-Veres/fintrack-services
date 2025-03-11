using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_common.DTO.StatisticsDTO
{
    public class MonthlyStatistics
    {
        public string Month { get; set; } = "";
        public int Income { get; set; }
        public int Expense { get; set; }
    }
}
