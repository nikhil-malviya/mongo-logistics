using Logistics.DataAccess.Models;
using Logistics.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("cities")]
public class CityController : BaseController
{
	private readonly ICityService _cityService;

	public CityController(ICityService cityService)
	{
		_cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
	}

	/// <summary>
	/// Get all cities
	/// </summary>
	/// <param name="location"></param>
	/// <param name="destination"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<ActionResult<IEnumerable<City>>> GetCities()
	{
		var (error, results) = await _cityService.GetCities();

		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		return Ok(results);
	}

	/// <summary>
	/// Get city by cityId
	/// </summary>
	/// <param name="location"></param>
	/// <param name="destination"></param>
	/// <returns></returns>
	[HttpGet("{cityId}")]
	public async Task<ActionResult<City>> GetCity(string cityId)
	{
		var (error, result) = await _cityService.GetCity(cityId);

		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		if (result == null)
		{
			return NotFound();
		}

		return Ok(result);
	}

	/// <summary>
	/// Get requested number of neighbouring cities for a city
	/// </summary>
	/// <param name="cityId"></param>
	/// <param name="count"></param>
	/// <returns></returns>

	[HttpGet("{cityId}/neighbors/{count}")]
	public async Task<dynamic> GetNeighbouringCities(string cityId, long count)
	{
		if (await _cityService.CityDoesNotExists(cityId))
		{
			return BadRequest();
		}

		var (error, result) = await _cityService.GetNeighbouringCities(cityId, count);

		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		return Ok(result);
	}
}