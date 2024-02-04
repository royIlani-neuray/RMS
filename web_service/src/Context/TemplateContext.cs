/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Database;
using WebService.Entites;
using WebService.RadarLogic.IPRadar;

namespace WebService.Context;

public sealed class TemplateContext : EntityContext<RadarTemplate> {

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

    private TemplateContext() : base(IEntity.EntityTypes.RadarTemplate) {}

    #endregion

    public void LoadTemplatesFromStorage()
    {
        LoadEntitiesFromStorage(StorageDatabase.TemplateStoragePath);

        // default templates doesn't have thier radar setting serialized
        foreach (var template in entities.Values)
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
        return IsEntityExist(templateId);
    }

    public RadarTemplate GetTemplate(string templateId)
    {
        return GetEntity(templateId);
    }

    public void AddTemplate(RadarTemplate template)
    {
        AddEntity(template);
    }

    public void UpdateTemplate(RadarTemplate template)
    {
        UpdateEntity(template);
    }

    public void DeleteTemplate(RadarTemplate template)
    {
        DeleteEntity(template);
    }

    public List<RadarTemplate.RadarTemplateBrief> GetTemplatesBrief()
    {
        return entities.Values.ToList().ConvertAll<RadarTemplate.RadarTemplateBrief>(template => new RadarTemplate.RadarTemplateBrief(template));
    }
}