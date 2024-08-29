/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;

namespace WebService.Services.LineCrossing;

public class LineSettings
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("start")]
    public required Point Start { get; set; }

    [JsonPropertyName("end")]
    public required Point End { get; set; }
    
    [JsonPropertyName("enabled")]
    public bool IsEnabled { get; set; } = true;

}

public class Point
{
    [JsonPropertyName("x")]
    public double X { get; set; }
    
    [JsonPropertyName("y")]
    public double Y { get; set; }
}