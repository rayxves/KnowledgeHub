using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Models
{
    public class MediaVersion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("Url")]
        public string Url { get; set; } = string.Empty;
        [BsonElement("Type")]
        public MediaType Type { get; set; }
        [BsonElement("Description")]
        public string? Description { get; set; }
        [BsonElement("ArticleVersionId")]
        public string ArticleVersionId { get; set; }
        [BsonIgnore]
        public ArticleVersion ArticleVersion { get; set; } = null!;
    }

}