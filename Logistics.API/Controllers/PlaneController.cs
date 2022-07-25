using Logistics.DataAccess.Models;
using Logistics.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.GeoJsonObjectModel;

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
	public async Task<ActionResult<IEnumerable<Plane>>> GetPlanes()
	{
		var (error, results) = await _planeService.GetPlanes();

		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		return Ok(results);
	}

	/// <summary>
	/// Get plane by planeId
	/// </summary>
	/// <param name="planeId"></param>
	/// <returns></returns>
	[HttpGet("{planeId}")]
	public async Task<ActionResult<Plane>> GetPlane(string planeId)
	{
		var (error, result) = await _planeService.GetPlane(planeId);

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
	/// Update plane location
	/// </summary>
	/// <param name="planeId"></param>
	/// <param name="location"></param>
	/// <param name="heading"></param>
	/// <returns></returns>
	[HttpPut("{planeId}/location/{location}/{heading}")]
	public async Task<ActionResult<Plane>> UpdatePlaneLocation(string planeId, string location, double heading)
	{
		if (await _planeService.PlaneDoesNotExists(planeId))
		{
			return BadRequest();
		}

		var (parsed, newLocation) = TryParseLocation(location);
		if (!parsed)
		{
			return BadRequest();
		}

		var (error, result) = await _planeService.UpdatePlaneLocation(planeId, newLocation, heading);

		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		return Ok(result);
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
	public async Task<ActionResult<Plane>> LandPlane(string planeId, [FromRoute] string location, double heading, string cityId)
	{
		if (await _cityService.CityDoesNotExists(cityId))
		{
			return BadRequest();
		}
		if (await _planeService.PlaneDoesNotExists(planeId))
		{
			return BadRequest();
		}

		var newLocation = new GeoJson2DCoordinates(double.Parse(location.Split(',')[0]), double.Parse(location.Split(',')[1]));

		var (error, result) = await _planeService.LandPlane(planeId, newLocation, heading, cityId);

		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		return Ok(result);
	}

	/// <summary>
	/// Remove destination from route
	/// </summary>
	/// <param name="planeId"></param>
	/// <returns></returns>
	[HttpDelete("{planeId}/route/destination")]
	public async Task<ActionResult<bool>> RemoveDestination(string planeId)
	{
		if (await _planeService.PlaneDoesNotExists(planeId))
		{
			return BadRequest();
		}

		var (error, result) = await _planeService.RemoveDestination(planeId);

		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		return Ok(result);
	}

	/// <summary>
	/// Replace routes with a single city
	/// </summary>
	/// <param name="planeId"></param>
	/// <param name="cityId"></param>
	/// <returns></returns>
	[HttpPut("{planeId}/route/{cityId}")]
	public async Task<ActionResult<bool>> ReplaceDestination(string planeId, string cityId)
	{
		if (await _cityService.CityDoesNotExists(cityId))
		{
			return BadRequest();
		}
		if (await _planeService.PlaneDoesNotExists(planeId))
		{
			return BadRequest();
		}

		var (error, result) = await _planeService.ReplaceDestination(planeId, cityId);

		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		return Ok(result);
	}

	/// <summary>
	/// Replace route with a single city
	/// </summary>
	/// <param name="planeId"></param>
	/// <param name="cityId"></param>
	/// <returns></returns>
	[HttpPost("{planeId}/route/{cityId}")]
	public async Task<ActionResult<bool>> UpdateDestination(string planeId, string cityId)
	{
		if (await _cityService.CityDoesNotExists(cityId))
		{
			return BadRequest();
		}
		if (await _planeService.PlaneDoesNotExists(planeId))
		{
			return BadRequest();
		}

		var (error, result) = await _planeService.UpdateDestination(planeId, cityId);

		if (!string.IsNullOrWhiteSpace(error))
		{
			return ServerError();
		}

		return Ok(result);
	}

	/// <summary>
	/// Try parse location string to GeoJson2DCoordinates
	/// </summary>
	/// <param name="location"></param>
	/// <returns></returns>
	private (bool, GeoJson2DCoordinates) TryParseLocation(string location)
	{
		var tokens = location.Split(',');
		if (tokens.Length != 2 || !double.TryParse(tokens[0], out double latitude) || !double.TryParse(tokens[1], out double longitude))
		{
			return (false, new GeoJson2DCoordinates(0, 0));
		}

		return (true, new GeoJson2DCoordinates(latitude, longitude));
	}
}