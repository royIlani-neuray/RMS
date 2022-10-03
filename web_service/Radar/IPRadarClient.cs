
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace WebService.Radar;

public class IPRadarClient 
{
    #region Radar protocol definitions

    public const int IP_RADAR_CONTROL_PORT = 7001;
    public const int IP_RADAR_DATA_PORT = 7002;
    public const int IP_RADAR_BROADCAST_PORT = 7003;
    public const uint MESSAGE_HEADER_MAGIC = 0xE1AD1984;
    public const int MESSAGE_HEADER_SIZE = 6;
    public const byte PROTOCOL_REVISION = 1;
    public const byte DEVICE_INFO_KEY = 100;
    public const byte TI_COMMAND_RESPONSE_KEY = 101;
    public const byte DISCOVER_DEVICE_KEY = 200;
    public const byte CONFIGURE_NETWORK_KEY = 201;
    public const byte TI_COMMAND_KEY = 202;
    public const byte RESET_RADAR_KEY = 203;
    public const int DEVICE_ID_SIZE_BYTES = 16; // GUID
    public const int MAX_TI_COMMAND_SIZE = 256;
    public const int MAX_TI_RESPONSE_SIZE = 256;

    public const int IPV4_ADDRESS_SIZE = 4;
    public const int MODEL_STRING_MAX_LENGTH = 15;
    public const int APP_STRING_MAX_LENGTH = 30;
    
    #endregion

    private string ipAddress;
    private TcpClient? controlStream;
    private TcpClient? dataStream;
    private bool isConnected;

    public IPRadarClient(string ipAddress)
    {
        isConnected = false;
        this.ipAddress = ipAddress;
    }

    ~IPRadarClient()
    {
        Disconnect();
    }

    public void Connect()
    {
        if (isConnected)
            return;

        try
        {
            Console.WriteLine($"Connecting to radar control port at ({ipAddress}:{IP_RADAR_CONTROL_PORT})...");
            controlStream = new TcpClient(ipAddress, IP_RADAR_CONTROL_PORT);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Connection to radar control port at {ipAddress} failed: {ex.Message}");
            throw;
        }
        
        try
        {
            Console.WriteLine($"Connecting to radar data port at ({ipAddress}:{IP_RADAR_DATA_PORT})...");
            dataStream = new TcpClient(ipAddress, IP_RADAR_DATA_PORT);
            dataStream.ReceiveTimeout = 1000;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Connection to radar data port at {ipAddress} failed: {ex.Message}");
            controlStream.Close();
            throw;
        }

        isConnected = true;
    }

    public void Disconnect()
    {
        if (isConnected)
            controlStream!.Close();
            dataStream!.Close();
            isConnected = false;
    }

    public bool IsConnected()
    {
        return isConnected;
    }

    public void ResetRadar()
    {
        if (!isConnected)
            throw new Exception("ResetRadar failed - radar not connected.");
        
        System.Console.WriteLine("Sending Reset command...");

        // create the reset command packet
        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(IPRadarClient.MESSAGE_HEADER_MAGIC);
        writer.Write(IPRadarClient.PROTOCOL_REVISION);
        writer.Write(IPRadarClient.RESET_RADAR_KEY);
        var packet = new byte[MESSAGE_HEADER_SIZE];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(packet, 0, packet.Length);

        // send the command
        controlStream!.GetStream().Write(packet, 0, packet.Length);

        Disconnect();
    }

    public string SendTICommand(string command)
    {
        if (!isConnected)
            throw new Exception("SendTICommand failed - radar not connected.");
        
        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(IPRadarClient.MESSAGE_HEADER_MAGIC);
        writer.Write(IPRadarClient.PROTOCOL_REVISION);
        writer.Write(IPRadarClient.TI_COMMAND_KEY);

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
        
        var packet = new byte[MESSAGE_HEADER_SIZE + MAX_TI_COMMAND_SIZE];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(packet, 0, packet.Length);

        // send the command
        controlStream!.GetStream().Write(packet, 0, packet.Length);

        int responseSize = MESSAGE_HEADER_SIZE + MAX_TI_RESPONSE_SIZE;
        var responseBytes = new byte[responseSize];
        int count = controlStream.GetStream().Read(responseBytes, 0, responseBytes.Length);

        if (count != responseSize)
            throw new Exception("Error: Invalid response size for TI command");
        

        var reader = new BinaryReader(new MemoryStream(responseBytes));
        var magic = reader.ReadUInt32();
        var protocol = reader.ReadByte();
        var messageType = reader.ReadByte();
            
        if (magic != IPRadarClient.MESSAGE_HEADER_MAGIC)
        {
            throw new Exception("Error: invalid magic in response header");
        }

        if (protocol != IPRadarClient.PROTOCOL_REVISION)
        {
            throw new Exception("Invalid protocol in response header");
        }

        var responseChars = reader.ReadChars(MAX_TI_RESPONSE_SIZE);
        string responseString = new string(responseChars).Replace("\x00","");

        return responseString;
    }

    public int ReadTIData(byte[] dataArray, int size)
    {
        if (!isConnected)
            throw new Exception("ReadTIData failed - radar not connected.");

        //System.Console.WriteLine($"Debug: Trying to Read from data stream... size: {size}"); 
        return dataStream!.GetStream().Read(dataArray, 0, size);
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

        // create the broadcast packet
        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(IPRadarClient.MESSAGE_HEADER_MAGIC);
        writer.Write(IPRadarClient.PROTOCOL_REVISION);
        writer.Write(IPRadarClient.CONFIGURE_NETWORK_KEY);

        Guid guid = new Guid(deviceId);
        writer.Write(guid.ToByteArray());
        writer.Write(IPAddress.Parse(ipAddress).GetAddressBytes());
        writer.Write(IPAddress.Parse(subnetMask).GetAddressBytes());
        writer.Write(IPAddress.Parse(gwAddress).GetAddressBytes());
        writer.Write(staticIP);

        var packet = new byte[IPRadarClient.MESSAGE_HEADER_SIZE + DEVICE_ID_SIZE_BYTES + (IPV4_ADDRESS_SIZE * 3) + 1];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(packet, 0, packet.Length);

        // broadcast for each address
        foreach (var address in broadcastSources)
        {
            IPEndPoint sourceEndpoint = new IPEndPoint(address, 0);
            IPEndPoint targetEndpoint = new IPEndPoint(IPAddress.Broadcast, IPRadarClient.IP_RADAR_BROADCAST_PORT);

            UdpClient sendClient = new UdpClient(sourceEndpoint);
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
            sendClient.Send(packet, targetEndpoint);
        }
    }
}