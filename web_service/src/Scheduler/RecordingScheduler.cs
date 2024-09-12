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
using WebService.Actions.Recordings;
using Serilog;

namespace WebService.Scheduler;

public class RecordingScheduler : TaskScheduler{

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile RecordingScheduler? instance; 

    public static RecordingScheduler Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new RecordingScheduler();
                }
            }

            return instance;
        }
    }

    private RecordingScheduler() : base(SCHEDULING_PERIOD_MINUTES) {}

    #endregion

    private const String SCHEDULED_NAME_PREFIX = "SCH";

    private const double SCHEDULING_PERIOD_MINUTES = 1;
    private const int CHECK_LAST_MINUTES = 2;  // to not miss events while sleeping over the exact minute

    public void RestoreSchedules()
    {
        DateTime now = DateTime.UtcNow;
        var schedulesList = RecordingScheduleContext.Instance.GetSchedules();
        foreach (var schedule in schedulesList)
        {
            if (schedule.LastStart > schedule.LastEnd) {
                schedule.LastEnd = now;
                RecordingScheduleContext.Instance.UpdateSchedule(schedule);
                if (schedule.Enabled && IsTimeDuringSchedule(schedule, now)) {
                    Log.Information($"Schedule {schedule.Name} got terminated, restoring it");
                    StartSchedule(schedule, now);
                }
            }
        }
    }

    private bool IsTimeDuringSchedule(RecordingSchedule schedule, DateTime time)
    {
        DayOfWeek timeDay = time.DayOfWeek;
        TimeOnly timeTime = TimeOnly.FromDateTime(time);
        bool startedOnTimeDay = false, endedOnTimeDay = false;
        if (schedule.StartDays.Contains(timeDay) && schedule.StartTime < timeTime) {
            startedOnTimeDay = true;
        }
        if (schedule.EndDays.Contains(timeDay) && schedule.EndTime < timeTime) {
            endedOnTimeDay = true;
        }
        if (startedOnTimeDay && !endedOnTimeDay) return true;
        if (endedOnTimeDay && !startedOnTimeDay) return false;
        if (endedOnTimeDay && startedOnTimeDay) return schedule.StartTime > schedule.EndTime;

        int dayBefore = Mod((int)timeDay - 1, 7);
        int DaysFromLastEndToDayBefore = schedule.EndDays.Select(endDay => Mod(dayBefore - (int)endDay, 7)).Min(); 
        int DaysFromLastStartToDayBefore = schedule.StartDays.Select(startDay => Mod(dayBefore - (int)timeDay, 7)).Min(); 
        if (DaysFromLastEndToDayBefore > DaysFromLastStartToDayBefore) return true;
        if (DaysFromLastEndToDayBefore < DaysFromLastStartToDayBefore) return false;
        return schedule.StartTime > schedule.EndTime;
    }

    public static int Mod(int dividend, int divisor)
    {
        return ((dividend % divisor) + divisor) % divisor;
    }

    private bool ShouldStart(RecordingSchedule schedule, DateTime now)
    {
        DateTime startTime = now.Date.Add(schedule.StartTime.ToTimeSpan());
        return schedule.Enabled &&
            schedule.StartDays.Contains(now.DayOfWeek) &&
            startTime <= now &&
            startTime >= now.AddMinutes(-CHECK_LAST_MINUTES) &&
            schedule.LastStart.AddMinutes(CHECK_LAST_MINUTES) <= now;
    }

    private bool ShouldStop(RecordingSchedule schedule, DateTime now)
    {
        DateTime endTime = now.Date.Add(schedule.EndTime.ToTimeSpan());
        return schedule.Enabled &&
            schedule.EndDays.Contains(now.DayOfWeek) &&
            endTime <= now &&
            endTime >= now.AddMinutes(-CHECK_LAST_MINUTES) &&
            schedule.LastEnd.AddMinutes(CHECK_LAST_MINUTES) <= now;
    }

    private void StartSchedule(RecordingSchedule schedule, DateTime now)
    {
        Log.Information($"Recording Scheduler: Starting scheduled recording - {schedule.Name}");
        schedule.LastStart = now;
        RecordingScheduleContext.Instance.UpdateSchedule(schedule);
        String recordingName = $"{SCHEDULED_NAME_PREFIX}_{schedule.Name}_{now:yyyy-MM-dd_HH-mm-ss.fff}";
        StartRecordingArgs args = new()
        {
            RecordingName = recordingName,
            RadarIds = schedule.RadarIds,
            CameraIds = schedule.CameraIds,
            UploadS3 = schedule.UploadS3,
        };
        var action = new StartRecordingAction(args);
        action.Run();
    }

    private void StopSchedule(RecordingSchedule schedule, DateTime now)
    {
        Log.Information($"Recording Scheduler: Stopping scheduled recording - {schedule.Name}");
        schedule.LastEnd = now;
        RecordingScheduleContext.Instance.UpdateSchedule(schedule);
        StopRecordingArgs args = new()
        {
            RadarIds = schedule.RadarIds,
            CameraIds = schedule.CameraIds,
        };
        var action = new StopRecordingAction(args);
        action.Run();
    }

    public override void RunTask()
    {
        DateTime now = DateTime.UtcNow;
        Log.Information($"Recording Scheduler: Checking for recording schedules... now: {now.DayOfWeek}, {now}");
        var schedulesList = RecordingScheduleContext.Instance.GetSchedules();
        foreach (var schedule in schedulesList)
        {
            if (ShouldStop(schedule, now))
            {
                StopSchedule(schedule, now);
            }
            if (ShouldStart(schedule, now))
            {
                StartSchedule(schedule, now);
            }
        }
    }
}