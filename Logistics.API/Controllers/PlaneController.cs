using Logistics.API.Serializer;
using Logistics.DataAccess.Models;
using Logistics.DataAccess.Utilities;
using Logistics.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("planes")]
public class PlaneController : BaseController
{
	private readonly IPlaneService _planeService;
	private readonly ICityService _cityService;

	public PlaneController(IPlaneService planeService, ICityService cityService)
	{
		_planeService = planeService ?? throw new ArgumentNullException(nameof(planeService));
		_cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
	}

	/// <summary>
	/// Get all planes
	/// </summary>
	/// <returns></returns>
	[HttpGet]
	public async Task<IActionResult> GetPlanes()
	{
		var planes = await _planeService.GetPlanes();

		return planes.Match(
			some: results => Ok(results.ToPlaneResponse()),
			none: error => ServerError(error)
		);
	}

	/// <summary>
	/// Get plane by planeId
	/// </summary>
	/// <param name="planeId"></param>
	/// <returns></returns>
	[HttpGet("{planeId}")]
	public async Task<IActionResult> GetPlane(string planeId)
	{
		var plane = await _planeService.GetPlane(planeId);

		if (plane.Contains(null))
		{
			return NotFound();
		}

		return plane.Match(
			some: result => Ok(result.ToPlaneResponse()),
			none: error => ServerError(error)
		);
	}

	/// <summary>
	/// Update plane location
	/// </summary>
	/// <param name="planeId"></param>
	/// <param name="location"></param>
	/// <param name="heading"></param>
	/// <returns></returns>
	[HttpPut("{planeId}/location/{location}/{heading}")]
	public async Task<IActionResult> UpdatePlaneLocation(string planeId, string location, double heading)
	{
		var exists = await _planeService.PlaneExists(planeId);

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

		var (parsed, newLocation) = location.TryParseCoordinates();
		if (!parsed)
		{
			return BadRequest();
		}

		var result = await _planeService.UpdatePlaneLocation(planeId, newLocation, heading);

		return result.Match(
			some: result => Ok(result.ToPlaneResponse()),
			none: error => ServerError(error)
		);
	}

	/// <summary>
	/// Land plane to the nearest city
	/// </summary>
	/// <param name="planeId"></param>
	/// <param name="location"></param>
	/// <param name="heading"></param>
	/// <param name="cityId"></param>
	/// <returns></returns>
	[HttpPut("{planeId}/location/{location}/{heading}/{cityId}")]
	public async Task<IActionResult> LandPlane(string planeId, [FromRoute] string location, double heading, string cityId)
	{
		var cityExists = await _cityService.CityExists(cityId);

		Error errorCity = new();
		cityExists.MatchNone(err => errorCity = err);

		if (!cityExists.HasValue)
		{
			return ServerError(errorCity);
		}

		if (cityExists.Contains(false))
		{
			return BadRequest();
		}

		var planeExists = await _planeService.PlaneExists(planeId);

		Error errorPlane = new();
		planeExists.MatchNone(err => errorPlane = err);

		if (!planeExists.HasValue)
		{
			return ServerError(errorPlane);
		}

		if (planeExists.Contains(false))
		{
			return BadRequest();
		}

		var (parsed, newLocation) = location.TryParseCoordinates();
		if (!parsed)
		{
			return BadRequest();
		}

		var result = await _planeService.LandPlane(planeId, newLocation, heading, cityId);

		return result.Match(
			some: result => Ok(result.ToPlaneResponse()),
			none: error => ServerError(error)
		);
	}

	/// <summary>
	/// Remove destination from route
	/// </summary>
	/// <param name="planeId"></param>
	/// <returns></returns>
	[HttpDelete("{planeId}/route/destination")]
	public async Task<IActionResult> RemoveDestination(string planeId)
	{
		var exists = await _planeService.PlaneExists(planeId);

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

		var result = await _planeService.RemoveDestination(planeId);

		return result.Match(
			some: result => Ok(result),
			none: error => ServerError(error)
		);
	}

	/// <summary>
	/// Replace routes with a single city
	/// </summary>
	/// <param name="planeId"></param>
	/// <param name="cityId"></param>
	/// <returns></returns>
	[HttpPut("{planeId}/route/{cityId}")]
	public async Task<IActionResult> ReplaceDestination(string planeId, string cityId)
	{
		var cityExists = await _cityService.CityExists(cityId);

		Error errorCity = new();
		cityExists.MatchNone(err => errorCity = err);

		if (!cityExists.HasValue)
		{
			return ServerError(errorCity);
		}

		if (cityExists.Contains(false))
		{
			return BadRequest();
		}

		var planeExists = await _planeService.PlaneExists(planeId);

		Error errorPlane = new();
		planeExists.MatchNone(err => errorPlane = err);

		if (!planeExists.HasValue)
		{
			return ServerError(errorPlane);
		}

		if (planeExists.Contains(false))
		{
			return BadRequest();
		}

		var result = await _planeService.ReplaceDestination(planeId, cityId);

		return result.Match(
			some: result => Ok(result),
			none: error => ServerError(error)
		);
	}

	/// <summary>
	/// Replace route with a single city
	/// </summary>
	/// <param name="planeId"></param>
	/// <param name="cityId"></param>
	/// <returns></returns>
	[HttpPost("{planeId}/route/{cityId}")]
	public async Task<IActionResult> UpdateDestination(string planeId, string cityId)
	{
		var cityExists = await _cityService.CityExists(cityId);

		Error errorCity = new();
		cityExists.MatchNone(err => errorCity = err);

		if (!cityExists.HasValue)
		{
			return ServerError(errorCity);
		}

		if (cityExists.Contains(false))
		{
			return BadRequest();
		}

		var planeExists = await _planeService.PlaneExists(planeId);

		Error errorPlane = new();
		planeExists.MatchNone(err => errorPlane = err);

		if (!planeExists.HasValue)
		{
			return ServerError(errorPlane);
		}

		if (planeExists.Contains(false))
		{
			return BadRequest();
		}

		var result = await _planeService.UpdateDestination(planeId, cityId);

		return result.Match(
			some: result => Ok(result),
			none: error => ServerError(error)
		);
	}
}
