using Logistics.DataAccess.Constants;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Logistics.DataAccess.MongoDB;

public static class Initialize
{
	public static async Task CreateIndexes(IMongoClient client, IConfiguration configuration)
	{
		var database = client.GetDatabase(configuration["Database"]);

		var planes = database.GetCollection<BsonDocument>(Database.PlaneCollection);
		var currentLocationIndex = Builders<BsonDocument>.IndexKeys.Geo2DSphere(Plane.CurrentLocation);
		await planes.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(currentLocationIndex));

		var cities = database.GetCollection<BsonDocument>(Database.CityCollection);
		var locationIndex = Builders<BsonDocument>.IndexKeys.Geo2DSphere(City.Position);
		await cities.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(locationIndex));
	}
}
