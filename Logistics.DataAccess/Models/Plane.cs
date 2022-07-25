using Logistics.DataAccess.Converters;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using System.Text.Json.Serialization;

namespace Logistics.DataAccess.Models
{
    [BsonIgnoreExtraElements]
    public class Plane
    {
        [BsonElement("_id")]
        public string Callsign { get; set; }

        [BsonElement("schemaType")]
        public string SchemaType { get; set; } = "plane";

        [BsonElement("SchemaVersion")]
        public string SchemaVersion { get; set; } = "1.0";

        [BsonElement("currentLocation")]
        [JsonConverter(typeof(GeoJson2DCoordinatesJsonConverter))]
        public GeoJson2DCoordinates CurrentLocation { get; set; }

        [BsonElement("heading")]
        public double Heading { get; set; }

        [BsonElement("route")]
        public string[] Route { get; set; }

        [BsonElement("landed")]
        public string Landed { get; set; }

        [BsonElement("previousLanded")]
        public string PreviousLanded { get; set; }

        [BsonElement("landedOn")]
        public DateTime LandedOn { get; set; }

        [BsonElement("statistics")]
        public PlaneStatistics Statistics { get; set; }
    }

    public class PlaneStatistics
    {
        [BsonElement("totalDistanceTravelledInMiles")]
        public double TotalDistanceTravelledInMiles { get; set; }

        [BsonElement("distanceTravelledSinceLastMaintenanceInMiles")]
        public double DistanceTravelledSinceLastMaintenanceInMiles { get; set; }

        [BsonElement("maintenanceRequired")]
        public bool MaintenanceRequired { get; set; }

        [BsonElement("airtime")]
        public double AirtimeInMinutes { get; set; }
    }
}