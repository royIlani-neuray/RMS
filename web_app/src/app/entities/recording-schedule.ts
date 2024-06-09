/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
export interface RecordingSchedule {
    id: string
    name: string
    enabled: boolean
    start_days: number[]
    start_time: string
    end_days: number[]
    end_time: string
    last_start: string
    radars: string[]
    cameras: string[]
}
export interface AddRecordingScheduleArgs {
    name: string
    start_days: number[]
    start_time: string
    end_days: number[]
    end_time: string
    radars: string[]
    cameras: string[]
}