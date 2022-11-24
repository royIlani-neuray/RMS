using System;
using System.Threading;
using System.Threading.Tasks;

using DeviceEmulator;

public class Startup 
{
    public static void ApplicationStart(ConfigurationManager config)
    {
        string version = config["emulator_version"]!;
        EmulatorSettings.Instance.EmulatorVersion = version;

        Console.WriteLine($"Radar Device Emulator Started - Version: {version}");

        Console.WriteLine($"Running as user: {Environment.UserName}");

        Emulator.Instance.Start();
    }

}