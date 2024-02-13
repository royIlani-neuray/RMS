/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;

public class GateIdRequest
{
    public class GateIdFrameInput 
    {
        [JsonPropertyName("azimuth")]
        public List<float> Azimuth { get; set; } = new List<float>();

        [JsonPropertyName("elevation")]
        public List<float> Elevation { get; set; } = new List<float>();

        [JsonPropertyName("range")]
        public List<float> Range { get; set; } = new List<float>();

        [JsonPropertyName("velocity")]
        public List<float> Velocity { get; set; } = new List<float>();

        [JsonPropertyName("intensity")]
        public List<float> Intensity { get; set; } = new List<float>();
    }

    [JsonPropertyName("frames")]
    public List<GateIdFrameInput> Frames { get; set; }

    public GateIdRequest()
    {
        Frames = new List<GateIdFrameInput>();
    }
}