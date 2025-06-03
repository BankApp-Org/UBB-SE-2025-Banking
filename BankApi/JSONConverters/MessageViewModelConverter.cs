using LoanShark.API.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LoanShark.API.JSONConverters
{
    /// <summary>
    /// USED ONLY FOR SERIALIZING!!!
    /// </summary>
    public class MessageViewModelConverter : JsonConverter<MessageViewModel>
    {
        public override MessageViewModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            if (!root.TryGetProperty("messageType", out var typeProp) || typeProp.GetString() is not { } typeString)
            {
                throw new JsonException("The 'messageType' property is missing or null.");
            }

            MessageViewModel result = typeString switch
            {
                "Text" => JsonSerializer.Deserialize<TextMessageViewModel>(root.GetRawText(), options),
                "Image" => JsonSerializer.Deserialize<ImageMessageViewModel>(root.GetRawText(), options),
                "Transfer" => JsonSerializer.Deserialize<TransferMessageViewModel>(root.GetRawText(), options),
                "Request" => JsonSerializer.Deserialize<RequestMessageViewModel>(root.GetRawText(), options),
                _ => throw new JsonException($"Unknown messageType: {typeString}. Expected one of: Text, Image, Transfer, Request.")
            };

            return result;
        }

        public override void Write(Utf8JsonWriter writer, MessageViewModel value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
