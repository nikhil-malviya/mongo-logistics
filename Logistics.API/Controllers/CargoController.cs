using Logistics.API.Serializer;
using Logistics.DataAccess.Models;
using Logistics.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers
{
	[Route("cargo")]
	public class CargoController : BaseController
	{
		private readonly ICargoService _cargoService;
		private readonly ICityService _cityService;

		public CargoController(ICargoService cargoService, ICityService cityService)
		{
			_cargoService = cargoService ?? throw new ArgumentNullException(nameof(cargoService));
			_cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
		}

		/// <summary>
		/// Create a cargo with location to destination
		/// </summary>
		/// <param name="location"></param>
		/// <param name="destination"></param>
		/// <returns></returns>
		[HttpPost("{location}/to/{destination}")]
		public async Task<IActionResult> AddCargo(string location, string destination)
		{
			Optional.Option<bool, Error> exists = await _cityService.CityExists(location);

			Error error = new();
			exists.MatchNone(err => error = err);

			if (!exists.HasValue)
			{
				return ServerError(error);
			}

			if (exists.Contains(false))
			{
				return BadRequest();
			}

			Optional.Option<MongoDB.Bson.BsonDocument?, Error> result = await _cargoService.AddCargo(location, destination);

			return result.Match(
				some: result => Ok(result.ToCargoResponse()),
				none: error => ServerError(error)
			);
		}

		/// <summary>
		/// Deliver a cargo
		/// </summary>
		/// <param name="cargoId"></param>
		/// <returns></returns>
		[HttpPut("{cargoId}/delivered")]
		public async Task<IActionResult> DeliverCargo(string cargoId)
		{
			Optional.Option<bool, Error> result = await _cargoService.DeliverCargo(cargoId);

			return result.Match(
				some: result => Ok(result),
				none: error => ServerError(error)
			);
		}

		/// <summary>
		/// Set courier for cargo
		/// </summary>
		/// <param name="cargoId"></param>
		/// <param name="planeId"></param>
		/// <returns></returns>
		[HttpPut("{cargoId}/courier/{courierId}")]
		public async Task<IActionResult> UpdateCargo(string cargoId, string courierId)
		{
			Optional.Option<bool, Error> result = await _cargoService.UpdateCargo(cargoId, courierId);

			return result.Match(
				some: result => Ok(result),
				none: error => ServerError(error)
			);
		}

		/// <summary>
		/// Set value of courier on a given piece of cargo to null
		/// </summary>
		/// <param name="cargoId"></param>
		/// <returns></returns>
		[HttpDelete("{cargoId}/courier")]
		public async Task<IActionResult> UnloadCargo(string cargoId)
		{
			Optional.Option<MongoDB.Bson.BsonDocument?, Error> result = await _cargoService.UnloadCargo(cargoId);

			return result.Match(
				some: result => Ok(result.ToCargoResponse()),
				none: error => ServerError(error)
			);
		}

		/// <summary>
		/// Move cargo from plane to city and vice versa
		/// </summary>
		/// <param name="cargoId"></param>
		/// <param name="location"></param>
		/// <returns></returns>
		[HttpPut("{cargoId}/location/{location}")]
		public async Task<IActionResult> UpdateCargoLocation(string cargoId, string location)
		{
			Optional.Option<MongoDB.Bson.BsonDocument?, Error> result = await _cargoService.UpdateCargoLocation(cargoId, location);

			return result.Match(
				some: result => Ok(result.ToCargoResponse()),
				none: error => ServerError(error)
			);
		}

		/// <summary>
		/// List of InProcess cargos for a location
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		[HttpGet("location/{location}")]
		public async Task<IActionResult> GetCargos(string location)
		{
			Optional.Option<IEnumerable<MongoDB.Bson.BsonDocument>, Error> results = await _cargoService.GetCargos(location);

			return results.Match(
				some: results => Ok(results.ToCargoResponse()),
				none: error => ServerError(error)
			);
		}
	}
}