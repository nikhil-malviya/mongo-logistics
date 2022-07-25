using Logistics.DataAccess.Models;
using Logistics.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Logistics.Service;

public class CityService : ICityService
{
	private readonly IMongoCollection<City> _collection;
	private readonly ILogger<CityService> _logger;

	public CityService(IMongoClient client, IConfiguration configuration, ILogger<CityService> logger)
	{
		var database = client.GetDatabase(configuration["Database"]);
		_collection = database.GetCollection<City>(DatabaseConstants.CityCollection);

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Get all cities
	/// </summary>
	/// <returns>List of cities</returns>
	public async Task<(string, IEnumerable<City>)> GetCities()
	{
		try
		{
			var results = await _collection.Find(_ => true).ToListAsync();

			return (string.Empty, results);
		}
		catch (MongoException ex)
		{
			_logger.LogError("Unable to fetch cities data", ex.Message);
			return (ex.Message, new List<City>());
		}
	}

	///	<summary>
	///	Get city by id
	///	</summary>
	///	<param name="cityId">Id of city</param>
	///	<returns>City</returns>
	public async Task<(string, City?)> GetCity(string cityId)
	{
		try
		{
			var result = await _collection.Find(city => city.Name == cityId).FirstOrDefaultAsync();

			return (string.Empty, result);
		}
		catch (MongoException ex)
		{
			_logger.LogError($"Unable to fetch city data for id: {cityId}", ex.Message);
			return (ex.Message, null);
		}
	}

	/// <summary>
	/// Get nearby cities using geo location coordinates
	/// </summary>
	/// <param name="cityId">Id of city</param>
	/// <param name="count">Number of nearby cities</param>
	public async Task<(string, NeighbouringCities?)> GetNeighbouringCities(string cityId, long count)
	{
		try
		{
			var neighbouringCities = new NeighbouringCities { Neighbors = new List<City>() };
			var (error, city) = await GetCity(cityId);

			if (city == null)
			{
				return ("Not Found", neighbouringCities);
			}

			var point = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(city.Location.X, city.Location.Y));

			var filter = Builders<City>.Filter.Near(x => x.Location, point);
			var nearbyCities = await _collection.Find(filter).ToListAsync();

			neighbouringCities.Neighbors = nearbyCities.Take((int)count);

			return (string.Empty, neighbouringCities);
		}
		catch (MongoException ex)
		{
			_logger.LogError($"Unable to fetch nearby cities for city: {cityId} count: {count}", ex.Message);
			return (ex.Message, null);
		}
	}

	/// <summary>
	/// Check if city exists or not
	/// </summary>
	/// <param name="cityId"></param>
	/// <returns>True if exits or false if not</returns>
	public async Task<bool> CityDoesNotExists(string cityId)
	{
		var (_error, result) = await GetCity(cityId);

		return result == null;
	}
}