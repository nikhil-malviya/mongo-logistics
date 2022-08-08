using Logistics.DataAccess.Constants;
using Logistics.DataAccess.Models;
using Logistics.DataAccess.MongoDB;
using Logistics.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Optional;

namespace Logistics.Service;

public class CityService : ICityService
{
	private readonly IMongoCollection<BsonDocument> _collection;
	private readonly ILogger<CityService> _logger;
	private readonly ProjectionDefinition<BsonDocument> _defaultProjection;

	public CityService(IMongoClient client, IConfiguration configuration, ILogger<CityService> logger)
	{
		var database = client.GetDatabase(configuration["Database"])
												.WithWriteConcern(WriteConcern.WMajority)
												.WithReadConcern(ReadConcern.Majority)
												.WithReadPreference(ReadPreference.Secondary);

		_collection = database.GetCollection<BsonDocument>(Database.CityCollection);
		_defaultProjection = Builders<BsonDocument>.Projection
								.Include(City.Id)
								.Include(City.Country)
								.Include(City.Position);

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Get all cities
	/// </summary>
	/// <returns>List of cities</returns>
	public async Task<Option<IEnumerable<BsonDocument>, Error>> GetCities(ProjectionDefinition<BsonDocument> projection = default)
	{
		try
		{
			var cities = await Query.GetAllAsync(_collection, projection ?? _defaultProjection).ConfigureAwait(false);

			return Option.Some<IEnumerable<BsonDocument>, Error>(cities);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError("Unable to fetch cities data", error);
			return Option.None<IEnumerable<BsonDocument>, Error>(error);
		}
	}

	///	<summary>
	///	Get city by id
	///	</summary>
	///	<param name="cityId">Id of city</param>
	///	<returns>City</returns>
	public async Task<Option<BsonDocument?, Error>> GetCity(string cityId, ProjectionDefinition<BsonDocument> projection = default)
	{
		try
		{
			var filter = Builders<BsonDocument>.Filter.Eq(City.Id, cityId);

			var city = await Query.FindOneAsync(_collection, filter, projection ?? _defaultProjection).ConfigureAwait(false);

			return Option.Some<BsonDocument?, Error>(city);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to fetch city data for id: {cityId}", error);
			return Option.None<BsonDocument?, Error>(error);
		}
	}

	/// <summary>
	/// Get nearby cities using geo location coordinates
	/// </summary>
	/// <param name="cityId">Id of city</param>
	/// <param name="count">Number of nearby cities</param>
	public async Task<Option<IEnumerable<BsonDocument>, Error>> GetNeighbouringCities(string cityId, int count, ProjectionDefinition<BsonDocument> projection = default)
	{
		try
		{
			var filter = Builders<BsonDocument>.Filter.Eq(City.Id, cityId);
			var positionOnly = Builders<BsonDocument>.Projection.Include(City.Position);
			var document = await Query.FindOneAsync(_collection, filter, positionOnly).ConfigureAwait(false);

			if (document == null)
			{
				var error = new Error(ErrorCode.NotFound, $"City not found for id: {cityId}");
				_logger.LogError($"City not found for id: {cityId}");
				return Option.None<IEnumerable<BsonDocument>, Error>(error);
			}

			var city = new CityDocument(document);

			var point = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(city.Position.Longitude, city.Position.Latitude));
			var geoNearFilter = Builders<BsonDocument>.Filter.Near(City.Position, point);

			var nearbyCities = await Query.FindAsync(_collection, geoNearFilter, projection ?? _defaultProjection, count).ConfigureAwait(false);

			return Option.Some<IEnumerable<BsonDocument>, Error>(nearbyCities);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to fetch {count} nearby cities for city having id: {cityId}", error);
			return Option.None<IEnumerable<BsonDocument>, Error>(error);
		}
	}

	/// <summary>
	/// Check if city exists or not
	/// </summary>
	/// <param name="cityId"></param>
	/// <returns>True if exits or false if not</returns>
	public async Task<Option<bool, Error>> CityExists(string cityId)
	{
		try
		{
			var filter = Builders<BsonDocument>.Filter.Eq(City.Id, cityId);

			var exists = await Query.DocumentExistsAsync(_collection, filter).ConfigureAwait(false);

			return Option.Some<bool, Error>(exists);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to fetch city data for id: {cityId}", error);
			return Option.None<bool, Error>(error);
		}
	}
}
