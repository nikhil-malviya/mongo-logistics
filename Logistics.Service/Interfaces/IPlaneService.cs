using Logistics.DataAccess.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Optional;

namespace Logistics.Service.Interfaces;

public interface IPlaneService
{
	public Task<Option<IEnumerable<BsonDocument>, Error>> GetPlanes(ProjectionDefinition<BsonDocument> projection = default);

	public Task<Option<BsonDocument?, Error>> GetPlane(string planeId, ProjectionDefinition<BsonDocument> projection = default);

	public Task<Option<BsonDocument?, Error>> UpdatePlaneLocation(string planeId, GeoJson2DGeographicCoordinates newLocation, double heading);

	public Task<Option<BsonDocument?, Error>> LandPlane(string planeId, GeoJson2DGeographicCoordinates newLocation, double heading, string cityId);

	public Task<Option<bool, Error>> RemoveDestination(string planeId);

	public Task<Option<bool, Error>> ReplaceDestination(string planeId, string cityId);

	public Task<Option<bool, Error>> UpdateDestination(string planeId, string cityId);

	public Task<Option<bool, Error>> PlaneExists(string planeId);
}
