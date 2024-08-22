/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Context;
using Serilog;

namespace WebService.Actions.RecordingSchedules;

public class DeleteRecordingScheduleAction : RecordingScheduleAction 
{
    public DeleteRecordingScheduleAction(string scheduleId) : base(scheduleId) {}

    protected override void RunRecordingScheduleAction(RecordingSchedule schedule)
    {
        Log.Information($"Deleting schedule - {schedule.Id}");
        RecordingScheduleContext.Instance.DeleteSchedule(schedule);
    }
}