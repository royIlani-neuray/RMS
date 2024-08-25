/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
namespace WebService.Security;

public class JWTSettings()
{
    public string Secret { get; set; } = String.Empty;
    public string Issuer { get; set; } = String.Empty;
    public string Audience { get; set; } = String.Empty;
    public int TokenExpiryInMinutes { get; set; } = 0;
}