/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/

export interface UserBrief {
    user_id: string
    first_name: string
    last_name: string
    email: string
}

export interface User {
    user_id: string
    first_name: string
    last_name: string
    email: string
    employee_id: string
    roles: string[]
    registered_at: string
}