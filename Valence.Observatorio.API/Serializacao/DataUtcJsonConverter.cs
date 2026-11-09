using System.Text.Json;
using System.Text.Json.Serialization;

namespace Valence.Observatorio.API.Serializacao
{
    public class DataUtcJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => ParaUtc(reader.GetDateTime());

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(ParaUtc(value));

        private static DateTime ParaUtc(DateTime data) => data.Kind switch
        {
            DateTimeKind.Utc => data,
            DateTimeKind.Local => data.ToUniversalTime(),
            _ => DateTime.SpecifyKind(data, DateTimeKind.Utc)
        };
    }
}
