using Logistics.DataAccess.Models;

namespace Logistics.Service.Interfaces
{
    public interface ICargoService
    {
        public Task<(string, Cargo?)> AddCargo(string location, string destination);

        public Task<(string, bool)> DeliverCargo(string cargoId);

        public Task<(string, bool)> UpdateCargo(string cargoId, string courierId);

        public Task<(string, Cargo?)> UnloadCargo(string cargoId);

        public Task<(string, Cargo?)> UpdateCargoLocation(string cargoId, string location);

        public Task<(string, IEnumerable<Cargo>)> GetCargos(string location);
    }
}