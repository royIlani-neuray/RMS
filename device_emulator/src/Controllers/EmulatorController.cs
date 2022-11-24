using Microsoft.AspNetCore.Mvc;

namespace DeviceEmulator.Controllers;


[ApiController]
[Route("emulator")]
public class EmulatorController : ControllerBase
{
    private readonly ILogger<EmulatorController> _logger;

    public EmulatorController(ILogger<EmulatorController> logger)
    {
        _logger = logger;
    }

    [HttpGet("version")]
    public object GetVersion()
    {
        return new 
        {
            version = EmulatorSettings.Instance.EmulatorVersion
        };
    }

}
