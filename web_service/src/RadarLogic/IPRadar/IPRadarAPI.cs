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
using Microsoft.AspNetCore.Mvc;
using Serilog;
using WebService.RadarLogic.IPRadar.Connection;

namespace WebService.RadarLogic.IPRadar;

public class IPRadarAPI 
{
    #region Radar protocol definitions


    public const uint MESSAGE_HEADER_MAGIC = 0xE1AD1984;
    public const int MESSAGE_HEADER_SIZE = 6;
    public const byte PROTOCOL_REVISION = 1;

    public const byte DEVICE_INFO_KEY = 100;
    public const byte TI_COMMAND_RESPONSE_KEY = 101;
    public const byte FW_UPDATE_RESPONSE_KEY = 102;
    public const byte SET_DEVICE_ID_RESPONSE_KEY = 103;
    public const byte GET_IMU_DATA_RESPONSE_KEY = 104;
    public const byte SET_RMS_HOSTNAME_RESPONSE_KEY = 105;
    public const byte CALIBRATION_DATA_RESPONSE_KEY = 106;
    public const byte DISCOVER_DEVICE_KEY = 200;
    public const byte CONFIGURE_NETWORK_KEY = 201;
    public const byte TI_COMMAND_KEY = 202;
    public const byte RESET_RADAR_KEY = 203;
    public const byte SET_DEVICE_ID_KEY = 204;
    public const byte FW_UPDATE_INIT_KEY = 205;
    public const byte FW_UPDATE_WRITE_CHUNK_KEY = 206;
    public const byte FW_UPDATE_APPLY_KEY = 207;
    public const byte GET_IMU_DATA_KEY = 208;
    public const byte SET_RMS_HOSTNAME_KEY = 209;
    public const byte GET_CALIBRATION_DATA_KEY = 210;

    public const int FW_UPDATE_CHUNK_SIZE = 512;
    public const int RMS_HOSTNAME_MAX_LENGTH = 128;

    public const int DEVICE_ID_SIZE_BYTES = 16; // GUID
    public const int MAX_TI_COMMAND_SIZE = 256;
    public const int MAX_TI_RESPONSE_SIZE = 256;
    public const int MAX_CALIBRATION_DATA_SIZE = 24;

    public const int IPV4_ADDRESS_SIZE = 4;
    public const int MODEL_STRING_MAX_LENGTH = 15;
    public const int APP_STRING_MAX_LENGTH = 30;
    
    #endregion

    private bool remoteConnection = false;

    private IRadarConnection? connection;

    ~IPRadarAPI()
    {
        Disconnect();
    }

    public void ConnectLocalRadar(string localIPAddress)
    {
        remoteConnection = false;
        connection = new IPRadarClient(localIPAddress);
        connection.Connect();
    }

    public void InitRemoteRadarConnection(string radarId)
    {
        remoteConnection = true;
        connection = new IPRadarServer(radarId);
        connection.Connect();
    }

    public void GetRemoteRadarConnectionPorts(out int controlPort, out int dataPort)
    {
        if (!remoteConnection || connection == null)
            throw new BadRequestException("Radar is not set for remote connection.");
        
        IPRadarServer server = (IPRadarServer) connection;
        controlPort = server.ControlPort;
        dataPort = server.DataPort;
    }

    public void Disconnect()
    {
        connection?.Disconnect();
    }

    public bool IsConnected()
    {
        return (connection != null) && connection.IsRadarConnected;
    }

    public void ResetRadar()
    {
        if (!IsConnected())
            throw new Exception("ResetRadar failed - radar not connected.");
        
        Log.Information("Sending Reset command...");

        // create the reset command packet
        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(IPRadarAPI.MESSAGE_HEADER_MAGIC);
        writer.Write(IPRadarAPI.PROTOCOL_REVISION);
        writer.Write(IPRadarAPI.RESET_RADAR_KEY);
        var packet = new byte[MESSAGE_HEADER_SIZE];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(packet, 0, packet.Length);

        // send the command
        connection!.ControlStream!.GetStream().Write(packet, 0, packet.Length);

        Disconnect();
    }

