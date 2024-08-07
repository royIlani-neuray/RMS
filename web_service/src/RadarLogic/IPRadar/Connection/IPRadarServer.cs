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
using WebService.Actions.Radars;
using WebService.RadarLogic.IPRadar.Connection;

namespace WebService.RadarLogic.IPRadar;

public class IPRadarServer : IRadarConnection
{
    private TcpListener? tcpListenerControl;
    private TcpListener? tcpListenerData;

    private TcpClient? controlStream;
    private TcpClient? dataStream;
    private bool isConnected = false;
    private readonly string radarId;
    private int controlPortNumber = 0;
    private int dataPortNumber = 0;

    public bool IsRadarConnected => isConnected;
    public int ControlPort => controlPortNumber;
    public int DataPort => dataPortNumber;
    public TcpClient? ControlStream => controlStream;
    public TcpClient? DataStream => dataStream;


    public IPRadarServer(string radarId)
    {
        this.radarId = radarId;
        WaitForConnectionTask();
    }

    private bool initTcpListener(out TcpListener? tcpListener, out int portNumber)
    {
        portNumber = 0;

        try
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            tcpListener = new TcpListener(endpoint);
            tcpListener.Start();
            endpoint = (IPEndPoint) tcpListener.LocalEndpoint;
            portNumber = endpoint.Port;
        }
        catch
        {
            tcpListener = null;
            return false;
        }

        return true;
    }

    private void WaitForConnectionTask()
    {
        Task.Run(async () => 
        {
            try
            {
                if (initTcpListener(out tcpListenerControl, out controlPortNumber) && initTcpListener(out tcpListenerData, out dataPortNumber))
                {
                    System.Console.WriteLine($"Debug: created TCP sockets - control port: {controlPortNumber}, data: {dataPortNumber}");
                    
                    controlStream = await tcpListenerControl!.AcceptTcpClientAsync();                    
                    // Setting timeout to 20 seconds since FW update has long commands that takes time
                    controlStream.ReceiveTimeout = 20000;
                    
                    dataStream = await tcpListenerData!.AcceptTcpClientAsync();
                    
                    // setting datastream to 2 sec so that we can operate even at a slow frame rate of 1 fps
                    dataStream.ReceiveTimeout = 2000;
                    
                    System.Console.WriteLine("Debug: Remote Radar connected.");

                    isConnected = true;

                    var action = new RemoteRadarConnectedAction(radarId);
                    action.Run();
                }

            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: WaitForConnectionTask - failed. Exception: {ex}");
            }
            finally
            {
                if (tcpListenerControl != null)
                {
                    tcpListenerControl.Dispose();
                    tcpListenerControl = null;
                }

                if (tcpListenerData != null)
                {
                    tcpListenerData.Dispose();
                    tcpListenerControl = null;
                }
            }
        });
    }

    public void Connect() {} // do nothing. connections are initiated by the remote radar

    public void Disconnect()
    {
        if (isConnected)
        {
            if (controlStream != null)
            {
                // Console.WriteLine($"Debug: Disconnnect - Closing control stream...");
                controlStream.Close();
                controlStream = null;
            }
            
            if (dataStream != null)
            {
                // Console.WriteLine($"Debug: Disconnnect - Closing data stream...");
                dataStream.Close();
                dataStream = null;
            }
            
            isConnected = false;
        }
    }
} 