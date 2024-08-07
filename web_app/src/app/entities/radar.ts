/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { RadarSettings } from "./radar-settings"

export interface RadarBrief {
    name: string
    state: string
    description: string
    device_id: string
    enabled: boolean,
    send_tracks_report: boolean
    is_recording: boolean
}

export interface Radar {
    name: string
    state: string
    status: string
    description: string
    device_id: string
    enabled: boolean
    send_tracks_report: boolean
    is_recording: boolean

    config_script: string[]

    device_mapping: DeviceMapping
    radar_settings: RadarSettings
    linked_services: LinkedService[]
}

export interface DeviceMapping {
    ip: string
    subnet: string
    gateway: string
    device_id: string
    model: string
    application: string
    static_ip: boolean,
    registered: boolean,
    fw_version: string,
    remote_device: boolean
}

export interface LinkedService {
    service_id: string
    service_options: Map<string, string>
}
