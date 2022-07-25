using Logistics.DataAccess.Models;

namespace Logistics.Service.Interfaces
{
    public interface ICityService
    {
        public Task<(string, IEnumerable<City>)> GetCities();

        public Task<(string, City?)> GetCity(string cityId);

        public Task<(string, NeighbouringCities?)> GetNeighbouringCities(string cityId, long count);

        public Task<bool> CityDoesNotExists(string cityId);
    }
}