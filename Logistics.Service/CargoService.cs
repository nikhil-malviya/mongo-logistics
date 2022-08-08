using Logistics.DataAccess.Constants;
using Logistics.DataAccess.Models;
using Logistics.DataAccess.MongoDB;
using Logistics.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Optional;

namespace Logistics.Service;

public class CargoService : ICargoService
{
	private readonly IMongoCollection<BsonDocument> _collection;
	private readonly ILogger<CargoService> _logger;
	private readonly ProjectionDefinition<BsonDocument> _defaultProjection;

	public CargoService(IMongoClient client, IConfiguration configuration, ILogger<CargoService> logger)
	{
		var database = client.GetDatabase(configuration["Database"])
										.WithWriteConcern(WriteConcern.WMajority)
										.WithReadConcern(ReadConcern.Majority);

		_collection = database.GetCollection<BsonDocument>(Database.CargoCollection);
		_defaultProjection = Builders<BsonDocument>.Projection
						.Include(Cargo.Id)
						.Include(Cargo.Courier)
						.Include(Cargo.Received)
						.Include(Cargo.Status)
						.Include(Cargo.Location)
						.Include(Cargo.Destination);

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Create a cargo with location to destination
	/// </summary>
	/// <param name="location"></param>
	/// <param name="destination"></param>
	public async Task<Option<BsonDocument?, Error>> AddCargo(string location, string destination)
	{
		try
		{
			var cargo = new CargoDocument(new BsonDocument())
			{
				Received = DateTime.UtcNow,
				Location = location,
				Destination = destination,
				Status = CargoStatus.InProcess,
			};

			await Command.InsertOneAsync(_collection, cargo.Document);

			return Option.Some<BsonDocument?, Error>(cargo.Document);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to add cargo for location: {location} and destination: {destination}", ex.Message);
			return Option.None<BsonDocument?, Error>(error);
		}
	}

	/// <summary>
	/// Fetches list of InProcess cargos for a location
	/// </summary>
	/// <param name="location"></param>
	/// <returns></returns>
	public async Task<Option<IEnumerable<BsonDocument>, Error>> GetCargos(string location, ProjectionDefinition<BsonDocument> projection = default)
	{
		try
		{
			var builder = Builders<BsonDocument>.Filter;
			var filter = builder.Eq(Cargo.Location, location) & builder.Eq(Cargo.Status, CargoStatus.InProcess);

			var cargos = await Query.FindAsync(_collection, filter, projection ?? _defaultProjection).ConfigureAwait(false);

			return Option.Some<IEnumerable<BsonDocument>, Error>(cargos);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError("Unable to fetch cargos data", error);
			return Option.None<IEnumerable<BsonDocument>, Error>(error);
		}
	}

	/// <summary>
	/// Updates cargo courier to null
	/// </summary>
	/// <param name="cargoId"></param>
	public async Task<Option<BsonDocument?, Error>> UnloadCargo(string cargoId)
	{
		try
		{
			var filter = Builders<BsonDocument>.Filter.Eq(Cargo.Id, new ObjectId(cargoId));
			var update = Builders<BsonDocument>.Update
					.Set(Cargo.Courier, default(string));

			var result = await Command.FindOneAndUpdateAsync(_collection, filter, update);

			return Option.Some<BsonDocument?, Error>(result);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to set cargo courier to null for cargoId: {cargoId}", error);
			return Option.None<BsonDocument?, Error>(error);
		}
	}

	/// <summary>
	/// Set cargo status as delivered
	/// </summary>
	/// <param name="cargoId"></param>
	/// <returns></returns>
	public async Task<Option<bool, Error>> DeliverCargo(string cargoId)
	{
		try
		{
			var filter = Builders<BsonDocument>.Filter.Eq(Cargo.Id, new ObjectId(cargoId));
			var update = Builders<BsonDocument>.Update
					.Set(Cargo.Status, CargoStatus.Delivered);

			var result = await Command.UpdateOneAsync(_collection, filter, update);

			return Option.Some<bool, Error>(result.IsAcknowledged && result.ModifiedCount == 1);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to set cargo status as delivered for cargoId: {cargoId}", ex.Message);
			return Option.None<bool, Error>(error);
		}
	}

	/// <summary>
	/// Set courier for cargo
	/// </summary>
	/// <param name="cargoId"></param>
	/// <param name="planeId"></param>
	public async Task<Option<bool, Error>> UpdateCargo(string cargoId, string courierId)
	{
		try
		{
			var filter = Builders<BsonDocument>.Filter.Eq(Cargo.Id, new ObjectId(cargoId));
			var update = Builders<BsonDocument>.Update
					.Set(Cargo.Courier, courierId);

			var result = await Command.UpdateOneAsync(_collection, filter, update);

			return Option.Some<bool, Error>(result.IsAcknowledged && result.ModifiedCount == 1);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to set courier as {courierId} for cargoid: {cargoId}", ex.Message);
			return Option.None<bool, Error>(error);
		}
	}

	/// <summary>
	/// Move cargo from plane to city and vice versa
	/// </summary>
	/// <param name="cargoId"></param>
	/// <param name="location"></param>
	public async Task<Option<BsonDocument?, Error>> UpdateCargoLocation(string cargoId, string location)
	{
		try
		{
			var filter = Builders<BsonDocument>.Filter.Eq(Cargo.Id, new ObjectId(cargoId));
			var update = Builders<BsonDocument>.Update
					.Set(Cargo.Location, location);

			var result = await Command.FindOneAndUpdateAsync(_collection, filter, update);

			return Option.Some<BsonDocument?, Error>(result);
		}
		catch (MongoException ex)
		{
			var error = new Error(ErrorCode.MongoError, ex.Message);
			_logger.LogError($"Unable to update location {location} on cargo {cargoId}", ex.Message);
			return Option.None<BsonDocument?, Error>(error);
		}
	}
}
