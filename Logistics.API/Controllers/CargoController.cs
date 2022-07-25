using Logistics.DataAccess.Models;
using Logistics.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

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
	public async Task<ActionResult<Cargo>> AddCargo(string location, string destination)
	{
		if (await _cityService.CityDoesNotExists(location))
		{
			return BadRequest();
		}
		if (await _cityService.CityDoesNotExists(destination))
		{
			return BadRequest();
		}

		var (error, cargo) = await _cargoService.AddCargo(location, destination);
		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		return Ok(cargo);
	}

	/// <summary>
	/// Deliver a cargo
	/// </summary>
	/// <param name="cargoId"></param>
	/// <returns></returns>
	[HttpPut("{cargoId}/delivered")]
	public async Task<ActionResult<bool>> DeliverCargo(string cargoId)
	{
		var (error, result) = await _cargoService.DeliverCargo(cargoId);
		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		return Ok(result);
	}

	/// <summary>
	/// Set courier for cargo
	/// </summary>
	/// <param name="cargoId"></param>
	/// <param name="planeId"></param>
	/// <returns></returns>
	[HttpPut("{cargoId}/courier/{courierId}")]
	public async Task<ActionResult<bool>> UpdateCargo(string cargoId, string courierId)
	{
		var (error, result) = await _cargoService.UpdateCargo(cargoId, courierId);

		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		return Ok(result);
	}

	/// <summary>
	/// Set value of courier on a given piece of cargo to null
	/// </summary>
	/// <param name="cargoId"></param>
	/// <returns></returns>
	[HttpDelete("{cargoId}/courier")]
	public async Task<ActionResult<Cargo>> UnloadCargo(string cargoId)
	{
		var (error, cargo) = await _cargoService.UnloadCargo(cargoId);

		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		return Ok(cargo);
	}

	/// <summary>
	/// Move cargo from plane to city and vice versa
	/// </summary>
	/// <param name="cargoId"></param>
	/// <param name="location"></param>
	/// <returns></returns>
	[HttpPut("{cargoId}/location/{location}")]
	public async Task<ActionResult<Cargo>> UpdateCargoLocation(string cargoId, string location)
	{
		var (error, cargo) = await _cargoService.UpdateCargoLocation(cargoId, location);

		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		return Ok(cargo);
	}

	/// <summary>
	/// List of InProcess cargos for a location
	/// </summary>
	/// <param name="location"></param>
	/// <returns></returns>
	[HttpGet("location/{location}")]
	public async Task<ActionResult<IEnumerable<Cargo>>> GetCargos(string location)
	{
		var (error, cargos) = await _cargoService.GetCargos(location);

		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		return Ok(cargos);
	}
}