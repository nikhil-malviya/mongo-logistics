using Logistics.DataAccess.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Optional;

namespace Logistics.Service.Interfaces;

public interface ICargoService
{
  public Task<Option<BsonDocument?, Error>> AddCargo(string location, string destination);

  public Task<Option<IEnumerable<BsonDocument>, Error>> GetCargos(string location, ProjectionDefinition<BsonDocument> projection = default);

  public Task<Option<BsonDocument?, Error>> UnloadCargo(string cargoId);

  public Task<Option<bool, Error>> DeliverCargo(string cargoId);

  public Task<Option<bool, Error>> UpdateCargo(string cargoId, string courierId);

  public Task<Option<BsonDocument?, Error>> UpdateCargoLocation(string cargoId, string location);
}

