/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebService.Entites;

namespace WebService.Recordings;

public class RecordingInfo
{
    public class RecordingEntry
    {
        [JsonPropertyName("device_name")]
        public String DeviceName { get; set; } = String.Empty;

        [JsonPropertyName("device_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DeviceEntity.DeviceTypes DeviceType { get; set; }      

        [JsonPropertyName("device_id")]
        public String DeviceId { get; set; } = String.Empty;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("is_finished")]
        public bool? IsFinished { get; set; }

        [JsonPropertyName("entry_size_bytes")]
        public float EntrySizeBytes { get; set; } = 0;
    }

    [JsonPropertyName("name")]
    public String Name { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("entries")]
    public List<RecordingEntry> RecordingEntries { get; set; }

    [JsonPropertyName("upload_s3")]
    public bool UploadS3 { get; set; } = false;

    [JsonPropertyName("last_uploaded")]
    public DateTime LastUploaded { get; set; }

    [JsonPropertyName("is_uploading")]
    public bool IsUploading { get; set; } = false;

    public RecordingInfo()
    {
        CreatedAt = DateTime.UtcNow;
        Name = CreatedAt.ToString("yyyy-MM-dd_HH-mm-ss.fff", CultureInfo.InvariantCulture);
        RecordingEntries = new List<RecordingEntry>();
    }

    public static RecordingInfo LoadFromFile(string filePath)
    {
        string jsonString = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<RecordingInfo>(jsonString)!;
    }

    public void SaveToFile(string filePath)
    {
        var jsonString = JsonSerializer.Serialize<RecordingInfo>(this, new JsonSerializerOptions{WriteIndented=true});
        File.WriteAllText(filePath, jsonString);
    }
}