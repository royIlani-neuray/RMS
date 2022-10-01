using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace WebService.Service;

public class DeviceMapper 
{
    public const int IP_RADAR_BROADCAST_PORT = 7003;

    // TODO: should move to IP-RADAR-Client class...
    public const int MESSAGE_HEADER_SIZE = 6;
    public const uint MESSAGE_HEADER_MAGIC = 0xE1AD1984;
    public const byte PROTOCOL_REVISION = 1;
    public const byte DEVICE_INFO_KEY = 100;
    public const byte DISCOVER_DEVICE_KEY = 200;
    public const int DEVICE_ID_SIZE_BYTES = 16; // GUID


    private Task? broadcastListenerTask;
    private UdpClient? udpClient;

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile DeviceMapper? instance; 

    public static DeviceMapper Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new DeviceMapper();
                }
            }

            return instance;
        }
    }

    private DeviceMapper() {}

    #endregion

    public void Start()
    {
        udpClient = new UdpClient(IP_RADAR_BROADCAST_PORT);
        udpClient.EnableBroadcast = true;

        broadcastListenerTask = new Task(() => 
        {
           BrodcastListnerTask();
        });

        broadcastListenerTask.Start();

        MapDevices();
    }

    public void MapDevices()
    {
        List<IPAddress> broadcastSources = new List<IPAddress>();

        // get addresses
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

        // create the broadcast packet

        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(MESSAGE_HEADER_MAGIC);
        writer.Write(PROTOCOL_REVISION);
        writer.Write(DISCOVER_DEVICE_KEY);
        var packet = new byte[6];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(packet, 0, packet.Length);

        // send for each address
        foreach (var address in broadcastSources)
        {
            System.Console.WriteLine($"address: {address}");

            IPEndPoint sourceEndpoint = new IPEndPoint(address, 0);
            IPEndPoint targetEndpoint = new IPEndPoint(IPAddress.Broadcast, IP_RADAR_BROADCAST_PORT);

            UdpClient sendClient = new UdpClient(sourceEndpoint);
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
            sendClient.Send(packet, targetEndpoint);
        }
    }

    private void BrodcastListnerTask()
    {
        IPEndPoint? endpoint = null;
        
        while (true)
        {
            var packet = udpClient!.Receive(ref endpoint); 

            if (packet.Length < MESSAGE_HEADER_SIZE)
            {
                Console.WriteLine("invalid broadcast message size. ignoring.");
                continue;
            }

            var reader = new BinaryReader(new MemoryStream(packet));
            var magic = reader.ReadUInt32();
            var protocol = reader.ReadByte();
            var messageType = reader.ReadByte();
            
            if (magic != MESSAGE_HEADER_MAGIC)
            {
                Console.WriteLine("Error: invalid magic in broadcast message. ignoring.");
                continue;
            }

            if (protocol != PROTOCOL_REVISION)
            {
                Console.WriteLine("Error: invalid protocol revision in broadcast message. ignoring.");
                continue;
            }

            if (messageType == DISCOVER_DEVICE_KEY)
            {
                // got the broadcast that we sent. ignore it.
                continue;
            }

            if (messageType != DEVICE_INFO_KEY)
            {
                Console.WriteLine("Unknown broadcast message type. ignoring.");
                continue;
            }

            var guidBytes = reader.ReadBytes(DEVICE_ID_SIZE_BYTES);           
            Guid guid = new Guid(guidBytes);

            IPAddress ip = new IPAddress(reader.ReadBytes(4));
            IPAddress subnet = new IPAddress(reader.ReadBytes(4));
            IPAddress gateway = new IPAddress(reader.ReadBytes(4));
            bool staticIP = reader.ReadBoolean();

            Console.WriteLine();
            Console.WriteLine($"Got a broadcast message from: {endpoint}");
            Console.WriteLine($"ip: {ip}");
            Console.WriteLine($"subnet: {subnet}");
            Console.WriteLine($"gateway: {gateway}");
            Console.WriteLine($"staticIP: {staticIP}");
            Console.WriteLine();
            

        }
    }

}