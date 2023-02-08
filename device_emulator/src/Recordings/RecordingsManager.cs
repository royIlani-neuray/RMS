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

namespace DeviceEmulator.Recordings;

public class RecordingsManager
{
    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile RecordingsManager? instance; 

    public static RecordingsManager Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new RecordingsManager();
                }
            }

            return instance;
        }
    }

    private RecordingsManager() 
    {
        var recordingPath = Environment.GetEnvironmentVariable("RMS_RECORDING_PATH");

        if (recordingPath != null)
        {
            System.Console.WriteLine($"Override recording path to: {recordingPath}");
            RecordingsFolderPath = recordingPath;
        }
    }

    #endregion

    public string RecordingsFolderPath = "./data/recordings";
    public const string RecordingDataFileExtention = ".rrec";
    public const string RecordingSettingFileExtention = ".json";

    public class DeviceSettings 
    {
        [JsonPropertyName("name")]
        public String Name { get; set; } = String.Empty;

        [JsonPropertyName("device_id")]
        public String Id { get; set; } = String.Empty;

        [JsonPropertyName("config_script")]
        public List<string> ConfigScript { get; set; } = new List<string>();
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

    public DeviceSettings GetDeviceSettings(string playbackFileName)
    {
        string deviceFileName = $"{System.IO.Path.GetFileNameWithoutExtension(playbackFileName)}{RecordingsManager.RecordingSettingFileExtention}";
        string deviceFilePath = System.IO.Path.Combine(RecordingsManager.Instance.RecordingsFolderPath, deviceFileName);
        string jsonString = File.ReadAllText(deviceFilePath);
        return JsonSerializer.Deserialize<DeviceSettings>(jsonString)!;
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