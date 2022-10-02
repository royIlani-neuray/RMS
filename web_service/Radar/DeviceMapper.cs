using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace WebService.Radar;

public class DeviceMapper 
{
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
        udpClient = new UdpClient(IPRadarClient.IP_RADAR_BROADCAST_PORT);
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
            System.Console.WriteLine($"address: {address}");

            IPEndPoint sourceEndpoint = new IPEndPoint(address, 0);
            IPEndPoint targetEndpoint = new IPEndPoint(IPAddress.Broadcast, IPRadarClient.IP_RADAR_BROADCAST_PORT);

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

            IPAddress ip = new IPAddress(reader.ReadBytes(4));
            IPAddress subnet = new IPAddress(reader.ReadBytes(4));
            IPAddress gateway = new IPAddress(reader.ReadBytes(4));
            bool staticIP = reader.ReadBoolean();

            Console.WriteLine();
            Console.WriteLine($"Got a broadcast message from: {endpoint}");
            Console.WriteLine($"Guid: {guid}");
            Console.WriteLine($"ip: {ip}");
            Console.WriteLine($"subnet: {subnet}");
            Console.WriteLine($"gateway: {gateway}");
            Console.WriteLine($"staticIP: {staticIP}");
            Console.WriteLine();
            
            // *** TEST
            /*
            IPRadarClient client = new IPRadarClient(ip.ToString());
            Console.WriteLine();
            Console.WriteLine($"Connecting...");
            client.Connect();
            string command = "sensorStop";
            System.Console.WriteLine($"Sending command: {command}");
            var resp = client.SendTICommand(command);
            System.Console.WriteLine($"Response: {resp}");
            Console.WriteLine();
            */
        }
    }

}