    public string SendTICommand(string command)
    {
        if (!IsConnected())
            throw new Exception("SendTICommand failed - radar not connected.");
        
        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(IPRadarAPI.MESSAGE_HEADER_MAGIC);
        writer.Write(IPRadarAPI.PROTOCOL_REVISION);
        writer.Write(IPRadarAPI.TI_COMMAND_KEY);

        var commandArray = command.ToCharArray();

        if (commandArray.Length > MAX_TI_COMMAND_SIZE)
            throw new Exception($"Error: Radar command exceed max length of {MAX_TI_COMMAND_SIZE} bytes");
        
        writer.Write(commandArray);
        
        int padLength = MAX_TI_COMMAND_SIZE - commandArray.Length;

        if (padLength > 0)
        {
            var padArray = new byte[padLength];
            padArray[0] = 0x0;
            writer.Write(padArray);
        }
        
        var reader = SendAndRecieveMessage(stream, responseSize: MESSAGE_HEADER_SIZE + MAX_TI_RESPONSE_SIZE, TI_COMMAND_RESPONSE_KEY);

        var responseChars = reader.ReadChars(MAX_TI_RESPONSE_SIZE);
        string responseString = new string(responseChars).Replace("\x00","");

        return responseString;
    }

    public int ReadTIData(byte[] dataArray, int size)
    {
        if (!IsConnected())
            throw new Exception("ReadTIData failed - radar not connected.");

        // Log.Debug($"Trying to Read from data stream... size: {size}"); 

        int bytesRead = 0;
        var stream = connection!.DataStream!.GetStream();

        try
        {
            while (bytesRead < size)
            {
                bytesRead += stream.Read(dataArray, bytesRead, size - bytesRead);
                // Log.Debug($"Read {bytesRead} out of {size}");
            }
        }
        catch (Exception ex)
        {
            Log.Debug($"ReadTIData failed. message: '{ex.Message}', size to read: {size}, bytes read: {bytesRead}", ex); 
            throw;
        }


        return bytesRead;
    }

    public void GetIMUData()
    {
        if (!IsConnected())
            throw new Exception("GetIMUData failed - radar not connected.");

        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(IPRadarAPI.MESSAGE_HEADER_MAGIC);
        writer.Write(IPRadarAPI.PROTOCOL_REVISION);
        writer.Write(IPRadarAPI.GET_IMU_DATA_KEY);

        var reader = SendAndRecieveMessage(stream, responseSize: MESSAGE_HEADER_SIZE + 10, GET_IMU_DATA_RESPONSE_KEY);

        bool isSupported = reader.ReadBoolean();
        bool isActive = reader.ReadBoolean();
        float pitchDegrees = reader.ReadSingle();
        float rollDegrees = reader.ReadSingle();

        Log.Information($"IMU supported? {isSupported}");
        if (isSupported) {
            Log.Information($"IMU active? {isActive}");
            if (isActive) {
                Log.Information($"IMU data: pitch_deg {pitchDegrees}");
                Log.Information($"IMU data: roll_deg {rollDegrees}");
            }
        }
    }

