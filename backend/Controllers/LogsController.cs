using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class LogsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetLogs()
    {
        // Return logs
        return Ok(new List<string> { "Log1", "Log2" });
    }
}
