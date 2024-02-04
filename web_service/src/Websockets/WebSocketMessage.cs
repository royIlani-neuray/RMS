/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;

namespace WebService.WebSockets;

public class WebSocketMessage 
{
    [JsonPropertyName("type")]
    public String MessageType { get; set; } = String.Empty;

    [JsonPropertyName("data")]
    public Object MessageData { get; set; } = new Object();
}