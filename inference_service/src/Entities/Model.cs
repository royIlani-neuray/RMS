/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System;
using System.Text.Json.Serialization;
using Microsoft.ML.OnnxRuntime;

namespace InferenceService.Entities;

public class Model
{
    public enum ModelTypes
    {
        Unknown,
        GateId,
        PoseEstimation
    }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ModelTypes ModelType { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("registered_at")]
    public DateTime RegisteredAt { get; set; }

    [JsonPropertyName("settings")]
    public Dictionary<string,string> Settings { get; set; }

    [JsonPropertyName("labels")]
    public List<string> Labels { get; set; }

    [JsonIgnore]
    public InferenceSession? Session { get; set; }

    public class ModelBrief 
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ModelTypes ModelType { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("registered_at")]
        public DateTime RegisteredAt { get; set; }

        public ModelBrief(Model model)
        {
            Name = model.Name;
            ModelType = model.ModelType;
            Description = model.Description;
            RegisteredAt = model.RegisteredAt;
        }
    }

    public Model()
    {
        Name = string.Empty;
        ModelType = ModelTypes.Unknown;
        Description = string.Empty;
        RegisteredAt = DateTime.UtcNow;
        Labels = new List<string>();
        Settings = new Dictionary<string, string>();
    }
}