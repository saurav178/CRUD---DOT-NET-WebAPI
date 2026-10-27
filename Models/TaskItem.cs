using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskApi.Models;

public class TaskItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("isCompleted")]
    public bool IsCompleted { get; set; } = false;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("dueDate")]
    public DateTime? DueDate { get; set; }
}
