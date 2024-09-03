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
using WebService.Recordings;

namespace WebService.Scheduler;

public class RecordingsRetentionScheduler : TaskScheduler{

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile RecordingsRetentionScheduler? instance; 

    public static RecordingsRetentionScheduler Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new RecordingsRetentionScheduler();
                }
            }

            return instance;
        }
    }

    private RecordingsRetentionScheduler() : base(SCHEDULING_PERIOD_MINUTES) {}

    #endregion

    private const double SCHEDULING_PERIOD_MINUTES = 60 * 24;  // 1 day

    public override void RunTask()
    {
        RMSSettings.RecordingsRetention recordingsRetentionSettings = RMSSettings.Instance.RecordingsRetentionSettings;
        if (!recordingsRetentionSettings.Enabled) {
            Log.Information($"Recordings Retention Scheduler: disabled, doing nothing");
            return;
        }
        Log.Information($"Recordings Retention Scheduler: started, looking for expired recordings...");
        List<RecordingInfo> recordings = RecordingsManager.Instance.GetRecordings();
        DateTime expiredThreshold = DateTime.Now.AddDays(-recordingsRetentionSettings.ExpirationDays);

        Log.Information($"Recordings Retention Scheduler: filtering older than {recordingsRetentionSettings.ExpirationDays} days");
        List<RecordingInfo> recordingsToDelete = recordings.FindAll(Recording => Recording.CreatedAt < expiredThreshold);
        if (recordingsRetentionSettings.DeleteUploadedOnly) {
            Log.Information($"Recordings Retention Scheduler: filtering only recordings that were uploaded to cloud");
            recordingsToDelete = recordingsToDelete.FindAll(Recording => Recording.LastUploaded != DateTime.MinValue);
        }

        Log.Information($"Recordings Retention Scheduler: deleting {recordingsToDelete.Count} recordings");
        foreach (RecordingInfo recording in recordingsToDelete) {
            Log.Information($"Recordings Retention Scheduler: deleting an expired recording - {recording.Name}");
            RecordingsManager.Instance.DeleteRecording(recording.Name);
        }
        Log.Information($"Recordings Retention Scheduler: done deleting expired recordings");
    }
}