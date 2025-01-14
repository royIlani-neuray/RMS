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
using System.Text.Json.Serialization;
using Serilog;
using WebService.Context;
using WebService.Events;
using WebService.RadarLogic.IPRadar.Connection;

namespace WebService.RadarLogic.IPRadar;

public class RadarDeviceMapper 
{
    private Task? broadcastListenerTask;
    private UdpClient? udpClient;

    private Dictionary<string, MappedDevice> mappedDevices;

    Action<string>? deviceDiscoveredCallback;

    public class MappedDevice 
    {
        [JsonPropertyName("ip")]
        public string ipAddress { get; set; } = String.Empty;
        
        [JsonPropertyName("subnet")]
        public string subnetMask { get; set; } = String.Empty;
        
        [JsonPropertyName("gateway")]
        public string gwAddress { get; set; } = String.Empty;
        
        [JsonPropertyName("device_id")]
        public string deviceId { get; set; } = String.Empty;
        
        [JsonPropertyName("model")]
        public string model { get; set; } = String.Empty;
        
        [JsonPropertyName("application")]
        public string appName { get; set; } = String.Empty;

        [JsonPropertyName("fw_version")]
        public string fwVersion { get; set; } = String.Empty;

        [JsonPropertyName("static_ip")]
        public bool staticIP { get; set; } = false;

        [JsonPropertyName("registered")]
        public bool registered { get; set; } = false;
        
        [JsonPropertyName("remote_device")]
        public bool remoteDevice { get; set; } = false;     // a remote device is a device that is not in the local RMS network (i.e. behind NAT)     
    }

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile RadarDeviceMapper? instance; 

