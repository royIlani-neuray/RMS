using WebService.Entites;
using WebService.Context;
using WebService.Radar;
using Microsoft.AspNetCore.Mvc;

namespace WebService.Controllers;

[ApiController]
[Route("device-mapping")]
public class DeviceMappingController : ControllerBase
{
    private readonly ILogger<DeviceMappingController> _logger;

    public DeviceMappingController(ILogger<DeviceMappingController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public List<DeviceMapper.MappedDevice> GetMappedDevices()
    {
        return DeviceMapper.Instance.GetMappedDevices();
    }

    [HttpPost]
    public void TriggerDeviceMapping()
    {
        DeviceMapper.Instance.MapDevices();
    }

}
