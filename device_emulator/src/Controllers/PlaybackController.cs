using Microsoft.AspNetCore.Mvc;
using DeviceEmulator;

namespace DeviceEmulator.Controllers;


[ApiController]
[Route("emulator/playback")]
public class PlaybackController : ControllerBase
{
    private readonly ILogger<PlaybackController> _logger;

    public PlaybackController(ILogger<PlaybackController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public Emulator.PlaybackArgs GetPlaybackInfo()
    {
        return Emulator.Instance.GetPlayback();
    }

    [HttpPut]
    public async Task SetPlayback([FromBody] Emulator.PlaybackArgs args)
    {
        await Emulator.Instance.SetPlaybackAsync(args);
    }

}
