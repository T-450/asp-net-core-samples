using Microsoft.AspNetCore.Mvc;

namespace RateLimiting.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ActionResult<string>), StatusCodes.Status200OK)]
    [LimitRequests(MaxRequests = 2, TimeWindow = 5)]
    public ActionResult<string> GetAll()
    {
        return Ok("Request received");
    }
}
