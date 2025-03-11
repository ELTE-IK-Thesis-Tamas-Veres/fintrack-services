using fintrack_common.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fintrack_common.DTO.ImportDTO
{
    public class ImportTransactionRequest
    {
        public List<ImportTransaction> Transactions { get; set; } = new List<ImportTransaction>();
    }

    public class ImportTransaction
    {
        [JsonConverter(typeof(DateTimeOffsetConverter))]
        public DateTimeOffset? TransactionDateTime { get; set; }

        [JsonConverter(typeof(DateTimeOffsetConverter))]
        public DateTimeOffset Booking { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public string? Note { get; set; } = "";
        public AmountModel Amount { get; set; } = new ();
    }

    public class AmountModel
    {
        public int Value { get; set; }
    }
}
