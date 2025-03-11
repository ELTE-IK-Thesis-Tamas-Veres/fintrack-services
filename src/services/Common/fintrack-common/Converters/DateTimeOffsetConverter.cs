using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace fintrack_common.Converters
{
    public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string dateString = reader.GetString() ?? throw new JsonException();

            // Fix missing colon in offset (e.g., "+0100" -> "+01:00")
            if (dateString.Length > 5 && (dateString[^5] == '+' || dateString[^5] == '-'))
            {
                dateString = dateString.Insert(dateString.Length - 2, ":");
            }

            return DateTimeOffset.ParseExact(dateString, "yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"));
        }
    }
}
