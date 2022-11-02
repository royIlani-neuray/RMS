import { RadarSettings } from "./radar-settings"

export interface RadarTemplate {
    name: string
    description: string
    template_id: string
    model: string
    application: string

    config_script: string[]
    
    radar_settings: RadarSettings
}

export interface RadarTemplateBrief {
    name: string
    description: string
    template_id: string
    model: string
    application: string
}
