using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("/")]
public class BaseController : ControllerBase
{
	protected ObjectResult ServerError()
	{
		return StatusCode(500, "Internal server error");
	}
}