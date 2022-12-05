/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;

namespace WebService.Entites;

public class RadarTemplate 
{
    [JsonPropertyName("template_id")]
    public String Id { get; set; }

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

    [JsonPropertyName("radar_settings")]
    public RadarSettings? radarSettings { get; set;}

    [JsonIgnore]
    public ReaderWriterLockSlim templateLock;

    public class RadarTemplateBrief 
    {
        [JsonPropertyName("template_id")]
        public String Id { get; set; }

        [JsonPropertyName("name")]
        public String Name { get; set; }

        [JsonPropertyName("description")]
        public String Description { get; set; }

        [JsonPropertyName("model")]
        public String Model { get; set; }

        [JsonPropertyName("application")]
        public String Application { get; set; }

        public RadarTemplateBrief(RadarTemplate template)
        {
            Name = template.Name;
            Description = template.Description;
            Id = template.Id;
            Model = template.Model;
            Application = template.Application;
        }
    }

    public RadarTemplate()
    {
        Id = Guid.NewGuid().ToString();
        Name = String.Empty;
        Description = String.Empty;
        Model = String.Empty;
        Application = String.Empty;
        ConfigScript = new List<string>();
        templateLock = new ReaderWriterLockSlim();
    }

}