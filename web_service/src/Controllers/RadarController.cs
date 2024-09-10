/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Context;
using WebService.Actions.Radars;
using WebService.RadarLogic.Streaming;
using Microsoft.AspNetCore.Mvc;
using WebService.Actions.Services;
using WebService.RadarLogic.IPRadar;
using Serilog;

namespace WebService.Controllers;

[ApiController]
[Route("radars")]
public class RadarController : ControllerBase
{
    private readonly ILogger<RadarController> _logger;

    public RadarController(ILogger<RadarController> logger)
    {
        _logger = logger;
    }

    private void ValidateRadarId(string radarId)
    {
        if (string.IsNullOrWhiteSpace(radarId) || !Guid.TryParse(radarId, out _))
            throw new BadRequestException("invalid device id provided.");
    }

    [HttpGet]
    public List<Radar.RadarBrief> GetRadars()
    {
        return RadarContext.Instance.GetRadarsBrief();
    }

    [HttpGet("{radarId}")]
    public Radar GetRadar(string radarId)
    {
        ValidateRadarId(radarId);        
        if (!RadarContext.Instance.IsRadarExist(radarId))
            throw new NotFoundException("There is no radar with the provided id");

        return RadarContext.Instance.GetRadar(radarId);
    }

    [HttpPost]
    public void AddRadar([FromBody] AddRadarArgs args)
    {
        AddRadarAction action = new AddRadarAction(args);
        action.Run();
        return;
    }

    [HttpDelete("{radarId}")]
    public void DeleteRadar(string radarId)
    {        
        ValidateRadarId(radarId); 
        var action = new DeleteRadarAction(radarId);
        action.Run();
    }

    [HttpPost("{radarId}/enable")]
    public void EnableRadar(string radarId)
    {
        ValidateRadarId(radarId);
        var action = new EnableRadarAction(radarId);
        action.Run();
    }

    [HttpPost("{radarId}/disable")]
    public void DisableRadar(string radarId)
    {
        ValidateRadarId(radarId);
        var action = new DisableRadarAction(radarId);
        action.Run();
    }

    [HttpPut("{radarId}/radar-info")]
    public void UpdateRadarInfo(string radarId, [FromBody] UpdateRadarInfoArgs args)
    {
        ValidateRadarId(radarId); 
        args.Validate();
        var action = new UpdateRadarInfoAction(radarId, args);
        action.Run();        
    }

    [HttpPut("{radarId}/tracks-reports")]
    public void SetTracksReports(string radarId, [FromBody] SetTracksReportsArgs args)
    {
        ValidateRadarId(radarId); 
        args.Validate();
        var action = new SetTracksReportsAction(radarId, args);
        action.Run();        
    }

    [HttpPut("{radarId}/network")]
    public void SetDeviceNetwork(string radarId, [FromBody] SetDeviceNetworkArgs args)
    {
        ValidateRadarId(radarId); 
        args.Validate();
        var action = new SetDeviceNetworkAction(radarId, args);
        action.Run();
    }
    
    [HttpPut("{radarId}/device-id")]
    public void SetRadarId(string radarId, [FromBody] SetRadarIdArgs args)
    {
        ValidateRadarId(radarId); 
        args.Validate();
        var action = new SetRadarIdAction(radarId, args);
        action.Run();
    }

    [HttpPost("{radarId}/config")]
    public void SetRadarConfig(string radarId, [FromBody] SetRadarConfigArgs args)
    {
        ValidateRadarId(radarId); 
        args.Validate();
        var action = new SetRadarConfigAction(radarId, args);
        action.Run();
    }

    [HttpPost("{radarId}/reset-device-broadcast")]
    public void SendRestBroadcast(string radarId)
    {
        ValidateRadarId(radarId); 
        IPRadarAPI.SendResetBroadcast(radarId);
    }

    [HttpGet("{radarId}/tracks")]
    public HttpTracksReport GetRadarTracks(string radarId)
    {
        ValidateRadarId(radarId); 
        var radar = RadarContext.Instance.GetRadar(radarId);

        if ((radar.radarStreamer != null) && (radar.radarStreamer.LastFrameData != null))
        {
            return new HttpTracksReport(radar.radarStreamer.LastFrameData);
        }
        else
        {
            return new HttpTracksReport() {
                DeviceId = radar.Id,
                DeviceName = radar.Name
            };
        }
    }

    [HttpGet("{radarId}/remote-radar-ports")]
    public IActionResult GetRemoteRadarPorts(string radarId)
    {
        ValidateRadarId(radarId); 
        var radar = RadarContext.Instance.GetRadar(radarId);

        if (radar.ipRadarAPI == null)
        {
            throw new WebServiceException("failed to get remote radar ports.");
        }

        radar.ipRadarAPI.GetRemoteRadarConnectionPorts(out int controlPort, out int dataPort);

        byte [] response = new byte[4];
        BitConverter.GetBytes((UInt16) controlPort).CopyTo(response, 0);
        BitConverter.GetBytes((UInt16) dataPort).CopyTo(response, 2);

        return File(response, "application/octet-stream");
    }


    [HttpPost("{radarId}/fw-update")]
    public async Task<IActionResult> FirmewareUpdate(string radarId)
    {
        using (var ms = new MemoryStream())
        {
            await Request.Body.CopyToAsync(ms);
            byte [] image = new byte[ms.Length];
            ms.Seek(0,SeekOrigin.Begin);
            ms.Read(image, 0, image.Length);

            var action = new FWUpdateAction(radarId, image);
            action.Run();
        }

        return Ok();
    }   

    [HttpPost("{radarId}/tracks-loopback")]
    public void TracksLoopback(string radarId, [FromBody] string data)
    {
        // this API is used for debug

        Log.Debug("***** Tracks Loopback ******");
        Log.Debug(data);
        Log.Debug("****************************");
    }

    [HttpPost("{radarId}/services")]
    public void LinkToService(string radarId, [FromBody] LinkServiceArgs args)
    {
        ValidateRadarId(radarId); 
        args.Validate();
        LinkRadarServiceAction action = new LinkRadarServiceAction(radarId, args);
        action.Run();
    }

    [HttpDelete("{radarId}/services/{serviceId}")]
    public void UnlinkService(string radarId, string serviceId)
    {        
        ValidateRadarId(radarId); 
        var action = new UnlinkRadarServiceAction(radarId, serviceId);
        action.Run();
    }

    [HttpPost("{radarId}/rms-hostname")]
    public void SetRMSHostname(string radarId, [FromBody] SetRMSHostnameArgs args)
    {
        ValidateRadarId(radarId); 
        args.Validate();
        var action = new SetRMSHostnameAction(radarId, args);
        action.Run();
    }

    [HttpGet("{radarId}/calibration")]
    public string GetRadarCalibration(string radarId)
    {
        ValidateRadarId(radarId); 
        var action = new GetCalibrationData(radarId);
        action.Run();
        return action.Results;
    }
}

