
export interface RadarDeviceBrief {
    name: string
    state: string
    description: string
    device_id: string
    enabled: boolean
}

export interface RadarDevice {
    name: string
    state: string
    description: string
    device_id: string
    enabled: boolean

    config: string[]

    device_mapping: DeviceMapping
}

export interface DeviceMapping {
    ip: string
    subnet: string
    gateway: string
    device_id: string
    model: string
    application: string
    static_ip: string
}
