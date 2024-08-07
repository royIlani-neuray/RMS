/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Net.Sockets;

namespace WebService.RadarLogic.IPRadar.Connection;

public interface IRadarConnection
{
    public const int IP_RADAR_CONTROL_PORT = 7001;
    public const int IP_RADAR_DATA_PORT = 7002;
    public const int IP_RADAR_BROADCAST_PORT_SERVER = 7003;
    public const int IP_RADAR_BROADCAST_PORT_DEVICE = 7004;

    public bool IsRadarConnected { get; }

    public TcpClient? ControlStream { get; }
    public TcpClient? DataStream { get; }

    public void Connect();

    public void Disconnect();
}
