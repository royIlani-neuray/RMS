/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;

public class SmartFanGestureRequest
{
    public class FrameInput 
    {
        [JsonPropertyName("x_axis")]
        public List<float> xAxis { get; set; } = new List<float>();

        [JsonPropertyName("y_axis")]
        public List<float> yAxis { get; set; } = new List<float>();

        [JsonPropertyName("z_axis")]
        public List<float> zAxis { get; set; } = new List<float>();

        [JsonPropertyName("velocity")]
        public List<float> Velocity { get; set; } = new List<float>();

        [JsonPropertyName("intensity")]
        public List<float> Intensity { get; set; } = new List<float>();
    }

    [JsonPropertyName("frames")]
    public List<FrameInput> Frames { get; set; }

    public SmartFanGestureRequest()
    {
        Frames = new List<FrameInput>();
    }
}