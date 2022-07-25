using Logistics.DataAccess.Converters;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using System.Text.Json.Serialization;

namespace Logistics.DataAccess.Models
{
    [BsonIgnoreExtraElements]
    public class City
    {
        [BsonElement("_id")]
        public string Name { get; set; }

        [BsonElement("schemaType")]
        public string SchemaType { get; set; } = "city";

        [BsonElement("SchemaVersion")]
        public string SchemaVersion { get; set; } = "1.0";

        [BsonElement("country")]
        public string Country { get; set; }

        [BsonElement("position")]
        [JsonConverter(typeof(GeoJson2DCoordinatesJsonConverter))]
        public GeoJson2DCoordinates Location { get; set; }
    }
}