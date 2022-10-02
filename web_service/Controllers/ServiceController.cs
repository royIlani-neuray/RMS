using WebService.Entites;
using WebService.Context;
using WebService.Radar;
using Microsoft.AspNetCore.Mvc;

namespace WebService.Controllers;

[ApiController]
[Route("")]
public class ServiceController : ControllerBase
{
    private readonly ILogger<ServiceController> _logger;

    public ServiceController(ILogger<ServiceController> logger)
    {
        _logger = logger;
    }

}
