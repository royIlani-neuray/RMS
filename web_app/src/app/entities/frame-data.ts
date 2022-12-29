/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/

export interface FrameData {
    device_id: string
    device_name: string
    timestamp: string

    tracks: TrackData[]
    points: PointData[]
    targets_index: number[]
}

export interface TrackData {
    track_id: number
    position_x : number
    position_y : number
    position_z : number
    velocity_x : number
    velocity_y : number
    velocity_z : number
    acceleration_x : number
    acceleration_y : number
    acceleration_z : number
}

export interface PointData {
    range: number
    azimuth : number
    elevation : number
    doppler : number
    snr : number
    position_x : number
    position_y : number
    position_z : number
}