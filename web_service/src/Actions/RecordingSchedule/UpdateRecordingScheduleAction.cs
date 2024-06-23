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

public class UpdateRecordingScheduleArgs 
{
    [JsonPropertyName("name")]
    public String? Name { get; set; }

    [JsonPropertyName("enabled")]
    public Boolean? Enabled { get; set; }

    [JsonPropertyName("upload_s3")]
    public Boolean? UploadS3 { get; set; }
}

public class UpdateRecordingSchedule : IAction 
{
    string scheduleId;
    UpdateRecordingScheduleArgs args;

    public UpdateRecordingSchedule(string scheduleId, UpdateRecordingScheduleArgs args)
    {
        this.scheduleId = scheduleId;
        this.args = args;
    }

    public void Run()
    {
        var schedule = RecordingScheduleContext.Instance.GetSchedule(scheduleId);
        System.Console.WriteLine($"Updating schedule - {schedule.Name}");

        if (args.Name != null && args.Name != schedule.Name) {
            if (RecordingScheduleContext.Instance.IsScheduleNameExist(args.Name))
                throw new Exception($"Cannot update schedule name to {args.Name}. Another schedule with the same Name already exist.");
            System.Console.WriteLine($"Updating schedule - new name: {args.Name}");
            schedule.Name = args.Name;
        }
        if (args.Enabled != null && args.Enabled != schedule.Enabled) {
            System.Console.WriteLine($"Updating schedule - enabled: {args.Enabled}");
            schedule.Enabled = args.Enabled ?? true;
        }
        if (args.UploadS3 != null && args.UploadS3 != schedule.UploadS3) {
            System.Console.WriteLine($"Updating schedule - uploadS3: {args.UploadS3}");
            schedule.UploadS3 = args.UploadS3 ?? true;
        }
 
        RecordingScheduleContext.Instance.UpdateSchedule(schedule);

        System.Console.WriteLine($"Recording Schedule updated.");
    }
}