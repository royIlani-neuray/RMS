using WebService.Entites;
using WebService.Context;
using WebService.Actions.Radar;
using WebService.Tracking;
using Microsoft.AspNetCore.Mvc;


namespace WebService.Controllers;

[ApiController]
[Route("devices")]
public class DeviceController : ControllerBase
{
    private readonly ILogger<DeviceController> _logger;

    public DeviceController(ILogger<DeviceController> logger)
    {
        _logger = logger;
    }

    private void ValidateDeviceId(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || !Guid.TryParse(deviceId, out _))
            throw new BadRequestException("invalid device id provided.");
    }

    [HttpGet]
    public List<RadarDevice.RadarDeviceBrief> GetDevices()
    {
        return DeviceContext.Instance.GetDevicesBrief();
    }

    [HttpGet("{deviceId}")]
    public RadarDevice GetRadarDevice(string deviceId)
    {
        ValidateDeviceId(deviceId);        
        if (!DeviceContext.Instance.IsRadarDeviceExist(deviceId))
            throw new NotFoundException("There is no device with the provided id");

        return DeviceContext.Instance.GetDevice(deviceId);
    }

    [HttpPost]
    public void AddRadarDevice([FromBody] AddRadarDeviceArgs args)
    {
        AddRadarAction action = new AddRadarAction(args);
        action.Run();
        return;
    }

    [HttpDelete("{deviceId}")]
    public void DeleteRadarDevice(string deviceId)
    {        
        ValidateDeviceId(deviceId); 
        var action = new DeleteRadarAction(deviceId);
        action.Run();
    }

    [HttpPost("{deviceId}/enable")]
    public void EnableRadarDevice(string deviceId)
    {
        ValidateDeviceId(deviceId);
        var action = new EnableRadarAction(deviceId);
        action.Run();
    }

    [HttpPost("{deviceId}/disable")]
    public void DisableRadarDevice(string deviceId)
    {
        ValidateDeviceId(deviceId);
        var action = new DisableRadarAction(deviceId);
        action.Run();
    }

    [HttpPut("{deviceId}/radar-info")]
    public void UpdateRadarInfo(string deviceId, [FromBody] UpdateRadarInfoArgs args)
    {
        ValidateDeviceId(deviceId); 
        args.Validate();
        var action = new UpdateRadarInfoAction(deviceId, args);
        action.Run();        
    }

    [HttpPut("{deviceId}/tracks-reports")]
    public void SetTracksReports(string deviceId, [FromBody] SetTracksReportsArgs args)
    {
        ValidateDeviceId(deviceId); 
        args.Validate();
        var action = new SetTracksReportsAction(deviceId, args);
        action.Run();        
    }

    [HttpPut("{deviceId}/network")]
    public void SetDeviceNetwork(string deviceId, [FromBody] SetDeviceNetworkArgs args)
    {
        ValidateDeviceId(deviceId); 
        args.Validate();
        var action = new SetDeviceNetworkAction(deviceId, args);
        action.Run();
    }
    
    [HttpPost("{deviceId}/config")]
    public void SetRadarConfig(string deviceId, [FromBody] SetRadarConfigArgs args)
    {
        ValidateDeviceId(deviceId); 
        args.Validate();
        var action = new SetRadarConfigAction(deviceId, args);
        action.Run();
    }

    [HttpGet("{deviceId}/tracks")]
    public HttpTracksReport GetDeviceTracks(string deviceId)
    {
        ValidateDeviceId(deviceId); 
        var radarDevice = DeviceContext.Instance.GetDevice(deviceId);

        if ((radarDevice.radarTracker != null) && (radarDevice.radarTracker.LastFrameData != null))
        {
            return new HttpTracksReport(radarDevice.radarTracker.LastFrameData);
        }
        else
        {
            return new HttpTracksReport() {
                DeviceId = radarDevice.Id,
                DeviceName = radarDevice.Name
            };
        }
    }

    [HttpPost("{deviceId}/tracks-loopback")]
    public void TracksLoopback(string deviceId, [FromBody] object data)
    {
        System.Console.WriteLine("***** Tracks Loopback ******");
        System.Console.WriteLine(data);
        System.Console.WriteLine("****************************");
    }
}
