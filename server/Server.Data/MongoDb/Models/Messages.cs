using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Server.Data.MongoDb.Models;

public class Messages
{
    public class Message
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
}