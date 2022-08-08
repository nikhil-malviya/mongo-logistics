using Logistics.DataAccess.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Optional;

namespace Logistics.Service.Interfaces;

public interface ICityService
{
  public Task<Option<IEnumerable<BsonDocument>, Error>> GetCities(ProjectionDefinition<BsonDocument> projection = default);

  public Task<Option<BsonDocument?, Error>> GetCity(string cityId, ProjectionDefinition<BsonDocument> projection = default);

  public Task<Option<IEnumerable<BsonDocument>, Error>> GetNeighbouringCities(string cityId, int count, ProjectionDefinition<BsonDocument> projection = default);

  public Task<Option<bool, Error>> CityExists(string cityId);
}
