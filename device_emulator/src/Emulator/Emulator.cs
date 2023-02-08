/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
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
    }

    #endregion

    public class PlaybackArgs 
    {
        [JsonPropertyName("playback_file")]
        public string PlaybackFile { get; set; } = String.Empty;

        [JsonPropertyName("loop_forever")]
        public bool LoopForever { get; set; } = false;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(PlaybackFile))
                throw new HttpRequestException("Missing playback file name.");
        }
    }

    private Task? emulatorTask;
    private RMSClient? rmsClient;
    private string deviceId;
    private EmulatorDevice? device;

    private PlaybackArgs playback;

    public async Task SetPlaybackAsync(PlaybackArgs playbackArgs)
    {
        playbackArgs.Validate();
        var deviceSettings = RecordingsManager.Instance.GetDeviceSettings(playbackArgs.PlaybackFile);
        
        System.Console.WriteLine();
        System.Console.WriteLine("** Loading playback file **");
        System.Console.WriteLine($"** Recorded Device Name: {deviceSettings.Name}");
        System.Console.WriteLine($"** Recorded Device Id: {deviceSettings.Id}");
        System.Console.WriteLine($"** Loop forever: {playbackArgs.LoopForever}");
        System.Console.WriteLine();

        string playbackFilePath = System.IO.Path.Combine(RecordingsManager.Instance.RecordingsFolderPath, playbackArgs.PlaybackFile);

        RecordingStreamer.Instance.SetRecordingSource(playbackFilePath, playbackArgs.LoopForever);

        playback = playbackArgs;
        
        await RegisterEmulatorAsync(); // register the device again in case it was removed.

        System.Console.WriteLine("Update emulator device configuration script");
        await rmsClient!.SetDeviceConfig(deviceId, deviceSettings.ConfigScript);

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
        emulatorTask = Task.Run(() => EmulatorMain());
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

    private async Task EmulatorMain()
    {
        System.Console.WriteLine($"Emulator Device ID: {deviceId}");

        System.Console.WriteLine("Starting the device...");
        device = new EmulatorDevice();

        await RegisterEmulatorAsync();

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