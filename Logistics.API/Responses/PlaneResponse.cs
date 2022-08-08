using Logistics.DataAccess.Constants;
using MongoDB.Bson.Serialization.Attributes;

namespace Logistics.API.Responses;

[BsonIgnoreExtraElements]
public class PlaneResponse
{
	[BsonElement(Plane.Id)]
	public string Callsign { get; set; }

	[BsonElement(Plane.CurrentLocation)]
	public List<double> CurrentLocation { get; set; }

	[BsonElement(Plane.Heading)]
	public decimal Heading { get; set; }

	[BsonElement(Plane.Route)]
	public List<string> Route { get; set; }

	[BsonElement(Plane.Landed)]
	public string Landed { get; set; }
}
