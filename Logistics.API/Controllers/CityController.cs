using Logistics.API.Responses;
using Logistics.API.Serializer;
using Logistics.DataAccess.Models;
using Logistics.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Optional.Unsafe;

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
	public async Task<IActionResult> GetCities()
	{
		var cities = await _cityService.GetCities();

		return cities.Match(
			some: results => Ok(results.ToCityResponse()),
			none: error => ServerError(error)
		);
	}

	/// <summary>
	/// Get city by cityId
	/// </summary>
	/// <param name="location"></param>
	/// <param name="destination"></param>
	/// <returns></returns>
	[HttpGet("{cityId}")]
	public async Task<IActionResult> GetCity(string cityId)
	{
		var city = await _cityService.GetCity(cityId);

		if (city.Contains(null))
		{
			return NotFound();
		}

		return city.Match(
			some: result => Ok(result.ToCityResponse()),
			none: error => ServerError(error)
		);
	}

	/// <summary>
	/// Get requested number of neighbouring cities for a city
	/// </summary>
	/// <param name="cityId"></param>
	/// <param name="count"></param>
	/// <returns></returns>

	[HttpGet("{cityId}/neighbors/{count}")]
	public async Task<IActionResult> GetNeighbouringCities(string cityId, int count)
	{
		var exists = await _cityService.CityExists(cityId);

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

		var results = await _cityService.GetNeighbouringCities(cityId, count);

		return results.Match(
			some: results => Ok(new { Neighbors = results.ToCityResponse() }),
			none: error => ServerError(error)
		);
	}
}
