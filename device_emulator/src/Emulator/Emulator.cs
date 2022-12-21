/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json;
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

    public class DeviceSettings 
    {
        [JsonPropertyName("name")]
        public String Name { get; set; } = String.Empty;

        [JsonPropertyName("device_id")]
        public String Id { get; set; } = String.Empty;

        [JsonPropertyName("config_script")]
        public List<string> ConfigScript { get; set; } = new List<string>();
    }

    public const string RecordingsFolderPath = "./data/recordings";
    public const string RecordingDataFileExtention = ".bin";
    public const string RecordingSettingFileExtention = ".json";

    private Task? emulatorTask;
    private RMSClient? rmsClient;
    private string deviceId;
    private EmulatorDevice? device;

    private PlaybackArgs playback;

    public async Task SetPlaybackAsync(PlaybackArgs playbackArgs)
    {
        playbackArgs.Validate();
        var deviceSettings = GetDeviceSettings(playbackArgs.PlaybackFile);
        
        System.Console.WriteLine();
        System.Console.WriteLine("** Loading playback file **");
        System.Console.WriteLine($"** Recorded Device Name: {deviceSettings.Name}");
        System.Console.WriteLine($"** Recorded Device Id: {deviceSettings.Id}");
        System.Console.WriteLine($"** Loop forever: {playbackArgs.LoopForever}");
        System.Console.WriteLine();

        RecordingStreamer.Instance.SetRecordingSource(playbackArgs.PlaybackFile, playbackArgs.LoopForever);

        playback = playbackArgs;
        
        await RegisterEmulatorAsync(); // register the device again in case it was removed.

        System.Console.WriteLine("Update emulator device configuration script");
        await rmsClient!.SetDeviceConfig(deviceId, deviceSettings.ConfigScript);

        System.Console.WriteLine("Sending device info broadcast");
        rmsClient!.SendDeviceDiscoveryMessage(deviceId);
    }

    private DeviceSettings GetDeviceSettings(string playbackFileName)
    {
        string deviceFileName = $"{System.IO.Path.GetFileNameWithoutExtension(playbackFileName)}{RecordingSettingFileExtention}";
        string deviceFilePath = System.IO.Path.Combine(RecordingsFolderPath, deviceFileName);
        string jsonString = File.ReadAllText(deviceFilePath);
        return JsonSerializer.Deserialize<DeviceSettings>(jsonString)!;
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

    public class RecordingInfo 
    {
        [JsonPropertyName("device_name")]
        public String Name { get; set; } = String.Empty;

        [JsonPropertyName("device_id")]
        public String Id { get; set; } = String.Empty;

        [JsonPropertyName("file_name")]
        public String Filename { get; set; } = String.Empty;

        [JsonPropertyName("file_size_bytes")]
        public float FileSizeBytes { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = String.Empty;

    }

    public List<RecordingInfo> GetRecordingsList()
    {
        List<RecordingInfo> recordings = new List<RecordingInfo>();

        var files = System.IO.Directory.GetFiles(RecordingsFolderPath, "*" + RecordingDataFileExtention);

        foreach (string filePath in files)
        {
            string filename = System.IO.Path.GetFileName(filePath);

            var deviceSettings = GetDeviceSettings(filename);

            float fileSizeBytes = (new FileInfo(filePath).Length);
            string timestamp = System.IO.Path.GetFileNameWithoutExtension(filename).Substring(filename.IndexOf('_') + 1);

            recordings.Add(new RecordingInfo() 
            {
                Name = deviceSettings.Name,
                Id = deviceSettings.Id,
                Filename = filename,
                FileSizeBytes = fileSizeBytes,
                Timestamp = timestamp
            });
        }

        return recordings;
    }

    public void DeleteRecording(string recordingFile)
    {
        if (String.IsNullOrWhiteSpace(recordingFile))
        {
            throw new BadRequestException("Recording file not provided");
        }

        if (System.IO.Path.GetExtension(recordingFile) != RecordingDataFileExtention)
        {
            throw new BadRequestException("Invalid recording file");
        }

        var dataFilePath = System.IO.Path.Combine(RecordingsFolderPath, recordingFile);
        
        if (!File.Exists(dataFilePath))
        {
            throw new BadRequestException($"Cannot find the given recording file: {recordingFile}");
        }

        string fileBaseName = System.IO.Path.GetFileNameWithoutExtension(recordingFile);

        string metaFilePath = System.IO.Path.Combine(RecordingsFolderPath, $"{fileBaseName}{RecordingSettingFileExtention}");

        try
        {
            File.Delete(dataFilePath);

            if (File.Exists(metaFilePath))
            {
                File.Delete(metaFilePath);
            }
        }
        catch
        {
            throw new Exception("Cannot delete the given recording file. Recording/Playback might be in progress.");
        }

    }
}