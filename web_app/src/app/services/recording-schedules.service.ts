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

import TIMES from '../utils/times';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RecordingSchedulesService {

  constructor(private http:HttpClient) { }

  public getSchedule(scheduleId : string)
  {
    return this.http.get<RecordingSchedule>("/api/schedules/" + scheduleId).pipe(
      tap(
        (schedule: RecordingSchedule) => this.convertFromUTC(schedule)
      )
    );
  }

  public getSchedules()
  {
    return this.http.get<RecordingSchedule[]>("/api/schedules").pipe(
      tap(
        (schedules: RecordingSchedule[]) => schedules.forEach(
          (schedule: RecordingSchedule) => this.convertFromUTC(schedule)
        )
      )
    );
  }

  public addSchedule(schedule: AddRecordingScheduleArgs)
  {
    let scheduleCopy = JSON.parse(JSON.stringify(schedule))  // to not change the user view of the schedule
    this.convertToUTC(scheduleCopy);
    return this.http.post("/api/schedules/", scheduleCopy)
  }

  public deleteSchedule(scheduleId: string)
  {
    return this.http.delete("/api/schedules/" + scheduleId)
  }

  public updateSchedule(scheduleId: string, name?: string, enabled?: boolean, upload_s3?: boolean)
  {
    return this.http.post("/api/schedules/" + scheduleId, {
      "name": name,
      "enabled": enabled,
      "upload_s3": upload_s3,
    })
  }

  public convertToUTC(schedule: AddRecordingScheduleArgs) {
    const [startTimeUTC, startDaysShift] = TIMES.getTimeAndDayShiftOnToUTCConversion(schedule.start_time);
    schedule.start_time = startTimeUTC;
    schedule.start_days = schedule.start_days.map(TIMES.dayShiftFunc(startDaysShift));
    const [endTimeUTC, endDaysShift] = TIMES.getTimeAndDayShiftOnToUTCConversion(schedule.end_time);
    schedule.end_time = endTimeUTC;
    schedule.end_days = schedule.end_days.map(TIMES.dayShiftFunc(endDaysShift));
  }

  public convertFromUTC(schedule: RecordingSchedule) {
    const [startTimeUTC, startDaysShift] = TIMES.getTimeAndDayShiftOnFromUTCConversion(schedule.start_time);
    schedule.start_time = startTimeUTC;
    schedule.start_days = schedule.start_days.map(TIMES.dayShiftFunc(startDaysShift));
    const [endTimeUTC, endDaysShift] = TIMES.getTimeAndDayShiftOnFromUTCConversion(schedule.end_time);
    schedule.end_time = endTimeUTC;
    schedule.end_days = schedule.end_days.map(TIMES.dayShiftFunc(endDaysShift));
  }
}
