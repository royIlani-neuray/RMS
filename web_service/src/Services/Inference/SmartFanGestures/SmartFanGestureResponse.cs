/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;

public class SmartFanGestureResponse
{
    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("confidence")]
    public float Confidence { get; set; }

    public SmartFanGestureResponse()
    {
        Label = String.Empty;
        Confidence = -1;
    }
}