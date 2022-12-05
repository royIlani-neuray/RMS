/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Database;
using WebService.Entites;
using WebService.Radar;

namespace WebService.Context;

public sealed class TemplateContext {

    private static Dictionary<string, RadarTemplate> templates = new Dictionary<string, RadarTemplate>();

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile TemplateContext? instance; 

    public static TemplateContext Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new TemplateContext();
                }
            }

            return instance;
        }
    }

    private TemplateContext() {}

    #endregion

    public void LoadTemplatesFromStorage()
    {
        templates = new Dictionary<string, RadarTemplate>(TemplateStorage.LoadAllTemplates());

        // default templates doesn't have thier radar setting serialized
        foreach (var template in templates.Values)
        {
            if (template.radarSettings != null)
                continue;

            try
            {
                RadarConfigParser configParser = new RadarConfigParser(template.ConfigScript);
                template.radarSettings = configParser.GetRadarSettings();
            }
            catch
            {
                System.Console.WriteLine($"Error: failed to parse config script for template - {template.Id}");
            }
        }
    }

    public bool IsRadarTemplateExist(string templateId)
    {
        if (templates.Keys.Contains(templateId))
            return true;
        
        return false;
    }

    public RadarTemplate GetTemplate(string templateId)
    {
        if (!IsRadarTemplateExist(templateId))
            throw new NotFoundException($"Could not find template in context with id - {templateId}");

        return templates[templateId];
    }

    public void AddTemplate(RadarTemplate template)
    {
        if (IsRadarTemplateExist(template.Id))
            throw new Exception("Cannot add template. Another template with the same ID already exist.");

        TemplateStorage.SaveTemplate(template);
        templates.Add(template.Id, template);
    }

    public void UpdateTemplate(RadarTemplate template)
    {
        if (!IsRadarTemplateExist(template.Id))
            throw new NotFoundException($"Could not find template in context with id - {template.Id}");

        TemplateStorage.SaveTemplate(template);
    }

    public void DeleteTemplate(RadarTemplate template)
    {
        GetTemplate(template.Id); // make sure template enlisted

        TemplateStorage.DeleteTemplate(template);
        templates.Remove(template.Id);
    }

    public List<RadarTemplate.RadarTemplateBrief> GetTemplatesBrief()
    {
        return templates.Values.ToList().ConvertAll<RadarTemplate.RadarTemplateBrief>(template => new RadarTemplate.RadarTemplateBrief(template));
    }
}