/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json;
using System.Text.Json.Serialization;

public class DeviceInfo 
{
    [JsonPropertyName("name")]
    public String Name { get; set; } = String.Empty;

    [JsonPropertyName("device_id")]
    public String Id { get; set; } = String.Empty;

    [JsonPropertyName("config_script")]
    public List<string> ConfigScript { get; set; } = new List<string>();

    public static DeviceInfo LoadFromFile(string filePath)
    {
        string jsonString = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<DeviceInfo>(jsonString)!;
    }
}