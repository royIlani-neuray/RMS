/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Net;
using System.Net.Sockets;
using DeviceEmulator.Recordings;

namespace DeviceEmulator;

public class EmulatorDevice {

    public const int IP_RADAR_CONTROL_PORT = 7001;
    public const int IP_RADAR_DATA_PORT = 7002;
    public const int IP_RADAR_BROADCAST_PORT_SERVER = 7003;
    public const int IP_RADAR_BROADCAST_PORT_DEVICE = 7004;

    public const uint MESSAGE_HEADER_MAGIC = 0xE1AD1984;
    public const int MESSAGE_HEADER_SIZE = 6;
    public const byte PROTOCOL_REVISION = 1;

    public const byte DEVICE_INFO_KEY = 100;
    public const byte TI_COMMAND_RESPONSE_KEY = 101;
    public const byte TI_COMMAND_KEY = 202;
    public const int DEVICE_ID_SIZE_BYTES = 16; // GUID
    public const int MODEL_STRING_MAX_LENGTH = 15;
    public const int APP_STRING_MAX_LENGTH = 30;
    public const int MAX_TI_COMMAND_SIZE = 256;
    public const int MAX_TI_RESPONSE_SIZE = 256;



    private TcpListener controlServer;
    private TcpListener dataServer;

    public EmulatorDevice()
    {
        controlServer = new TcpListener(IPAddress.Any, IP_RADAR_CONTROL_PORT);
        dataServer = new TcpListener(IPAddress.Any, IP_RADAR_DATA_PORT);

        controlServer.Start();
        dataServer.Start();
    }

    public void HandleTICommand(NetworkStream controlStream, BinaryReader reader)
    {
        var commandBytes = reader.ReadBytes(MAX_TI_COMMAND_SIZE);
        string commandString = System.Text.Encoding.ASCII.GetString(commandBytes).Replace("\x00","");;
        System.Console.WriteLine($"Got TI Command: {commandString}");

        MemoryStream response = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(response);
        writer.Write(MESSAGE_HEADER_MAGIC);
        writer.Write(PROTOCOL_REVISION);
        writer.Write(TI_COMMAND_RESPONSE_KEY);
        
        var responseBytes = System.Text.Encoding.ASCII.GetBytes("Done");
        responseBytes = responseBytes.Concat(new byte[EmulatorDevice.MAX_TI_RESPONSE_SIZE - responseBytes.Length]).ToArray();
        writer.Write(responseBytes);
        controlStream.Write(response.ToArray());

        if (commandString == "sensorStart")
        {
            RecordingStreamer.Instance.StartStreaming();
        }
    }

    public void HandleCommand(NetworkStream controlStream)
    {
        BinaryReader reader = new BinaryReader(controlStream);
        
        var magic = reader.ReadUInt32();
        var protocol = reader.ReadByte();
        var messageType = reader.ReadByte();

        if (magic != MESSAGE_HEADER_MAGIC)
        {
            throw new Exception($"Error: invalid magic in header!");
        }

        if (protocol != PROTOCOL_REVISION)
        {
            throw new Exception("Invalid protocol in header");
        }

        switch (messageType)
        {
            case TI_COMMAND_KEY:
                HandleTICommand(controlStream, reader);
                break;
            
            default:
                System.Console.WriteLine("Got unknown message type: {messageType}");
                break;
        }

    }

    public void Run()
    {
        System.Console.WriteLine("Starting emulator excution, waiting for RMS connection...");
        
        while (true)
        {
            TcpClient controlClient = controlServer.AcceptTcpClient();            
            NetworkStream controlStream = controlClient.GetStream();
            System.Console.WriteLine("RMS connected to control port.");

            TcpClient dataClient = dataServer.AcceptTcpClient();
            NetworkStream dataStream = dataClient.GetStream();
            RecordingStreamer.Instance.SetDataStream(dataStream);
            System.Console.WriteLine("RMS connected to data port.");

            while (controlClient.Connected)
            {
                try
                {
                    HandleCommand(controlStream);
                }
                catch (System.IO.EndOfStreamException)
                {
                    break;
                }                
            }

            System.Console.WriteLine("RMS Disconnected!");

            RecordingStreamer.Instance.StopStreaming();

            controlClient.Close();
            dataClient.Close();
        }
    }
}