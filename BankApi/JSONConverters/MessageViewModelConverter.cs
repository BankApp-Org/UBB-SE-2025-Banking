using Common.DTOs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankApi.JSONConverters
{
    /// <summary>
    /// USED ONLY FOR SERIALIZING!!!
    /// </summary>
    public class MessageDtoConverter : JsonConverter<MessageDto>
    {
        public override MessageDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            if (!root.TryGetProperty("messageType", out var typeProp) || typeProp.GetString() is not { } typeString)
            {
                throw new JsonException("The 'messageType' property is missing or null.");
            }

            MessageDto result = typeString switch
            {
                "Text" => JsonSerializer.Deserialize<TextMessageDto>(root.GetRawText(), options),
                "Image" => JsonSerializer.Deserialize<ImageMessageDto>(root.GetRawText(), options),
                "Transfer" => JsonSerializer.Deserialize<TransferMessageDto>(root.GetRawText(), options),
                "Request" => JsonSerializer.Deserialize<RequestMessageDto>(root.GetRawText(), options),
                _ => throw new JsonException($"Unknown messageType: {typeString}. Expected one of: Text, Image, Transfer, Request.")
            };

            return result;
        }

        public override void Write(Utf8JsonWriter writer, MessageDto value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
