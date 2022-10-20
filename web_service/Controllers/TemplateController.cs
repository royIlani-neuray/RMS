using WebService.Entites;
using WebService.Context;
using WebService.Radar;
using WebService.Actions.Radar;
using WebService.Actions.Template;
using WebService.Tracking;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebService.Controllers;

[ApiController]
[Route("templates")]
public class TemplateController : ControllerBase
{
    private readonly ILogger<TemplateController> _logger;

    public TemplateController(ILogger<TemplateController> logger)
    {
        _logger = logger;
    }

    private void ValidateTemplateId(string templateId)
    {
        if (string.IsNullOrWhiteSpace(templateId) || !Guid.TryParse(templateId, out _))
            throw new BadRequestException("invalid template id provided.");
    }

    [HttpGet]
    public List<RadarTemplate.RadarTemplateBrief> GetTemplates()
    {
        return TemplateContext.Instance.GetTemplatesBrief();
    }

    [HttpGet("{templateId}")]
    public RadarTemplate GetRadarDevice(string templateId)
    {
        ValidateTemplateId(templateId);        
        if (!DeviceContext.Instance.IsRadarDeviceExist(templateId))
            throw new NotFoundException("There is no template with the provided id");

        return TemplateContext.Instance.GetTemplate(templateId);
    }

    [HttpPost]
    public void AddRadarDevice([FromBody] AddTemplateArgs args)
    {
        AddTemplateAction action = new AddTemplateAction(args);
        action.Run();
        return;
    }

    [HttpDelete("{templateId}")]
    public void DeleteRadarTemplate(string templateId)
    {        
        ValidateTemplateId(templateId); 
        var action = new DeleteTemplateAction(templateId);
        action.Run();
    }

}