/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;

namespace WebService.Services;

public class ExtensionServiceSettings 
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = String.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = String.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = String.Empty;

    [JsonPropertyName("host")]
    public string Host { get; set; } = String.Empty;

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

}