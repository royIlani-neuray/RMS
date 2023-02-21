/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
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
    enabled: boolean,
}

export interface Camera {
    name: string
    state: string
    status: string
    description: string
    device_id: string
    enabled: boolean
}
