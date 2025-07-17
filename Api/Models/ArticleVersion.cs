using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Models
{
    public class ArticleVersion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [BsonElement("Title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("ArticleId")]
        public Guid ArticleId { get; set; }

        [BsonElement("VersionNumber")]
        public int VersionNumber { get; set; }

        [BsonElement("ContentMarkdown")]
        public string ContentMarkdown { get; set; } = string.Empty;

        [BsonElement("ContentHtmlSanitized")]
        public string? ContentHtmlSanitized { get; set; }

        [BsonElement("EditedAt")]
        public DateTime EditedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("EditedByUserId")]
        public string? EditedByUserId { get; set; }

        [BsonElement("MediaItems")]
        public ICollection<MediaVersion>? MediaItems { get; set; }
    }
}
