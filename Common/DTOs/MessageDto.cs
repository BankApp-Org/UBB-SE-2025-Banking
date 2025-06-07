using Common.Models;
using Common.Models.Bank;
using Common.Models.Social;
using System.Text.Json.Serialization;

namespace Common.DTOs
{
    //public class MessageDtoConverter : JsonConverter<MessageDto>
    //{
    //    public override bool CanConvert(Type typeToConvert)
    //    {
    //        return typeof(MessageDto).IsAssignableFrom(typeToConvert);
    //    }

    //    public override MessageDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        // Save reader position
    //        var readerAtStart = reader;

    //        // Read to find the type discriminator
    //        using var doc = JsonDocument.ParseValue(ref reader);
    //        var root = doc.RootElement;

    //        if (!root.TryGetProperty("type", out var typeProperty))
    //            throw new JsonException("Missing 'type' property");

    //        var typeValue = typeProperty.GetString();

    //        // Reset reader to start
    //        reader = readerAtStart;

    //        // Deserialize based on type
    //        MessageDto? result = typeValue switch
    //        {
    //            "Text" => JsonSerializer.Deserialize<TextMessageDto>(ref reader, options),
    //            "Image" => JsonSerializer.Deserialize<ImageMessageDto>(ref reader, options),
    //            "Transfer" => JsonSerializer.Deserialize<TransferMessageDto>(ref reader, options),
    //            "Request" => JsonSerializer.Deserialize<RequestMessageDto>(ref reader, options),
    //            "BillSplit" => JsonSerializer.Deserialize<BillSplitMessageDto>(ref reader, options),
    //            _ => throw new JsonException($"Unknown message type: {typeValue}")
    //        };

    //        return result ?? throw new JsonException("Failed to deserialize message");
    //    }

    //    public override void Write(Utf8JsonWriter writer, MessageDto value, JsonSerializerOptions options)
    //    {
    //        // Start the JSON object
    //        writer.WriteStartObject();

    //        // Write the discriminator property
    //        writer.WriteString("type", value.Type);

    //        // Get all properties from the concrete type and write them
    //        var properties = value.GetType().GetProperties();
    //        foreach (var prop in properties)
    //        {
    //            // Skip the Type property as we've already written it
    //            if (prop.Name == nameof(MessageDto.Type))
    //                continue;

    //            var propValue = prop.GetValue(value);
    //            if (propValue != null)
    //            {
    //                writer.WritePropertyName(JsonNamingPolicy.CamelCase.ConvertName(prop.Name));
    //                JsonSerializer.Serialize(writer, propValue, prop.PropertyType, options);
    //            }
    //        }

    //        // End the JSON object
    //        writer.WriteEndObject();
    //    }
    //}

    [JsonDerivedType(typeof(TextMessageDto), "Text")]
    [JsonDerivedType(typeof(ImageMessageDto), "Image")]
    [JsonDerivedType(typeof(TransferMessageDto), "Transfer")]
    [JsonDerivedType(typeof(RequestMessageDto), "Request")]
    [JsonDerivedType(typeof(BillSplitMessageDto), "BillSplit")]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
    public abstract class MessageDto(string type)
    {
        public string Type { get; set; } = type;
        public int MessageID { get; set; }
        public int SenderID { get; set; }
        public int ChatID { get; set; }
        public string? Timestamp { get; set; }
        // ???????
        public string? SenderUsername { get; set; }

    }
    public class TextMessageDto : MessageDto
    {
        public TextMessageDto() : base(MessageType.Text.ToString())
        {
        }

        public string? Content { get; set; }
        public List<User>? UsersReport { get; set; }
    }

    public class ImageMessageDto : MessageDto
    {
        public ImageMessageDto() : base(MessageType.Image.ToString())
        {
        }
        public string? ImageURL { get; set; }
        public List<User>? UsersReport { get; set; }
    }
    public class TransferMessageDto : MessageDto
    {
        public TransferMessageDto() : base(MessageType.Transfer.ToString())
        {
        }
        public string? Status { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public Currency Currency { get; set; }
        public List<User>? ListOfReceivers { get; set; }
    }
    public class RequestMessageDto : MessageDto
    {
        public RequestMessageDto() : base(MessageType.Request.ToString())
        {
        }
        public string? Status { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public Currency Currency { get; set; }
    }
    public class BillSplitMessageDto : MessageDto
    {
        public BillSplitMessageDto() : base(MessageType.BillSplit.ToString())
        {
        }
        public string? Description { get; set; }
        public decimal TotalAmount { get; set; }
        public Currency Currency { get; set; }
        public List<User>? Participants { get; set; }
        public string? Status { get; set; }
    }
    //public class MessageViewModel
    //{
    //    public int MessageID { get; set; }
    //    public int SenderID { get; set; }
    //    public int ChatID { get; set; }
    //    public string Timestamp { get; set; }
    //    // ???????
    //    public string SenderUsername { get; set; }
    //    public string MessageType { get; set; }
    //    public string? Content { get; set; }
    //    public List<int>? UsersReport { get; set; }
    //    public string? Status { get; set; }
    //    public float? Amount { get; set; }
    //    public string? Currency { get; set; }
    //    public List<int>? ListOfReceiversID { get; set; }
    //}
}