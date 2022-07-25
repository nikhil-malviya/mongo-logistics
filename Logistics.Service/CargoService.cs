using Logistics.DataAccess.Constants;
using Logistics.DataAccess.Models;
using Logistics.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Logistics.Service;

public class CargoService : ICargoService
{
	private readonly IMongoCollection<Cargo> _collection;
	private readonly ILogger<CargoService> _logger;

	public CargoService(IMongoClient client, IConfiguration configuration, ILogger<CargoService> logger)
	{
		var database = client.GetDatabase(configuration["Database"]);
		_collection = database.GetCollection<Cargo>(DatabaseConstants.CargoCollection);

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}


	/// <summary>
	/// Create a cargo with location to destination
	/// </summary>
	/// <param name="location"></param>
	/// <param name="destination"></param>
	public async Task<(string, Cargo?)> AddCargo(string location, string destination)
	{
		try
		{
			var cargo = new Cargo
			{
				Received = DateTime.UtcNow,
				Location = location,
				Destination = destination,
				Status = CargoStatus.InProcess,
			};

			await _collection.InsertOneAsync(cargo);

			return (String.Empty, cargo);
		}
		catch (MongoException ex)
		{
			_logger.LogError($"Unable to create create cargo from {location} to {destination}", ex.Message);
			return (ex.Message, null);
		}
	}

	/// <summary>
	/// Fetches list of InProcess cargos for a location
	/// </summary>
	/// <param name="location"></param>
	/// <returns></returns>
	public async Task<(string, IEnumerable<Cargo>)> GetCargos(string location)
	{
		try
		{
			var builder = Builders<Cargo>.Filter;
			var filter = builder.Empty;

			filter &= builder.Eq(cargo => cargo.Location, location);
			filter &= builder.Eq(cargo => cargo.Status, CargoStatus.InProcess);

			var result = await _collection.FindAsync(filter);

			return (String.Empty, result.ToList());
		}
		catch (MongoException ex)
		{
			_logger.LogError("Unable to fetch cargo data", ex.Message);
			return (ex.Message, new List<Cargo>());
		}
	}

	/// <summary>
	/// Updates cargo courier to null
	/// </summary>
	/// <param name="cargoId"></param>
	public async Task<(string, Cargo?)> UnloadCargo(string cargoId)
	{
		try
		{
			var filter = Builders<Cargo>.Filter.Eq(cargo => cargo.Id, cargoId);
			var update = Builders<Cargo>.Update
					.Set(cargo => cargo.Courier, null);

			var result = await _collection.FindOneAndUpdateAsync(filter, update);

			return (string.Empty, result);
		}
		catch (MongoException ex)
		{
			_logger.LogError(ex.Message);
			return (ex.Message, null);
		}
	}

	/// <summary>
	/// Set cargo status as delivered
	/// </summary>
	/// <param name="cargoId"></param>
	/// <returns></returns>
	public async Task<(string, bool)> DeliverCargo(string cargoId)
	{
		try
		{
			var filter = Builders<Cargo>.Filter.Eq(cargo => cargo.Id, cargoId);
			var update = Builders<Cargo>.Update
					.Set(cargo => cargo.Status, CargoStatus.Delivered);

			var result = await _collection.UpdateOneAsync(filter, update);

			return (string.Empty, result.IsAcknowledged && result.ModifiedCount == 1);
		}
		catch (MongoException ex)
		{
			_logger.LogError($"Unable to set cargo status as delivered for {cargoId}", ex.Message);
			return (ex.Message, false);
		}
	}

	/// <summary>
	/// Set courier for cargo
	/// </summary>
	/// <param name="cargoId"></param>
	/// <param name="planeId"></param>
	public async Task<(string, bool)> UpdateCargo(string cargoId, string courierId)
	{
		try
		{
			var filter = Builders<Cargo>.Filter.Eq(cargo => cargo.Id, cargoId);
			var update = Builders<Cargo>.Update
					.Set(cargo => cargo.Courier, courierId);

			var result = await _collection.UpdateOneAsync(filter, update);

			return (string.Empty, result.IsAcknowledged && result.ModifiedCount == 1);
		}
		catch (MongoException ex)
		{
			_logger.LogError($"Unable to set courier {courierId} on cargo {cargoId}", ex.Message);
			return (ex.Message, false);
		}
	}

	/// <summary>
	/// Move cargo from plane to city and vice versa
	/// </summary>
	/// <param name="cargoId"></param>
	/// <param name="location"></param>
	public async Task<(string, Cargo?)> UpdateCargoLocation(string cargoId, string location)
	{
		try
		{
			var filter = Builders<Cargo>.Filter.Eq(cargo => cargo.Id, cargoId);
			var update = Builders<Cargo>.Update
					.Set(cargo => cargo.Location, location);

			var result = await _collection.FindOneAndUpdateAsync(filter, update);

			return (string.Empty, result);
		}
		catch (MongoException ex)
		{
			_logger.LogError($"Unable to update location {location} on cargo {cargoId}", ex.Message);
			return (ex.Message, null);
		}
	}
}