    public static RadarDeviceMapper Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new RadarDeviceMapper();
                }
            }

            return instance;
        }
    }

    private RadarDeviceMapper() 
    {
        mappedDevices = new Dictionary<string, MappedDevice>();
        deviceDiscoveredCallback = null;
    }

    #endregion

    public void Start()
    {
        udpClient = new UdpClient(IRadarConnection.IP_RADAR_BROADCAST_PORT_SERVER);
        udpClient.EnableBroadcast = true;

        broadcastListenerTask = new Task(() => 
        {
           BrodcastListnerTask();
        });

        broadcastListenerTask.Start();
    }

    public void MapDevices()
    {
        Log.Information($"Sending device mapping broadcast...");

        mappedDevices.Clear();
        
        RMSEvents.Instance.DeviceMappingUpdatedEvent();
        
        List<IPAddress> broadcastSources = IPRadarAPI.GetBroadcastAddresses();

        // create the broadcast packet

        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(IPRadarAPI.MESSAGE_HEADER_MAGIC);
        writer.Write(IPRadarAPI.PROTOCOL_REVISION);
        writer.Write(IPRadarAPI.DISCOVER_DEVICE_KEY);
        var packet = new byte[IPRadarAPI.MESSAGE_HEADER_SIZE];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(packet, 0, packet.Length);

        // send for each address
        foreach (var address in broadcastSources)
        {
            // Log.Debug($"address: {address}");

            IPEndPoint sourceEndpoint = new IPEndPoint(address, 0);
            IPEndPoint targetEndpoint = new IPEndPoint(IPAddress.Broadcast, IRadarConnection.IP_RADAR_BROADCAST_PORT_DEVICE);

            UdpClient sendClient = new UdpClient(sourceEndpoint);
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
            sendClient.Send(packet, targetEndpoint);
        }
    }

    private string ParseFwVersion(uint fwVersion)
    {
        uint major = fwVersion >> (14 + 7 + 7);     // 4 msb
        uint minor = fwVersion >> (14 + 7) & 0x7F;  // 7 bits
        uint hotfix = fwVersion >> (14) & 0x7F;     // 7 bits
        uint build = fwVersion & 0x3FFF;            // 14 lsb

        return $"{major}.{minor}.{hotfix}.{build}";
    }

    public bool AddMappedDevice(bool remoteDevice, MemoryStream deviceInfoStream)
    {
        var reader = new BinaryReader(deviceInfoStream);
        var magic = reader.ReadUInt32();
        var protocol = reader.ReadByte();
        var messageType = reader.ReadByte();
        
        if (deviceInfoStream.Length < IPRadarAPI.MESSAGE_HEADER_SIZE)
        {
            Log.Error("invalid mapped device info message size. ignoring.");
            return false;
        }

        if (magic != IPRadarAPI.MESSAGE_HEADER_MAGIC)
        {
            Log.Error("invalid magic in mapped device info message. ignoring.");
            return false;
        }

        if (protocol != IPRadarAPI.PROTOCOL_REVISION)
        {
            Log.Error("invalid protocol revision in mapped device info message. ignoring.");
            return false;
        }

        if (messageType == IPRadarAPI.DISCOVER_DEVICE_KEY)
        {
            // got the broadcast that we sent. ignore it.
            return false;
        }

        if (messageType != IPRadarAPI.DEVICE_INFO_KEY)
        {
            Log.Warning("Unknown message type in mapped device info. ignoring.");
            return false;
        }

        var guidBytes = reader.ReadBytes(IPRadarAPI.DEVICE_ID_SIZE_BYTES);           
        Guid guid = new Guid(guidBytes);

        IPAddress ip = new IPAddress(reader.ReadBytes(IPRadarAPI.IPV4_ADDRESS_SIZE));
        IPAddress subnet = new IPAddress(reader.ReadBytes(IPRadarAPI.IPV4_ADDRESS_SIZE));
        IPAddress gateway = new IPAddress(reader.ReadBytes(IPRadarAPI.IPV4_ADDRESS_SIZE));
        bool staticIP = reader.ReadBoolean();
        string model = new string(reader.ReadChars(IPRadarAPI.MODEL_STRING_MAX_LENGTH)).Replace("\x00","");
        string appName = new string(reader.ReadChars(IPRadarAPI.APP_STRING_MAX_LENGTH)).Replace("\x00","");
        uint fwVersion = reader.ReadUInt32();

        
        MappedDevice deviceInfo = new MappedDevice()
        {
            ipAddress = ip.ToString(),
            subnetMask = subnet.ToString(),
            gwAddress = gateway.ToString(),
            deviceId = guid.ToString(),
            staticIP = staticIP,
            model = model,
            appName = appName,
            fwVersion = ParseFwVersion(fwVersion),
            remoteDevice = remoteDevice
        };
        
        UpdateRegisteredStatus(deviceInfo);

        Log.Information("");
        Log.Information($"Device mapping info: [{DateTime.Now}]");
        Log.Information($"Guid: {guid}");
        Log.Information($"Remote device: {remoteDevice}");
        Log.Information($"ip: {ip}");
        Log.Information($"subnet: {subnet}");
        Log.Information($"gateway: {gateway}");
        Log.Information($"staticIP: {staticIP}");
        Log.Information($"model: {model}");
        Log.Information($"appName: {appName}");
        Log.Information($"FW version: {deviceInfo.fwVersion}");
        Log.Information("");
        
        AddOrUpdateMappedDevice(deviceInfo);

        RMSEvents.Instance.DeviceMappingUpdatedEvent();   
        return true;     
    }

    private void BrodcastListnerTask()
    {
        IPEndPoint? endpoint = null;
        
        while (true)
        {
            var packet = udpClient!.Receive(ref endpoint); 

            Log.Information($"Got a broadcast message from: {endpoint} [{DateTime.Now}]");

            var deviceInfoStream = new MemoryStream(packet);
            AddMappedDevice(remoteDevice: false, deviceInfoStream);
        }
    }

    public void SetDeviceDiscoveredCallback(Action<string> callback)
    {
        deviceDiscoveredCallback = callback;
    }

    private void AddOrUpdateMappedDevice(MappedDevice mappedDevice)
    {
        if (mappedDevices.ContainsKey(mappedDevice.deviceId))
        {
            mappedDevices[mappedDevice.deviceId] = mappedDevice;
        }
        else
        {
            mappedDevices.Add(mappedDevice.deviceId, mappedDevice);
        }

        if (deviceDiscoveredCallback != null)
            deviceDiscoveredCallback(mappedDevice.deviceId);
    }

    private void UpdateRegisteredStatus(MappedDevice mappedDevice)
    {
        mappedDevice.registered = RadarContext.Instance.IsRadarExist(mappedDevice.deviceId);
    }
    
    public List<MappedDevice> GetMappedDevices()
    {
        var deviceList = mappedDevices.Values.ToList();
        
        // update registered status which may have changed by the service
        deviceList.ForEach(mappedDevice => UpdateRegisteredStatus(mappedDevice));

        return deviceList;
    }

    public MappedDevice GetMappedDevice(string deviceId)
    {
        MappedDevice mappedDevice = mappedDevices[deviceId];
        UpdateRegisteredStatus(mappedDevice);
        return mappedDevice;
    }

    public bool IsDeviceHasMapping(string deviceId)
    {
        return mappedDevices.ContainsKey(deviceId);
    }
}