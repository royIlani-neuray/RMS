/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
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
