using Logistics.DataAccess.Constants;
using Logistics.DataAccess.Models;
using Logistics.DataAccess.Utilities;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Logistics.DataAccess.MongoDB;

public static class ChangeStream
{
	private const string Statistics = "statistics";
	private static object locker = new object();

	public static async Task Monitor(IMongoClient client, IConfiguration configuration)
	{
		var database = client.GetDatabase(configuration["Database"])
										.WithWriteConcern(WriteConcern.WMajority)
										.WithReadConcern(ReadConcern.Majority);

		var planesCollection = database.GetCollection<BsonDocument>(Database.PlaneCollection);
		var citiesCollection = database.GetCollection<BsonDocument>(Database.CityCollection);

		try
		{
			var options = new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup, };
			var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>()
											.Match(
												"{ operationType: { $in: [ 'update'] }, 'updateDescription.updatedFields.landed' : { $exists: true } }"
											);


			using (var cursor = await planesCollection.WatchAsync(pipeline, options))
			{
				await cursor.ForEachAsync(change =>
				{
					var plane = new PlaneDocument(change.FullDocument);

					if (!string.IsNullOrWhiteSpace(plane.Departed) && !string.IsNullOrWhiteSpace(plane.Landed))
					{
						var cityFilter = Builders<BsonDocument>.Filter.In(City.Id, new string[] { plane.Departed, plane.Landed });

						var travelledCities = citiesCollection.Find(cityFilter).ToList();

						var planeFilter = Builders<BsonDocument>.Filter.Eq(Plane.Id, plane.Id);
						var update = Builders<BsonDocument>.Update
													.Set(Plane.Departed, plane.Landed)
													.Set(Statistics, GetPlaneStatistics(plane, travelledCities));

						planesCollection.UpdateOne(planeFilter, update);
					}
				});
			}
		}
		catch (MongoException ex)
		{
			Console.WriteLine(ex.Message);
		}
	}


	public static BsonDocument GetPlaneStatistics(PlaneDocument plane, List<BsonDocument> travelledCities)
	{
		lock (locker)
		{
			if (travelledCities.Count != 2)
			{
				return new BsonDocument();
			}

			var city1Location = travelledCities[0].GetValueAsCoordinates(City.Position);
			var city2Location = travelledCities[1].GetValueAsCoordinates(City.Position);

			var travelledDistance = GetDistance(city1Location.Latitude, city1Location.Longitude, city2Location.Latitude, city2Location.Longitude);

			double timeTaken = 0;
			if (plane.LandedOn.HasValue)
			{
				timeTaken = DateTime.UtcNow.Subtract(plane.LandedOn.Value).TotalMinutes;
			}

			// Check if maintenance is required
			double distanceTravelledSinceLastMaintenance = 0;
			bool maintenanceRequired = false;

			if (!plane.MaintenanceRequired)
			{
				distanceTravelledSinceLastMaintenance = plane.DistanceTravelledSinceLastMaintenanceInMiles + travelledDistance;
				maintenanceRequired = distanceTravelledSinceLastMaintenance > 50000;
			}

			return new BsonDocument
			{
				{ Plane.DistanceTravelledSinceLastMaintenanceInMiles.TrimPrefix(), distanceTravelledSinceLastMaintenance },
				{ Plane.MaintenanceRequired.TrimPrefix(), maintenanceRequired },
				{ Plane.TotalDistanceTravelledInMiles.TrimPrefix(), plane.TotalDistanceTravelledInMiles + travelledDistance},
				{ Plane.AirtimeInMinutes.TrimPrefix(), plane.AirtimeInMinutes + timeTaken}
			};
		}
	}

	// Get distance between two points on earth in miles using the Haversine formula
	public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
	{
		var r = 3959.87433; // In miles
		var dLat = ToRadian(lat2 - lat1);
		var dLon = ToRadian(lon2 - lon1);
		var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
		var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));

		var distance = r * c;

		return distance;
	}

	public static double ToRadian(double deg)
	{
		return deg * (Math.PI / 180);
	}

	private static string TrimPrefix(this string field)
	{
		return field.Replace($"{Statistics}.", string.Empty);
	}
}
