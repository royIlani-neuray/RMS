/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AddRecordingScheduleArgs, RecordingSchedule } from '../entities/recording-schedule';

@Injectable({
  providedIn: 'root'
})
export class RecordingSchedulesService {

  constructor(private http:HttpClient) { }

  public getSchedule(scheduleId : string)
  {
    return this.http.get<RecordingSchedule>("/api/schedules/" + scheduleId)
  }

  public getSchedules()
  {
    return this.http.get<RecordingSchedule[]>("/api/schedules")
  }

  public addSchedule(schedule: AddRecordingScheduleArgs)
  {
    return this.http.post("/api/schedules/", schedule)
  }

  public deleteSchedule(scheduleId: string)
  {
    return this.http.delete("/api/schedules/" + scheduleId)
  }

  public updateSchedule(scheduleId: string, name?: string, enabled?: boolean)
  {
    return this.http.post("/api/schedules/" + scheduleId, {
      "name": name,
      "enabled": enabled,
    })
  }

}
