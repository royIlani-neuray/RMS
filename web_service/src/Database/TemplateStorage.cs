/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using System.Text.Json;

namespace WebService.Database;

public class TemplateStorage {

    public static readonly string StoragePath = "./data/templates";
    public static readonly string TemplateFileExtention = ".json";
    
    public static void SaveTemplate(RadarTemplate template)
    {
        string jsonString = JsonSerializer.Serialize(template);
        File.WriteAllText(System.IO.Path.Combine(StoragePath, template.Id + TemplateFileExtention), jsonString);
    }

    public static void DeleteTemplate(RadarTemplate template)
    {
        string filePath = System.IO.Path.Combine(StoragePath, template.Id + TemplateFileExtention);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        else
        {
            Console.WriteLine($"Warning: could not delete template file for template: {template.Id}");
        }
    }

    public static Dictionary<string, RadarTemplate> LoadAllTemplates()
    {
        Dictionary<string, RadarTemplate> templates = new Dictionary<string, RadarTemplate>();

        var files = System.IO.Directory.GetFiles(StoragePath, "*" + TemplateFileExtention);
        foreach (string filePath in files)
        {
            string jsonString = File.ReadAllText(filePath);
            
            RadarTemplate? template = JsonSerializer.Deserialize<RadarTemplate>(jsonString);

            if (template == null)
                throw new Exception("deserialze failed!");

            templates.Add(template.Id, template);
        }

        Console.WriteLine($"Loaded {templates.Keys.Count} templates from storage.");
        return templates;
    }
}