    public string GetCalibrationData()
    {
        if (!IsConnected())
            throw new Exception("GetCalibrationData failed - radar not connected.");

        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(IPRadarAPI.MESSAGE_HEADER_MAGIC);
        writer.Write(IPRadarAPI.PROTOCOL_REVISION);
        writer.Write(IPRadarAPI.GET_CALIBRATION_DATA_KEY);

        var reader = SendAndRecieveMessage(stream, responseSize: MESSAGE_HEADER_SIZE + 102, CALIBRATION_DATA_RESPONSE_KEY);

        int status = reader.ReadByte();
        int numElements = reader.ReadByte();
        float rangeBias = reader.ReadSingle();

        float [] rxChPhaseComp = new float[MAX_CALIBRATION_DATA_SIZE];

        string calibrationString = $"{rangeBias:0.0000}";

        for (int i=0; i<MAX_CALIBRATION_DATA_SIZE; i++)
        {
            rxChPhaseComp[i] = reader.ReadSingle();

            if (i < numElements)
            {
                calibrationString += $" {rxChPhaseComp[i]:0.0000}";
            }
        }

        Log.Debug("Calibration Data:");
        Log.Debug($"Calibration status: {status}");
        Log.Debug($"Calibration num elements: {numElements}");
        Log.Debug($"Calibration rangeBias: {rangeBias}");
        Log.Debug($"Calibration string: {calibrationString}");

        return calibrationString;
    }

    public void SetRMSHostname(string hostname)
    {
        if (!IsConnected())
            throw new Exception("SetRMSHostname failed - radar not connected.");
        
        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(IPRadarAPI.MESSAGE_HEADER_MAGIC);
        writer.Write(IPRadarAPI.PROTOCOL_REVISION);
        writer.Write(IPRadarAPI.SET_RMS_HOSTNAME_KEY);

        var hostnameArray = hostname.ToCharArray();

        if (hostnameArray.Length > RMS_HOSTNAME_MAX_LENGTH)
            throw new Exception($"Error: RMS hostname exceed max length of {RMS_HOSTNAME_MAX_LENGTH} bytes");
        
        writer.Write(hostnameArray);
        
        int padLength = RMS_HOSTNAME_MAX_LENGTH - hostnameArray.Length;

        if (padLength > 0)
        {
            var padArray = new byte[padLength];
            padArray[0] = 0x0;
            writer.Write(padArray);
        }
        
        var reader = SendAndRecieveMessage(stream, responseSize: MESSAGE_HEADER_SIZE, SET_RMS_HOSTNAME_RESPONSE_KEY);
    }

    public static List<IPAddress> GetBroadcastAddresses()
    {
        List<IPAddress> broadcastSources = new List<IPAddress>();

        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if(nic.OperationalStatus == OperationalStatus.Up && nic.SupportsMulticast && nic.GetIPProperties().GetIPv4Properties() != null) 
            {
                int index = nic.GetIPProperties().GetIPv4Properties().Index;

                if (index != NetworkInterface.LoopbackInterfaceIndex) 
                {
                    foreach(UnicastIPAddressInformation uip in nic.GetIPProperties().UnicastAddresses) 
                    {
                        if(uip.Address.AddressFamily == AddressFamily.InterNetwork) 
                        {
                            broadcastSources.Add(uip.Address);
                        }
                    }
                }
            }

        }

