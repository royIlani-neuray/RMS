using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DeviceEmulator.RMS;

public class RMSClient 
{
    private HttpClient httpClient;
    private string baseUrl;

    public RMSClient()
    {
        httpClient = new HttpClient();
        baseUrl = "localhost:16500";
    }

    public async Task<List<RadarDeviceBrief>> GetRegisteredDevicesAsync()
    {
        var devices = await httpClient.GetFromJsonAsync<List<RadarDeviceBrief>>($"http://{baseUrl}/devices");

        if (devices == null)
            throw new Exception("Failed to get device list from RMS");

        return devices!;
    }

    public async Task<bool> IsDeviceRegisterdAsync(string deviceId)
    {
        var devices = await GetRegisteredDevicesAsync();
        return devices.Exists(device => device.Id == deviceId);
    }

    public async Task<bool> SetDeviceConfig(string deviceId, List<string> config)
    {
        var requestBody = new 
        {
            config
        };

        var response = await httpClient.PostAsJsonAsync($"http://{baseUrl}/devices/{deviceId}/config", requestBody);

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            System.Console.WriteLine($"SetDeviceConfig: status code: {response.StatusCode} \n{responseBody}");
            return false;
        }

        return true;
    }

    public async Task<bool> RegisterDeviceAsync(string deviceId)
    {
        var requestBody = new 
        {
            name = "Device Emulator",
            description = "An app for streaming pre-recorded radar data.",
            device_id = deviceId,

            template_id = "",

            enabled = true,

            radar_position = new 
            {
                height = 1,
                azimuth_tilt = 0,
                elevation_tilt = 0
            }
        };

        var response = await httpClient.PostAsJsonAsync($"http://{baseUrl}/devices", requestBody);

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            System.Console.WriteLine($"RegisterDeviceAsync: status code: {response.StatusCode} \n{responseBody}");
            return false;
        }

        return true;
    }

    private List<IPAddress> GetBroadcastAddresses()
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


    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    public void SendDeviceDiscoveryMessage(string deviceId)
    {
        List<IPAddress> broadcastSources = GetBroadcastAddresses();
        
        // create the broadcast packet

        var stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(EmulatorDevice.MESSAGE_HEADER_MAGIC);
        writer.Write(EmulatorDevice.PROTOCOL_REVISION);
        writer.Write(EmulatorDevice.DEVICE_INFO_KEY);

        Guid guid = Guid.Parse(deviceId);
        writer.Write(guid.ToByteArray());

        IPAddress ip = IPAddress.Parse(GetLocalIPAddress());
        writer.Write(ip.GetAddressBytes());

        IPAddress subnet = IPAddress.Parse("255.255.255.0");
        writer.Write(subnet.GetAddressBytes());

        IPAddress gw = IPAddress.Parse("0.0.0.0");
        writer.Write(gw.GetAddressBytes());

        bool isStaticIpSet = false;
        writer.Write(isStaticIpSet);

        string model = "DEVICE_EMULATOR";
        var modelBytes = System.Text.Encoding.ASCII.GetBytes(model);

        if (modelBytes.Length < EmulatorDevice.MODEL_STRING_MAX_LENGTH)
        {
            modelBytes = modelBytes.Concat(new byte[EmulatorDevice.MODEL_STRING_MAX_LENGTH - modelBytes.Length]).ToArray();
        }

        writer.Write(modelBytes);

        string appName = "EMULATOR_APPLICATION";
        var appNameBytes = System.Text.Encoding.ASCII.GetBytes(appName);

        if (appNameBytes.Length < EmulatorDevice.APP_STRING_MAX_LENGTH)
        {
            appNameBytes = appNameBytes.Concat(new byte[EmulatorDevice.APP_STRING_MAX_LENGTH - appNameBytes.Length]).ToArray();
        }

        writer.Write(appNameBytes);

        uint fwVersion = 0x10000000; // 1.0.0.0
        writer.Write(fwVersion);

        var packet = new byte[stream.Length];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(packet, 0, packet.Length);

        // send for each address
        foreach (var address in broadcastSources)
        {
            //System.Console.WriteLine($"address: {address}");

            IPEndPoint sourceEndpoint = new IPEndPoint(address, 0);
            IPEndPoint targetEndpoint = new IPEndPoint(IPAddress.Broadcast, EmulatorDevice.IP_RADAR_BROADCAST_PORT_SERVER);

            UdpClient sendClient = new UdpClient(sourceEndpoint);
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
            sendClient.Send(packet, targetEndpoint);
        }
    }
}