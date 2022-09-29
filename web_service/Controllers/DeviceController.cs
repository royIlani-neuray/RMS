using WebService.Entites;
using WebService.Context;
using Microsoft.AspNetCore.Mvc;

namespace WebService.Controllers;

[ApiController]
[Route("devices")]
public class DeviceController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;

    public DeviceController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    public class NewDeviceInfo {
        public string Id = String.Empty;
        public string Name = String.Empty;
        public string Description = String.Empty;

        public void Validate()
        {
            if (Name == String.Empty)
                throw new HttpRequestException("Missing radar name.");
            if (Id == String.Empty)
                throw new HttpRequestException("Missing radar id.");            
        }
    }

    [HttpGet]
    public List<RadarDevice.RadarDeviceBrief> GetDevices()
    {
        Console.WriteLine("in GetDevices!");
        return DeviceContext.Instance.GetDevicesBrief();
    }

    [HttpGet("{deviceId}")]
    public RadarDevice GetRadarDevice(string deviceId)
    {
        Console.WriteLine($"in GetDevice - {deviceId}!");
        
        if (!DeviceContext.Instance.IsRadarDeviceExist(deviceId))
            throw new NotFoundException("There is no device with the provided id");

        return DeviceContext.Instance.GetDevice(deviceId);
    }

    [HttpPost]
    public string AddRadarDevice(NewDeviceInfo newDeviceInfo)
    {
        newDeviceInfo.Validate();
        RadarDevice device = new RadarDevice();
        device.Id = newDeviceInfo.Id;
        device.Name = newDeviceInfo.Name;
        device.Description = newDeviceInfo.Description;

        DeviceContext.Instance.AddDevice(device);
        return device.Id;
    }
}
