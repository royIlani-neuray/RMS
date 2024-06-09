/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Context;
using WebService.Entites;

namespace WebService.Actions;

public abstract class RecordingScheduleAction : EntityAction<RecordingSchedule>
{
    public RecordingScheduleAction(string scheduleId) : base(RecordingScheduleContext.Instance, scheduleId) {}

    protected abstract void RunRecordingScheduleAction(RecordingSchedule schedule);

    protected override void RunAction(RecordingSchedule schedule)
    {
        RunRecordingScheduleAction(schedule);
    }

    protected override void RunPostActionTask(RecordingSchedule schedule)
    {
    }
}