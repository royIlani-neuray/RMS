/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;

namespace WebService.Services.Inference.GateId;

public class GateIdRequest
{
    public class GateIdFrameInput 
    {
        [JsonPropertyName("x_axis")]
        public List<float> xAxis = new List<float>();

        [JsonPropertyName("y_axis")]
        public List<float> yAxis = new List<float>();

        [JsonPropertyName("z_axis")]
        public List<float> zAxis = new List<float>();

        [JsonPropertyName("velocity")]
        public List<float> Velocity = new List<float>();

        [JsonPropertyName("intensity")]
        public List<float> Intensity = new List<float>();
    }

    [JsonPropertyName("frames")]
    public List<GateIdFrameInput> Frames;

    public GateIdRequest()
    {
        Frames = new List<GateIdFrameInput>();
    }
}