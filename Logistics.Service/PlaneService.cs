using Logistics.DataAccess.Models;
using Logistics.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Logistics.Service;

public class PlaneService : IPlaneService
{
	private readonly IMongoCollection<Plane> _collection;
	private readonly ILogger<PlaneService> _logger;

	public PlaneService(IMongoClient client, IConfiguration configuration, ILogger<PlaneService> logger)
	{
		var database = client.GetDatabase(configuration["Database"]);
		_collection = database.GetCollection<Plane>(DatabaseConstants.PlaneCollection);

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Get all planes
	/// </summary>
	/// <returns>List of planes</returns>
	public async Task<(string, IEnumerable<Plane>)> GetPlanes()
	{
		try
		{
			var results = await _collection.Find(_ => true).ToListAsync();

			return (string.Empty, results);
		}
		catch (MongoException ex)
		{
			_logger.LogError("Unable to fetch planes data", ex.Message);
			return (ex.Message, new List<Plane>());
		}
	}

	///	<summary>
	///	Get plane by id
	///	</summary>
	///	<param name="planeId">Id of plane</param>
	///	<returns>Plane</returns>
	public async Task<(string, Plane?)> GetPlane(string planeId)
	{
		try
		{
			var result = await _collection.Find(plane => plane.Callsign == planeId).FirstOrDefaultAsync();

			return (string.Empty, result);
		}
		catch (MongoException ex)
		{
			_logger.LogError($"Unable to fetch plane data for id: {planeId}", ex.Message);
			return (ex.Message, null);
		}
	}

	/// <summary>
	/// Update plane location
	/// </summary>
	/// <param name="planeId">Id of plane</param>
	/// <param name="newLocation">New location of plane</param>
	/// <param name="heading">Angle in degrees</param>
	public async Task<(string, Plane?)> UpdatePlaneLocation(string planeId, GeoJson2DCoordinates newLocation, double heading)
	{
		try
		{
			var filter = Builders<Plane>.Filter.Eq(plane => plane.Callsign, planeId);
			var update = Builders<Plane>.Update
					.Set(plane => plane.CurrentLocation, newLocation)
					.Set(plane => plane.Heading, heading);

			var result = await _collection.FindOneAndUpdateAsync(filter, update);

			return (string.Empty, result);
		}
		catch (MongoException ex)
		{
			_logger.LogError($"Unable to update plane location for id: {planeId} latitude: {newLocation.X} longitude: {newLocation.Y} heading: {heading}", ex.Message);
			return (ex.Message, null);
		}
	}

	/// <summary>
	/// Update plane location
	/// </summary>
	/// <param name="planeId">Id of plane</param>
	/// <param name="newLocation">New location of plane</param>
	/// <param name="heading">Angle in degrees</param>
	/// <param name="cityId">Id of city</param>
	public async Task<(string, Plane?)> LandPlane(string planeId, GeoJson2DCoordinates newLocation, double heading, string cityId)
	{
		try
		{
			var filter = Builders<Plane>.Filter.Eq(plane => plane.Callsign, planeId);
			var update = Builders<Plane>.Update
					.Set(plane => plane.CurrentLocation, newLocation)
					.Set(plane => plane.Heading, heading)
					.Set(plane => plane.Landed, cityId)
					.Set(plane => plane.LandedOn, DateTime.UtcNow);

			var result = await _collection.FindOneAndUpdateAsync(filter, update);

			return (string.Empty, result);
		}
		catch (MongoException ex)
		{
			_logger.LogError($"Unable to land plane id: {planeId} latitude: {newLocation.X} longitude: {newLocation.Y} heading: {heading} city {cityId}", ex.Message);
			return (ex.Message, null);
		}
	}

	/// <summary>
	/// Remove first route from plane route list
	/// </summary>
	/// <param name="planeId">Id of plane</param>
	public async Task<(string, bool)> RemoveDestination(string planeId)
	{
		try
		{
			var filter = Builders<Plane>.Filter.Eq(plane => plane.Callsign, planeId);
			var plane = await _collection.Find(filter).FirstOrDefaultAsync();

			var previouslyLanded = string.Empty;

			if (plane?.Route?.Length > 0)
			{
				previouslyLanded = plane.Route.First();
			}

			var update = Builders<Plane>.Update
					.Set(plane => plane.PreviousLanded, previouslyLanded)
					.PopFirst(p => p.Route);

			var result = await _collection.UpdateOneAsync(filter, update);

			return (string.Empty, result.IsAcknowledged && result.ModifiedCount == 1);
		}
		catch (MongoException ex)
		{
			_logger.LogError($"Unable to remove destination from route for id: {planeId}", ex.Message);
			return (ex.Message, false);
		}
	}

	/// <summary>
	/// Replace route list with new route
	/// </summary>
	/// <param name="planeId">Id of plane</param>
	/// <param name="cityId">Id of city</param>
	public async Task<(string, bool)> ReplaceDestination(string planeId, string cityId)
	{
		try
		{
			var filter = Builders<Plane>.Filter.Eq(plane => plane.Callsign, planeId);
			var update = Builders<Plane>.Update
					.Set(plane => plane.Route, new string[] { cityId });

			var result = await _collection.UpdateOneAsync(filter, update);

			return (string.Empty, result.IsAcknowledged && result.ModifiedCount == 1);
		}
		catch (MongoException ex)
		{
			_logger.LogError($"Unable to replace destination for id: {planeId}", ex.Message);
			return (ex.Message, false);
		}
	}

	/// <summary>
	/// Add route to plane route list
	/// </summary>
	/// <param name="planeId">Id of plane</param>
	/// <param name="cityId">Id of city</param>
	public async Task<(string, bool)> UpdateDestination(string planeId, string cityId)
	{
		try
		{
			var filter = Builders<Plane>.Filter.Eq(plane => plane.Callsign, planeId);
			var update = Builders<Plane>.Update
					.AddToSet(plane => plane.Route, cityId);

			var result = await _collection.UpdateOneAsync(filter, update);

			return (string.Empty, result.IsAcknowledged && result.ModifiedCount == 1);
		}
		catch (MongoException ex)
		{
			_logger.LogError($"Unable to add city: {cityId} for id: {planeId}", ex.Message);
			return (ex.Message, false);
		}
	}

	/// <summary>
	/// Check if plane exists or not
	/// </summary>
	/// <param name="planeId"></param>
	/// <returns>True if exits or false if not</returns>
	public async Task<bool> PlaneDoesNotExists(string cityId)
	{
		var (_error, result) = await GetPlane(cityId);

		return result == null;
	}
}