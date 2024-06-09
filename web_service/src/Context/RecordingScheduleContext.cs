/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Database;
using WebService.Entites;

namespace WebService.Context;

public sealed class RecordingScheduleContext : EntityContext<RecordingSchedule> {

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile RecordingScheduleContext? instance; 

    public static RecordingScheduleContext Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new RecordingScheduleContext();
                }
            }

            return instance;
        }
    }

    private RecordingScheduleContext() : base(IEntity.EntityTypes.RecordingSchedule) {}

    #endregion

    public void LoadSchedulesFromStorage()
    {
        LoadEntitiesFromStorage(StorageDatabase.RecordingSchedulesStoragePath);
    }

    public bool IsScheduleExist(string scheduleId)
    {
        return IsEntityExist(scheduleId);
    }

    public bool IsScheduleNameExist(string name)
    {
        return entities.Values.ToList().Exists(schedule => schedule.Name == name);
    }    

    public RecordingSchedule GetSchedule(string scheduleId)
    {
        return GetEntity(scheduleId);
    }

    public void AddSchedule(RecordingSchedule schedule)
    {
        if (IsScheduleExist(schedule.Id))
            throw new Exception("Cannot add schedule. Another schedule with the same ID already exist.");
        if (IsScheduleNameExist(schedule.Name))
            throw new Exception("Cannot add schedule. Another schedule with the same Name already exist.");

        AddEntity(schedule);
    }

    public void UpdateSchedule(RecordingSchedule schedule)
    {
        UpdateEntity(schedule);
    }

    public void DeleteSchedule(RecordingSchedule schedule)
    {
        DeleteEntity(schedule);
    }

    public List<RecordingSchedule> GetSchedules()
    {
        return entities.Values.ToList();
    }
}