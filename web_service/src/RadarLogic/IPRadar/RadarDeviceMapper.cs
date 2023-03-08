/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Net;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using WebService.Context;
using WebService.Events;

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
        udpClient = new UdpClient(IPRadarClient.IP_RADAR_BROADCAST_PORT_SERVER);
        udpClient.EnableBroadcast = true;

        broadcastListenerTask = new Task(() => 
        {
           BrodcastListnerTask();
        });

        broadcastListenerTask.Start();
    }

    public void MapDevices()
    {
        System.Console.WriteLine($"Sending device mapping broadcast...");

        mappedDevices.Clear();
        
        RMSEvents.Instance.DeviceMappingUpdatedEvent();
        
        List<IPAddress> broadcastSources = IPRadarClient.GetBroadcastAddresses();

        // create the broadcast packet

        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(IPRadarClient.MESSAGE_HEADER_MAGIC);
        writer.Write(IPRadarClient.PROTOCOL_REVISION);
        writer.Write(IPRadarClient.DISCOVER_DEVICE_KEY);
        var packet = new byte[IPRadarClient.MESSAGE_HEADER_SIZE];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(packet, 0, packet.Length);

        // send for each address
        foreach (var address in broadcastSources)
        {
            // System.Console.WriteLine($"address: {address}");

            IPEndPoint sourceEndpoint = new IPEndPoint(address, 0);
            IPEndPoint targetEndpoint = new IPEndPoint(IPAddress.Broadcast, IPRadarClient.IP_RADAR_BROADCAST_PORT_DEVICE);

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

    private void BrodcastListnerTask()
    {
        IPEndPoint? endpoint = null;
        
        while (true)
        {
            var packet = udpClient!.Receive(ref endpoint); 

            if (packet.Length < IPRadarClient.MESSAGE_HEADER_SIZE)
            {
                Console.WriteLine("invalid broadcast message size. ignoring.");
                continue;
            }

            var reader = new BinaryReader(new MemoryStream(packet));
            var magic = reader.ReadUInt32();
            var protocol = reader.ReadByte();
            var messageType = reader.ReadByte();
            
            if (magic != IPRadarClient.MESSAGE_HEADER_MAGIC)
            {
                Console.WriteLine("Error: invalid magic in broadcast message. ignoring.");
                continue;
            }

            if (protocol != IPRadarClient.PROTOCOL_REVISION)
            {
                Console.WriteLine("Error: invalid protocol revision in broadcast message. ignoring.");
                continue;
            }

            if (messageType == IPRadarClient.DISCOVER_DEVICE_KEY)
            {
                // got the broadcast that we sent. ignore it.
                continue;
            }

            if (messageType != IPRadarClient.DEVICE_INFO_KEY)
            {
                Console.WriteLine("Unknown broadcast message type. ignoring.");
                continue;
            }

            var guidBytes = reader.ReadBytes(IPRadarClient.DEVICE_ID_SIZE_BYTES);           
            Guid guid = new Guid(guidBytes);

            IPAddress ip = new IPAddress(reader.ReadBytes(IPRadarClient.IPV4_ADDRESS_SIZE));
            IPAddress subnet = new IPAddress(reader.ReadBytes(IPRadarClient.IPV4_ADDRESS_SIZE));
            IPAddress gateway = new IPAddress(reader.ReadBytes(IPRadarClient.IPV4_ADDRESS_SIZE));
            bool staticIP = reader.ReadBoolean();
            string model = new string(reader.ReadChars(IPRadarClient.MODEL_STRING_MAX_LENGTH)).Replace("\x00","");
            string appName = new string(reader.ReadChars(IPRadarClient.APP_STRING_MAX_LENGTH)).Replace("\x00","");
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
                fwVersion = ParseFwVersion(fwVersion)
            };
            
            UpdateRegisteredStatus(deviceInfo);

            Console.WriteLine();
            Console.WriteLine($"Got a broadcast message from: {endpoint}");
            Console.WriteLine($"Guid: {guid}");
            Console.WriteLine($"ip: {ip}");
            Console.WriteLine($"subnet: {subnet}");
            Console.WriteLine($"gateway: {gateway}");
            Console.WriteLine($"staticIP: {staticIP}");
            Console.WriteLine($"model: {model}");
            Console.WriteLine($"appName: {appName}");
            Console.WriteLine($"FW version: {deviceInfo.fwVersion}");
            Console.WriteLine();
            
            AddOrUpdateMappedDevice(deviceInfo);

            RMSEvents.Instance.DeviceMappingUpdatedEvent();
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