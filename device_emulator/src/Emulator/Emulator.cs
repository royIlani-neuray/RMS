/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/

using System.Text.Json.Serialization;
using DeviceEmulator.Recordings;
using DeviceEmulator.RMS;

namespace DeviceEmulator;

public class Emulator {

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile Emulator? instance; 

    public static Emulator Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new Emulator();
                }
            }

            return instance;
        }
    }

    private Emulator() 
    {
        deviceId = EmulatorSettings.Instance.DeviceId.ToString();
        playback = new PlaybackArgs();

        var recordingPath = Environment.GetEnvironmentVariable("RMS_RECORDING_PATH");

        if (recordingPath != null)
        {
            System.Console.WriteLine($"Override recording path to: {recordingPath}");
            RecordingsFolderPath = recordingPath;
        }
    }

    #endregion

    public class PlaybackArgs 
    {
        [JsonPropertyName("recording_name")]
        public string RecordingName { get; set; } = String.Empty;

        [JsonPropertyName("device_id")]
        public string DeviceId { get; set; } = String.Empty;

        [JsonPropertyName("loop_forever")]
        public bool LoopForever { get; set; } = false;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(RecordingName))
                throw new HttpRequestException("Missing recording name.");
            if (string.IsNullOrWhiteSpace(DeviceId))
                throw new HttpRequestException("Missing device id.");
        }
    }

    public string RecordingsFolderPath = "./data/recordings";

    private Task? emulatorTask;
    private RMSClient? rmsClient;
    private string deviceId;
    private EmulatorDevice? device;

    private PlaybackArgs playback;

    private void GetRecordingFilesPath(string recordingName, string deviceId, out string playbackDataPath, out string deviceInfoPath)
    {
        var recordingPath = Path.Combine(RecordingsFolderPath, recordingName);
        var entryPath = Path.Combine(recordingPath, $"Radar_{deviceId}");
        playbackDataPath = Path.Combine(entryPath, "radar.rrec");
        deviceInfoPath = Path.Combine(entryPath, "radar.json");
    }

    public async Task SetPlaybackAsync(PlaybackArgs playbackArgs)
    {
        playbackArgs.Validate();

        GetRecordingFilesPath(playbackArgs.RecordingName, playbackArgs.DeviceId, out string playbackDataPath, out string deviceInfoPath);
        var device = DeviceInfo.LoadFromFile(deviceInfoPath);
        
        System.Console.WriteLine();
        System.Console.WriteLine("** Loading playback file **");
        System.Console.WriteLine($"** Recorded Device Name: {device.Name}");
        System.Console.WriteLine($"** Recorded Device Id: {device.Id}");
        System.Console.WriteLine($"** Loop forever: {playbackArgs.LoopForever}");
        System.Console.WriteLine();

        RecordingStreamer.Instance.SetRecordingSource(playbackDataPath, playbackArgs.LoopForever);

        playback = playbackArgs;
        
        await RegisterEmulatorAsync(); // register the device again in case it was removed.

        System.Console.WriteLine("Update emulator device configuration script");
        await rmsClient!.SetDeviceConfig(deviceId, device.ConfigScript);

        System.Console.WriteLine("Sending device info broadcast");
        rmsClient!.SendDeviceDiscoveryMessage(deviceId);
    }

    public PlaybackArgs GetPlayback()
    {
        return playback;
    }

    public void Start()
    {
        rmsClient = new RMSClient();
        emulatorTask = Task.Run(EmulatorMain);

        // register the device in RMS if needed.
        Task.Run(RegisterEmulatorAsync);
    }

    public async Task RegisterEmulatorAsync()
    {
        // check if already registerd
        if (await rmsClient!.IsDeviceRegisterdAsync(deviceId))
        {
            System.Console.WriteLine("The device is already registed in RMS.");
            return;
        }
        
        bool registerd = await rmsClient!.RegisterDeviceAsync(deviceId);

        if (!registerd)
            throw new Exception("Could not register the emulator in RMS");

        System.Console.WriteLine("Registered the Emulator in RMS.");
    }

    private void EmulatorMain()
    {
        System.Console.WriteLine($"Emulator Device ID: {deviceId}");

        System.Console.WriteLine("Starting the device...");
        device = new EmulatorDevice();

        System.Console.WriteLine("Sending device info broadcast");
        rmsClient!.SendDeviceDiscoveryMessage(deviceId);

        try
        {
            device!.Run();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: unexpcted error during device run - {ex.Message}");
        }
        
    }

}