using Microsoft.AspNetCore.Mvc;

namespace DeviceEmulator.Controllers;


[ApiController]
[Route("emulator/recordings")]
public class RecordingsController : ControllerBase
{
    private readonly ILogger<RecordingsController> _logger;

    public RecordingsController(ILogger<RecordingsController> logger)
    {
        _logger = logger;
    }

    [HttpPut]
    public void SetActiveRecording(string deviceId, [FromBody] object args)
    {
        
    }

}
