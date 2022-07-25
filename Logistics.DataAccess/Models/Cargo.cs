using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Logistics.DataAccess.Models
{
    [BsonIgnoreExtraElements]
    public class Cargo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("schemaType")]
        public string SchemaType { get; set; } = "cargo";

        [BsonElement("schemaVersion")]
        public string SchemaVersion { get; set; } = "1.0";

        [BsonElement("courier")]
        public string Courier { get; set; }

        [BsonElement("received")]
        public DateTime Received { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }

        [BsonElement("location")]
        public string Location { get; set; }

        [BsonElement("destination")]
        public string Destination { get; set; }
    }
}