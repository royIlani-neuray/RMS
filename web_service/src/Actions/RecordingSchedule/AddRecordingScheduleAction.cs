/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.Context;
using WebService.Entites;

namespace WebService.Actions.RecordingSchedules;

public class AddRecordingScheduleArgs 
{
    [JsonPropertyName("name")]
    public String Name { get; set; }

    [JsonPropertyName("enabled")]
    public Boolean Enabled { get; set; }

    [JsonPropertyName("radars")]
    public List<String> Radars { get; set; }

    [JsonPropertyName("cameras")]
    public List<String> Cameras { get; set; }

    [JsonPropertyName("start_days")]
    public List<DayOfWeek> StartDays { get; set; }

    [JsonRequired]
    [JsonPropertyName("start_time")]
    public TimeOnly StartTime { get; set; }

    [JsonPropertyName("end_days")]
    public List<DayOfWeek> EndDays { get; set; }

    [JsonRequired]
    [JsonPropertyName("end_time")]
    public TimeOnly EndTime { get; set; }

    [JsonPropertyName("upload_s3")]
    public bool UploadS3 { get; set; }

    public AddRecordingScheduleArgs()
    {
        Name = String.Empty;
        Enabled = true;
        Radars = new List<String>();
        Cameras = new List<String>();
        StartDays = new List<DayOfWeek>();
        EndDays = new List<DayOfWeek>();
        UploadS3 = false;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new HttpRequestException("Name not defined");
        if (StartDays.Count == 0)
            throw new HttpRequestException("Start days not defined");
        if (StartDays.Count != EndDays.Count)
            throw new HttpRequestException("Number of start days and end days should be equal");
        if (Radars.Count + Cameras.Count == 0)
            throw new HttpRequestException("Recording devices not defined");
    }
}

public class AddRecordingScheduleAction : IAction 
{
    AddRecordingScheduleArgs args;

    public AddRecordingScheduleAction(AddRecordingScheduleArgs args)
    {
        this.args = args;
    }

    public void Run()
    {
        args.Validate();

        RecordingSchedule schedule = new()
        {
            Name = args.Name,
            Enabled = args.Enabled,
            RadarIds = args.Radars,
            CameraIds = args.Cameras,
            StartDays = args.StartDays,
            StartTime = args.StartTime,
            EndDays = args.EndDays,
            EndTime = args.EndTime,
            UploadS3 = args.UploadS3,
        };

        System.Console.WriteLine($"Adding new schedule - {schedule.Name}");
 
        RecordingScheduleContext.Instance.AddSchedule(schedule);

        System.Console.WriteLine($"Recording Schedule added.");
    }
}