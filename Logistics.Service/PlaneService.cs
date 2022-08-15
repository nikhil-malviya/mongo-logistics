using Logistics.DataAccess.Constants;
using Logistics.DataAccess.Models;
using Logistics.DataAccess.MongoDB;
using Logistics.DataAccess.Utilities;
using Logistics.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Optional;

namespace Logistics.Service;

public class PlaneService : IPlaneService
{
	private readonly IMongoCollection<BsonDocument> _collection;
	private readonly ILogger<PlaneService> _logger;
	private readonly ProjectionDefinition<BsonDocument> _defaultProjection;

	public PlaneService(IMongoClient client, IConfiguration configuration, ILogger<PlaneService> logger)
	{
		var database = client.GetDatabase(configuration["Database"])
														.WithWriteConcern(WriteConcern.WMajority)
														.WithReadConcern(ReadConcern.Majority);

		_collection = database.GetCollection<BsonDocument>(Database.PlaneCollection);
		_defaultProjection = Builders<BsonDocument>.Projection
						.Include(Plane.Id)
						.Include(Plane.Heading)
						.Include(Plane.CurrentLocation)
						.Include(Plane.Route)
						.Include(Plane.Landed);

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Get all planes
	/// </summary>
	/// <returns>List of planes</returns>
	public async Task<Option<IEnumerable<BsonDocument>, Error>> GetPlanes(ProjectionDefinition<BsonDocument> projection = default)
	{
		try
		{
			var planes = await Query.GetAllAsync(_collection, projection ?? _defaultProjection).ConfigureAwait(false);

			return Option.Some<IEnumerable<BsonDocument>, Error>(planes);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError("Unable to fetch planes data", error);
			return Option.None<IEnumerable<BsonDocument>, Error>(error);
		}
	}

	///	<summary>
	///	Get plane by id
	///	</summary>
	///	<param name="planeId">Id of plane</param>
	///	<returns>Plane</returns>
	public async Task<Option<BsonDocument?, Error>> GetPlane(string planeId, ProjectionDefinition<BsonDocument> projection = default)
	{
		try
		{
			var filter = Builders<BsonDocument>.Filter.Eq(Plane.Id, planeId);

			var plane = await Query.FindOneAsync(_collection, filter, projection ?? _defaultProjection).ConfigureAwait(false);

			return Option.Some<BsonDocument?, Error>(plane);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to fetch plane data for id: {planeId}", error);
			return Option.None<BsonDocument?, Error>(error);
		}
	}

	/// <summary>
	/// Update plane location
	/// </summary>
	/// <param name="planeId">Id of plane</param>
	/// <param name="newLocation">New location of plane</param>
	/// <param name="heading">Angle in degrees</param>
	public async Task<Option<BsonDocument?, Error>> UpdatePlaneLocation(string planeId, GeoJson2DGeographicCoordinates newLocation, double heading)
	{
		try
		{
			var filter = Builders<BsonDocument>.Filter.Eq(Plane.Id, planeId);
			var update = Builders<BsonDocument>.Update
					.Set(Plane.CurrentLocation, newLocation.ToBsonArray())
					.Set(Plane.Heading, heading);

			var result = await Command.FindOneAndUpdateAsync(_collection, filter, update);

			// First time the plane takes off, set the departed time
			if (result.GetValueAsDateTime(Plane.LandedOn) == null)
			{
				var updateDeparted = Builders<BsonDocument>.Update
					.Set(Plane.LandedOn, DateTime.UtcNow);

				await Command.UpdateOneAsync(_collection, filter, updateDeparted);
			}

			return Option.Some<BsonDocument?, Error>(result);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to update plane location for id: {planeId} latitude: {newLocation.Latitude} longitude: {newLocation.Longitude} heading: {heading}", ex.Message);
			return Option.None<BsonDocument?, Error>(error);
		}
	}

	/// <summary>
	/// Update plane location
	/// </summary>
	/// <param name="planeId">Id of plane</param>
	/// <param name="newLocation">New location of plane</param>
	/// <param name="heading">Angle in degrees</param>
	/// <param name="cityId">Id of city</param>
	public async Task<Option<BsonDocument?, Error>> LandPlane(string planeId, GeoJson2DGeographicCoordinates newLocation, double heading, string cityId)
	{
		try
		{
			var filter = Builders<BsonDocument>.Filter.Eq(Plane.Id, planeId);
			var update = Builders<BsonDocument>.Update
					.Set(Plane.CurrentLocation, newLocation.ToBsonArray())
					.Set(Plane.Heading, heading)
					.Set(Plane.Landed, cityId)
					.Set(Plane.LandedOn, DateTime.UtcNow);

			var result = await Command.FindOneAndUpdateAsync(_collection, filter, update);

			return Option.Some<BsonDocument?, Error>(result);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to land plane id: {planeId} latitude: {newLocation.Latitude} longitude: {newLocation.Longitude} heading: {heading} city {cityId}", ex.Message);
			return Option.None<BsonDocument?, Error>(error);
		}
	}

	/// <summary>
	/// Remove first route from plane route list
	/// </summary>
	/// <param name="planeId">Id of plane</param>
	public async Task<Option<bool, Error>> RemoveDestination(string planeId)
	{
		try
		{
			var filter = Builders<BsonDocument>.Filter.Eq(Plane.Id, planeId);
			var routeOnly = Builders<BsonDocument>.Projection.Include(Plane.Route);
			var document = await Query.FindOneAsync(_collection, filter, routeOnly).ConfigureAwait(false);

			// Remove first route from route list and set it as departed city
			var update = Builders<BsonDocument>.Update
					.PopFirst(Plane.Route);

			var result = await Command.UpdateOneAsync(_collection, filter, update);

			return Option.Some<bool, Error>(result.IsAcknowledged && result.ModifiedCount == 1);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to remove destination from route for id: {planeId}", ex.Message);
			return Option.None<bool, Error>(error);
		}
	}

	/// <summary>
	/// Replace route list with new route
	/// </summary>
	/// <param name="planeId">Id of plane</param>
	/// <param name="cityId">Id of city</param>
	public async Task<Option<bool, Error>> ReplaceDestination(string planeId, string cityId)
	{
		try
		{
			var filter = Builders<BsonDocument>.Filter.Eq(Plane.Id, planeId);
			var update = Builders<BsonDocument>.Update
					.Set(Plane.Route, new string[] { cityId });

			var result = await Command.UpdateOneAsync(_collection, filter, update);

			return Option.Some<bool, Error>(result.IsAcknowledged && result.ModifiedCount == 1);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to replace destination for id: {planeId}", ex.Message);
			return Option.None<bool, Error>(error);
		}
	}

	/// <summary>
	/// Add route to plane route list
	/// </summary>
	/// <param name="planeId">Id of plane</param>
	/// <param name="cityId">Id of city</param>
	public async Task<Option<bool, Error>> UpdateDestination(string planeId, string cityId)
	{
		try
		{
			var filter = Builders<BsonDocument>.Filter.Eq(Plane.Id, planeId);
			var update = Builders<BsonDocument>.Update
					.AddToSet(Plane.Route, cityId);

			var result = await Command.UpdateOneAsync(_collection, filter, update);

			return Option.Some<bool, Error>(result.IsAcknowledged && result.ModifiedCount == 1);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to add city: {cityId} for id: {planeId}", ex.Message);
			return Option.None<bool, Error>(error);
		}
	}

	/// <summary>
	/// Check if plane exists or not
	/// </summary>
	/// <param name="planeId"></param>
	/// <returns>True if exits or false if not</returns>
	///
	public async Task<Option<bool, Error>> PlaneExists(string planeId)
	{
		try
		{
			var filter = Builders<BsonDocument>.Filter.Eq(Plane.Id, planeId);

			var exists = await Query.DocumentExistsAsync(_collection, filter).ConfigureAwait(false);

			return Option.Some<bool, Error>(exists);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to fetch city data for id: {planeId}", error);
			return Option.None<bool, Error>(error);
		}
	}
}