        return broadcastSources;
    }

    public static void SetDeviceNetwork(string deviceId, string ipAddress, string subnetMask, string gwAddress, bool staticIP)
    {
        List<IPAddress> broadcastSources = GetBroadcastAddresses();

        if (!staticIP)
        {
            ipAddress = "0.0.0.0";
            subnetMask = "255.255.255.0";
            gwAddress = "0.0.0.0";
        }

        Log.Information("");
        Log.Information("Setting Device Network:");
        Log.Information($"** Device: {deviceId}");
        Log.Information($"** IP Address: {deviceId}");
        Log.Information($"** Subnet Mask: {subnetMask}");
        Log.Information($"** Gateway Address: {gwAddress}");
        Log.Information($"** Static IP: {staticIP}");
        Log.Information("");

        // create the broadcast packet
        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(IPRadarAPI.MESSAGE_HEADER_MAGIC);
        writer.Write(IPRadarAPI.PROTOCOL_REVISION);
        writer.Write(IPRadarAPI.CONFIGURE_NETWORK_KEY);

        Guid guid = new Guid(deviceId);
        writer.Write(guid.ToByteArray());
        writer.Write(IPAddress.Parse(ipAddress).GetAddressBytes());
        writer.Write(IPAddress.Parse(subnetMask).GetAddressBytes());
        writer.Write(IPAddress.Parse(gwAddress).GetAddressBytes());
        writer.Write(staticIP);

        var packet = new byte[IPRadarAPI.MESSAGE_HEADER_SIZE + DEVICE_ID_SIZE_BYTES + (IPV4_ADDRESS_SIZE * 3) + 1];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(packet, 0, packet.Length);

        // broadcast for each address
        foreach (var address in broadcastSources)
        {
            IPEndPoint sourceEndpoint = new IPEndPoint(address, 0);
            IPEndPoint targetEndpoint = new IPEndPoint(IPAddress.Broadcast, IRadarConnection.IP_RADAR_BROADCAST_PORT_DEVICE);

            UdpClient sendClient = new UdpClient(sourceEndpoint);
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
            sendClient.Send(packet, targetEndpoint);
        }
    }

    public static void SendResetBroadcast(string deviceId)
    {
        List<IPAddress> broadcastSources = GetBroadcastAddresses();

        Log.Information("");
        Log.Information($"** Sending Device-Reset broadcast to: {deviceId}");
        Log.Information("");

        // create the broadcast packet
        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(IPRadarAPI.MESSAGE_HEADER_MAGIC);
        writer.Write(IPRadarAPI.PROTOCOL_REVISION);
        writer.Write(IPRadarAPI.RESET_RADAR_KEY);

        Guid guid = new Guid(deviceId);
        writer.Write(guid.ToByteArray());

        var packet = new byte[IPRadarAPI.MESSAGE_HEADER_SIZE + DEVICE_ID_SIZE_BYTES];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(packet, 0, packet.Length);

        // broadcast for each address
        foreach (var address in broadcastSources)
        {
            IPEndPoint sourceEndpoint = new IPEndPoint(address, 0);
            IPEndPoint targetEndpoint = new IPEndPoint(IPAddress.Broadcast, IRadarConnection.IP_RADAR_BROADCAST_PORT_DEVICE);

            UdpClient sendClient = new UdpClient(sourceEndpoint);
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
            sendClient.Send(packet, targetEndpoint);
        }
    }


    public void SetDeviceId(string deviceId, string newDeviceId)
    {
        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(MESSAGE_HEADER_MAGIC);
        writer.Write(PROTOCOL_REVISION);
        writer.Write(SET_DEVICE_ID_KEY);

        Guid guid = new Guid(newDeviceId);
        writer.Write(guid.ToByteArray());

        var reader = SendAndRecieveMessage(stream, responseSize: MESSAGE_HEADER_SIZE, SET_DEVICE_ID_RESPONSE_KEY);
    }

    private BinaryReader SendAndRecieveMessage(MemoryStream requestStream, int responseSize, byte expectedResponseType)
    {
        var request = new byte[requestStream.Length];
        
        requestStream.Seek(0, SeekOrigin.Begin);
        requestStream.Read(request, 0, request.Length);

        // send the command
        connection!.ControlStream!.GetStream().Write(request, 0, request.Length);

        
        // read the response
        var responseBytes = new byte[responseSize];
        int count = connection.ControlStream.GetStream().Read(responseBytes, 0, responseBytes.Length);

        if (count != responseSize)
            throw new Exception("Error: Invalid response size for command");
        
        var reader = new BinaryReader(new MemoryStream(responseBytes));

        ReadResponseMessageHeader(reader, expectedResponseType);

        return reader;
    }

    private void ReadResponseMessageHeader(BinaryReader reader, byte expectedMessageType)
    {
        var magic = reader.ReadUInt32();
        var protocol = reader.ReadByte();
        var messageType = reader.ReadByte();

        if (magic != IPRadarAPI.MESSAGE_HEADER_MAGIC)
        {
            throw new Exception("Error: invalid magic in response header");
        }

        if (protocol != IPRadarAPI.PROTOCOL_REVISION)
        {
            throw new Exception("Invalid protocol in response header");
        }

        if (messageType != expectedMessageType)
        {
            throw new Exception("Unexpected message type in response header");
        }
    }

    private void UploadFirmwareImage(byte [] image)
    {
        var totalChunks = image.Length / FW_UPDATE_CHUNK_SIZE;

        if (image.Length % FW_UPDATE_CHUNK_SIZE > 0)
            totalChunks++;

        ushort chunkNumber = 0;
        
        int pos = 0;

        while (pos < image.Length)
        {
            var stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(MESSAGE_HEADER_MAGIC);
            writer.Write(PROTOCOL_REVISION);
            writer.Write(FW_UPDATE_WRITE_CHUNK_KEY);
            writer.Write(chunkNumber);

            if (image.Length - pos >= FW_UPDATE_CHUNK_SIZE)
            {
                writer.Write(image, pos, FW_UPDATE_CHUNK_SIZE);
                pos += FW_UPDATE_CHUNK_SIZE;
            }
            else
            {
                var dataSize = image.Length - pos;
                writer.Write(image, pos, dataSize);
                var padArray = new byte[FW_UPDATE_CHUNK_SIZE - dataSize];
                writer.Write(padArray, 0, padArray.Length);
                pos += dataSize;
            }

            Log.Information($"uploading chunk: {chunkNumber + 1} / {totalChunks}");
            
            var reader = SendAndRecieveMessage(stream, responseSize: MESSAGE_HEADER_SIZE + 1, FW_UPDATE_RESPONSE_KEY);

            byte status = reader.ReadByte();

            if (status != 0)
            {
                throw new Exception("Unexpected FW error when uploading firmware image chunk");
            }
            
            chunkNumber++;
        }
    }

    private void ApplyFirmwareUpdate()
    {
        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(MESSAGE_HEADER_MAGIC);
        writer.Write(PROTOCOL_REVISION);
        writer.Write(FW_UPDATE_APPLY_KEY);

        var reader = SendAndRecieveMessage(stream, responseSize: MESSAGE_HEADER_SIZE + 1, FW_UPDATE_RESPONSE_KEY);

        byte status = reader.ReadByte();

        if (status != 0)
        {
            throw new Exception("Unexpected FW error when applying firmware update.");
        }
    }

    private void InitFirmwareUpdate(byte [] image)
    {
        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(MESSAGE_HEADER_MAGIC);
        writer.Write(PROTOCOL_REVISION);
        writer.Write(FW_UPDATE_INIT_KEY);

        uint imageSize = (uint) image.Length;
        writer.Write(imageSize);

        ushort totalChunks = (ushort) (image.Length / FW_UPDATE_CHUNK_SIZE);
        if (image.Length % FW_UPDATE_CHUNK_SIZE > 0)
            totalChunks++;

        writer.Write(totalChunks);


        var reader = SendAndRecieveMessage(stream, responseSize: MESSAGE_HEADER_SIZE + 1, FW_UPDATE_RESPONSE_KEY);

        byte status = reader.ReadByte();

        if (status != 0)
        {
            throw new Exception("Unexpected FW error during firmware update init.");
        }
    }

    private void ValidateImage(byte [] image)
    {
        string header = $"{(char)image[0]}{(char)image[1]}{(char)image[2]}{(char)image[3]}";

        if (header != "MSTR")
        {
            throw new Exception("Invalid FW image provided. could not find MSTR header.");
        }
    }

    public void UpdateFirmware(byte [] image)
    {
        if (!IsConnected())
            throw new Exception("UpdateFirmware failed - radar not connected.");

        ValidateImage(image);
        
        Log.Information($"Initializing FW update process...");
        InitFirmwareUpdate(image);

        Log.Information($"Uploading Firmware image. image size: {image.Length}");

        UploadFirmwareImage(image);

        Log.Information($"FW image uploaded, applying update...");

        ApplyFirmwareUpdate();

        Log.Information($"FW update process is done!");
        
    }

}