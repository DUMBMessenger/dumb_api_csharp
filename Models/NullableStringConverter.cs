using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dumb_api_csharp
{
    /// <summary>
    /// Custom JSON converter that handles nullable values that might come as numbers, null, strings, or booleans
    /// </summary>
    public class NullableStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                
                case JsonTokenType.String:
                    return reader.GetString();
                
                case JsonTokenType.Number:
                    // If it's a number, convert it to string
                    if (reader.TryGetInt64(out long longValue))
                    {
                        return longValue.ToString();
                    }
                    if (reader.TryGetDouble(out double doubleValue))
                    {
                        return doubleValue.ToString();
                    }
                    return reader.GetString();
                
                case JsonTokenType.True:
                    return "true";
                
                case JsonTokenType.False:
                    return "false";
                
                default:
                    throw new JsonException($"Unexpected token type '{reader.TokenType}' when parsing nullable string.");
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value);
            }
        }
    }
}
