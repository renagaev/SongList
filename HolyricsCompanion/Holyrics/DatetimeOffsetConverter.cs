using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HolyricsCompanion.Holyrics;

public class DatetimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(DateTimeOffset));
        return DateTimeOffset.Parse(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}