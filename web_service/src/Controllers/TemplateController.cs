/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Context;
using WebService.Actions.RadarTemplates;
using Microsoft.AspNetCore.Mvc;

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
    public RadarTemplate GetRadarTemplate(string templateId)
    {
        ValidateTemplateId(templateId);        
        if (!TemplateContext.Instance.IsRadarTemplateExist(templateId))
            throw new NotFoundException("There is no template with the provided id");

        return TemplateContext.Instance.GetTemplate(templateId);
    }

    [HttpPost]
    public void AddRadarTemplate([FromBody] AddTemplateArgs args)
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