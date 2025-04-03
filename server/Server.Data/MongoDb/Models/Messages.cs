using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Server.Data.MongoDb.Models;

public class Messages
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("sender")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Sender { get; set; }

    [BsonElement("recipient")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Recipient { get; set; }

    [BsonElement("messageType")]
    [BsonRequired]
    public string MessageType { get; set; } // "text" hoặc "file"

    [BsonElement("content")] public string Content { get; set; }

    [BsonElement("fileUrl")] public string FileUrl { get; set; }

    [BsonElement("timestamp")] public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class MessageDTO
{
    public string Id { get; set; }
    public string Sender { get; set; }
    public string Recipient { get; set; }
    public string MessageType { get; set; }
    public string Content { get; set; }
    public string FileUrl { get; set; }
    public DateTime Timestamp { get; set; }
    public CustomerDTO SenderCustomer { get; set; }
    public CustomerDTO RecipientCustomer { get; set; }
}

public class CustomerDTO
{
    public string? Id { get; set; }
    
    public string? Email { get; set; }
    
    public string? Password { get; set; }
    
    public string? FullName { get; set; }
    
    public string? UserId { get; set; }
    
    public string? Image { get; set; }
}
