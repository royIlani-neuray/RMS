/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/

export interface CameraBrief {
    name: string
    state: string
    description: string
    device_id: string
    enabled: boolean
    is_recording: boolean
}

export interface Camera {
    name: string
    state: string
    status: string
    description: string
    device_id: string
    enabled: boolean
    rtsp_url: string
    frame_rate: number
    fov_x: number
    fov_y: number
    resolution_x: number
    resolution_y: number
    linked_services: LinkedService[]
    is_recording: boolean
}

export interface LinkedService {
    service_id: string
    service_options: Map<string, string>
}