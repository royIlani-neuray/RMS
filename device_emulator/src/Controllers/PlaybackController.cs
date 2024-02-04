/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
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
