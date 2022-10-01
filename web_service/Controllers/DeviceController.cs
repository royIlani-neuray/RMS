using WebService.Entites;
using WebService.Context;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    [HttpGet]
    public List<RadarDevice.RadarDeviceBrief> GetDevices()
    {
        return DeviceContext.Instance.GetDevicesBrief();
    }

    [HttpGet("{deviceId}")]
    public RadarDevice GetRadarDevice(string deviceId)
    {        
        if (!DeviceContext.Instance.IsRadarDeviceExist(deviceId))
            throw new NotFoundException("There is no device with the provided id");

        return DeviceContext.Instance.GetDevice(deviceId);
    }

    [HttpDelete("{deviceId}")]
    public void DeleteRadarDevice(string deviceId)
    {        

    }

    public class NewDeviceInfo 
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
        }
    }

    [HttpPost]
    public string AddRadarDevice([FromBody] NewDeviceInfo newDeviceInfo)
    {
        string jsonString = JsonSerializer.Serialize(newDeviceInfo);
        _logger.LogInformation($"info: {jsonString}");

        newDeviceInfo.Validate();
        RadarDevice device = new RadarDevice();
        device.Id = newDeviceInfo.Id;
        device.Name = newDeviceInfo.Name;
        device.Description = newDeviceInfo.Description;

        DeviceContext.Instance.AddDevice(device);
        return device.Id;
    }

    [HttpPost("{deviceId}/enable")]
    public void EnableRadarDevice(string deviceId)
    {
        
    }

    [HttpPost("{deviceId}/disable")]
    public void DisableRadarDevice(string deviceId)
    {
        
    }

    [HttpPut("{deviceId}/network")]
    public void SetRadarDeviceNetwork(string deviceId)
    {
        
    }

}
