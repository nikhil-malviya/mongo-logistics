using Logistics.DataAccess.Constants;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Logistics.API.Responses;

[BsonIgnoreExtraElements]
public class CargoResponse
{
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; set; }

	[BsonElement(Cargo.Destination)]
	public string Destination { get; set; }

	[BsonElement(Cargo.Location)]
	public string Location { get; set; }

	[BsonElement(Cargo.Courier)]
	public string Courier { get; set; }

	[BsonElement(Cargo.Received)]
	public DateTime Received { get; set; }

	[BsonElement(Cargo.Status)]
	public string Status { get; set; }
}
