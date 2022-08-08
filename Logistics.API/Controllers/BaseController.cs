using Logistics.DataAccess.Models;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("/")]
public class BaseController : ControllerBase
{
	public const string JsonContentType = "application/json";

	protected ObjectResult ServerError(Error error)
	{
		return StatusCode(500, $"Internal server error. Error code: {error.Code}");
	}

	protected async Task<OkResult> WriteResponseAsync(string json)
	{
		Response.ContentType = JsonContentType;
		await Response.WriteAsync(json);

		return Ok();
	}
}
