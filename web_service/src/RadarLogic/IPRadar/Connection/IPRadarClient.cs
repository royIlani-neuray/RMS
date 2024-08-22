/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Serilog;
using WebService.RadarLogic.IPRadar.Connection;

namespace WebService.RadarLogic.IPRadar;

public class IPRadarClient : IRadarConnection
{
    private string ipAddress;
    private TcpClient? controlStream;
    private TcpClient? dataStream;
    private bool isConnected;

    public bool IsRadarConnected => isConnected;
    public TcpClient? ControlStream => controlStream;
    public TcpClient? DataStream => dataStream;

    public IPRadarClient(string localIPAddress)
    {
        this.ipAddress = localIPAddress;
    }

    public void Connect()
    {
        if (isConnected)
            return;

        try
        {
            Log.Information($"Connecting to radar control port at ({ipAddress}:{IRadarConnection.IP_RADAR_CONTROL_PORT})...");
            controlStream = new TcpClient(ipAddress, IRadarConnection.IP_RADAR_CONTROL_PORT);
            
            // Setting timeout to 20 seconds since FW update has long commands that takes time
            controlStream.ReceiveTimeout = 20000;
        }
        catch (Exception ex)
        {
            Log.Error($"Connection to radar control port at {ipAddress} failed", ex);
            throw;
        }
        
        try
        {
            Log.Information($"Connecting to radar data port at ({ipAddress}:{IRadarConnection.IP_RADAR_DATA_PORT})...");
            dataStream = new TcpClient(ipAddress, IRadarConnection.IP_RADAR_DATA_PORT);
            
            // setting to 2 sec so that we can operate even at a slow frame rate of 1 fps
            dataStream.ReceiveTimeout = 2000;
        }
        catch (Exception ex)
        {
            Log.Error($"Connection to radar data port at {ipAddress} failed", ex);
            controlStream.Close();
            throw;
        }

        isConnected = true;
    }

    public void Disconnect()
    {
        if (isConnected)
        {
            if (controlStream != null)
            {
                // Log.Debug($"Disconnnect - Closing control stream...");
                controlStream.Close();
                controlStream = null;
            }
            
            if (dataStream != null)
            {
                // Log.Debug($"Disconnnect - Closing data stream...");
                dataStream.Close();
                dataStream = null;
            }
            
            isConnected = false;
        }
    }
} 