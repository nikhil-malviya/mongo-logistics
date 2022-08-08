using Logistics.DataAccess.Constants;
using Logistics.DataAccess.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace Logistics.API.Responses;

[BsonIgnoreExtraElements]
public class CityResponse
{
	[BsonElement(City.Id)]
	public string Name { get; set; }

	[BsonElement(City.Position)]
	public IEnumerable<double> Location { get; set; }

	[BsonElement(City.Country)]
	public string Country { get; set; }
}
