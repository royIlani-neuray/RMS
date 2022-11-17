import { RadarSettings } from "./radar-settings"

export interface RadarDeviceBrief {
    name: string
    state: string
    description: string
    device_id: string
    enabled: boolean,
    send_tracks_report: boolean
}

export interface RadarDevice {
    name: string
    state: string
    status: string
    description: string
    device_id: string
    enabled: boolean
    send_tracks_report: boolean

    config_script: string[]

    device_mapping: DeviceMapping
    radar_settings: RadarSettings
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
    fw_version: string
}

