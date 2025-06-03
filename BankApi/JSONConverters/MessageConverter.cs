using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using LoanShark.Domain.Enums;
using LoanShark.Domain.MessageClasses;

namespace LoanShark.API.JSONConverters
{
    public class MessageConverter : JsonConverter<Message>
    {
        public override Message Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            if (!root.TryGetProperty("messageType", out var typeProp))
            {
                throw new JsonException("messageType property is missing.");
            }

            var messageType = Enum.Parse<MessageType>(typeProp.GetString());
            var text = root.GetRawText();

            return messageType switch
            {
                MessageType.Text => JsonSerializer.Deserialize<TextMessage>(root.GetRawText(), options),
                MessageType.Image => JsonSerializer.Deserialize<ImageMessage>(root.GetRawText(), options),
                MessageType.Request => JsonSerializer.Deserialize<RequestMessage>(root.GetRawText(), options),
                MessageType.Transfer => JsonSerializer.Deserialize<TransferMessage>(root.GetRawText(), options),
                _ => throw new JsonException($"Unknown MessageType: {messageType}")
            };
        }

        public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
