using WebService.Entites;
using WebService.Context;
using WebService.Radar;
using WebService.Actions.Radar;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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

    public class AddRadarDeviceArgs 
    {
        [JsonPropertyName("device_id")]
        public string Id { get; set; } = String.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = String.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = String.Empty;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new HttpRequestException("Missing radar name.");
            if (string.IsNullOrWhiteSpace(Id))
                throw new HttpRequestException("Missing radar id.");
            if (!Guid.TryParse(Id, out _))
                throw new BadRequestException("invalid device id provided.");            
        }
    }

    [HttpPost]
    public string AddRadarDevice([FromBody] AddRadarDeviceArgs args)
    {
        args.Validate();
        RadarDevice device = new RadarDevice();
        device.Id = args.Id;
        device.Name = args.Name;
        device.Description = args.Description;

        DeviceContext.Instance.AddDevice(device);
        return device.Id;
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

    public class SetDeviceNetworkArgs
    {
        [JsonPropertyName("ip")]
        public string ipAddress { get; set; } = String.Empty;

        [JsonPropertyName("subnet")]
        public string subnetMask { get; set; } = String.Empty;

        [JsonPropertyName("gateway")]
        public string gwAddress { get; set; } = String.Empty;

        [JsonPropertyName("static_ip")]
        public bool? staticIP { get; set; }


        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ipAddress) || !IPAddress.TryParse(ipAddress, out _))
                throw new BadRequestException("invalid IP provided");
            if (string.IsNullOrWhiteSpace(subnetMask) || !IPAddress.TryParse(subnetMask, out _))
                throw new BadRequestException("invalid subnet provided");
            if (string.IsNullOrWhiteSpace(gwAddress) || !IPAddress.TryParse(gwAddress, out _))
                throw new BadRequestException("invalid gateway address provided");
            if (staticIP == null)
                throw new BadRequestException("static IP option not provided");
        }
    }

    [HttpPut("{deviceId}/network")]
    public void SetDeviceNetwork(string deviceId, [FromBody] SetDeviceNetworkArgs args)
    {
        ValidateDeviceId(deviceId); 
        args.Validate();
        IPRadarClient.SetDeviceNetwork(deviceId, args.ipAddress, args.subnetMask, args.gwAddress, args.staticIP!.Value);
    }
    
    public class SetRadarConfigArgs
    {
        [JsonPropertyName("config")]
        public List<string>? Config { get; set; }

        public void Validate()
        {
            if (Config == null)
                throw new BadRequestException("missing radar configuration.");
        }
    }

    [HttpPost("{deviceId}/config")]
    public void SetRadarConfig(string deviceId, [FromBody] SetRadarConfigArgs args)
    {
        ValidateDeviceId(deviceId); 
        args.Validate();
        var action = new SetRadarConfigAction(deviceId, args.Config!);
        action.Run();
    }

}
