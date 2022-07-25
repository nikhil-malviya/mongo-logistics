using Logistics.DataAccess.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Logistics.DataAccess.Initialize
{
	public static class Initialize
	{
		public static async Task CreateIndexes(IMongoClient client, IConfiguration configuration)
		{
			var database = client.GetDatabase(configuration["Database"]);

			var planes = database.GetCollection<Plane>(DatabaseConstants.PlaneCollection);
			var currentLocationIndex = Builders<Plane>.IndexKeys.Geo2DSphere(plane => plane.CurrentLocation);
			await planes.Indexes.CreateOneAsync(new CreateIndexModel<Plane>(currentLocationIndex));

			var cities = database.GetCollection<City>(DatabaseConstants.CityCollection);
			var locationIndex = Builders<City>.IndexKeys.Geo2DSphere(city => city.Location);
			await cities.Indexes.CreateOneAsync(new CreateIndexModel<City>(locationIndex));
		}
	}
}
