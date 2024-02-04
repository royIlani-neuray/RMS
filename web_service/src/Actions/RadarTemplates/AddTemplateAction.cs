/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.Context;
using WebService.Entites;
using WebService.Events;
using WebService.RadarLogic.IPRadar;

namespace WebService.Actions.RadarTemplates;

public class AddTemplateArgs 
{
    [JsonPropertyName("name")]
    public String Name { get; set; }

    [JsonPropertyName("description")]
    public String Description { get; set; }

    [JsonPropertyName("model")]
    public String Model { get; set; }

    [JsonPropertyName("application")]
    public String Application { get; set; }

    [JsonPropertyName("config_script")]
    public List<string> ConfigScript { get; set; }

    public AddTemplateArgs()
    {
        Name = String.Empty;
        Description = String.Empty;
        Model = String.Empty;
        Application = String.Empty;
        ConfigScript = new List<string>();
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new HttpRequestException("Template name not defined");
        if (string.IsNullOrWhiteSpace(Application))
            throw new HttpRequestException("Template application not defined");
        if (string.IsNullOrWhiteSpace(Model))
            throw new HttpRequestException("Template model not defined");
    }
}

public class AddTemplateAction : IAction 
{
    AddTemplateArgs args;

    public AddTemplateAction(AddTemplateArgs args)
    {
        this.args = args;
    }

    public void Run()
    {
        args.Validate();

        RadarTemplate template = new RadarTemplate();
        template.Name = args.Name;
        template.Description = args.Description;
        template.Model = args.Model;
        template.Application = args.Application;
        template.ConfigScript = args.ConfigScript;

        RadarConfigParser configParser = new RadarConfigParser(template.ConfigScript);
        template.radarSettings = configParser.GetRadarSettings();

        System.Console.WriteLine($"Adding new template - [{template.Name}]");
 
        TemplateContext.Instance.AddTemplate(template);

        System.Console.WriteLine($"Radar template added.");

        RMSEvents.Instance.TemplateAddedEvent(template.Id);
    }
}