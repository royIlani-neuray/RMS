/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;

namespace WebService.Security;

public class AuthRequest()
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = String.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = String.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Email))
            throw new HttpRequestException("Email not defined");
        if (string.IsNullOrWhiteSpace(Password))
            throw new HttpRequestException("Password not defined");
    }
}