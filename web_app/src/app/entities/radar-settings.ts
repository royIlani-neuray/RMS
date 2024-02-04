/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/

export interface RadarSettings {
    boundary_box: BoundaryBoxParams,
    static_boundary_box: BoundaryBoxParams,
    presence_boundary_box: BoundaryBoxParams,
    sensor_position: SensorPositionParams,
    allocation_params: AllocationParams,
    gating_params: GatingParams,
    detection_params: DetectionParams,
    radar_calibration : string
}

export interface BoundaryBoxParams {
    x_min: number
    x_max: number
    y_min: number
    y_max: number
    z_min: number
    z_max: number
}

export interface SensorPositionParams {
    height: number
    azimuth_tilt: number
    elevation_tilt: number
}

export interface AllocationParams {
    snr_threshold: number
    snr_threshold_obscured: number
    velocity_threshold: number
    points_threshold: number
    max_distance_threshold: number
    max_velocity_threshold: number
}

export interface GatingParams {
    gain: number
    limit_width: number
    limit_depth: number
    limit_height: number
    limit_velocity: number
}

export interface DetectionParams {
    range_resolution : number
    velocity_resolution : number
    max_range : number
    max_velocity : number
    frame_rate : number
    frame_size : number
    tx_count : number
    rx_count : number
}

