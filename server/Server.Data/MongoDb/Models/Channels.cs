using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Server.Data.MongoDb.Models;

public class Channels
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("name")]
    [BsonRequired]
    public string Name { get; set; }

    [BsonElement("members")]
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> Members { get; set; } = new List<string>();

    [BsonElement("admin")]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonRequired]
    public string Admin { get; set; }

    [BsonElement("messages")]
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> Messages { get; set; } = new List<string>();

    [BsonElement("createAt")]
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updateAt")]
    public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

    public void UpdateTimestamp()
    {
        UpdateAt = DateTime.UtcNow;
    }
}