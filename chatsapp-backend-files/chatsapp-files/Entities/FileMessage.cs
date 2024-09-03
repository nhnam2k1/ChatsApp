using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class FileMessage{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonRepresentation(BsonType.Binary)]
    public byte[] File { get; set; } = Array.Empty<byte>();
}