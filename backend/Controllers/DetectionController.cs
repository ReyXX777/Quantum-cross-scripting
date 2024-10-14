using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class DetectionController : ControllerBase
{
    [HttpPost("detect")]
    public IActionResult Detect([FromBody] DetectionModel model)
    {
        // XSS detection logic
        return Ok(new { IsMalicious = false });
    }
}
