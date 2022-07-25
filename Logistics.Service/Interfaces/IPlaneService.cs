using Logistics.DataAccess.Models;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Logistics.Service.Interfaces
{
    public interface IPlaneService
    {
        public Task<(string, IEnumerable<Plane>)> GetPlanes();

        public Task<(string, Plane?)> GetPlane(string planeId);

        public Task<(string, Plane?)> UpdatePlaneLocation(string planeId, GeoJson2DCoordinates newLocation, double heading);

        public Task<(string, Plane?)> LandPlane(string planeId, GeoJson2DCoordinates newLocation, double heading, string cityId);

        public Task<(string, bool)> RemoveDestination(string planeId);

        public Task<(string, bool)> ReplaceDestination(string planeId, string cityId);

        public Task<(string, bool)> UpdateDestination(string planeId, string cityId);

        public Task<bool> PlaneDoesNotExists(string planeId);
    }
}