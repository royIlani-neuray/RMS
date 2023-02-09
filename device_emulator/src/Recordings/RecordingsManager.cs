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
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;

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

    private void GetRecordingFilePaths(string recordingFile, out string dataFilePath, out string metaFilePath)
    {
        if (String.IsNullOrWhiteSpace(recordingFile))
        {
            throw new BadRequestException("Recording file not provided");
        }

        if (System.IO.Path.GetExtension(recordingFile) != RecordingDataFileExtention)
        {
            throw new BadRequestException("Invalid recording file");
        }

        dataFilePath = System.IO.Path.Combine(RecordingsFolderPath, recordingFile);

        if (!File.Exists(dataFilePath))
        {
            throw new BadRequestException($"Cannot find the given recording file: {recordingFile}");
        }

        string fileBaseName = System.IO.Path.GetFileNameWithoutExtension(recordingFile);

        metaFilePath = System.IO.Path.Combine(RecordingsFolderPath, $"{fileBaseName}{RecordingSettingFileExtention}");
    }

    public void DeleteRecording(string recordingFile)
    {
        GetRecordingFilePaths(recordingFile, out string dataFilePath, out string metaFilePath);

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

    public async Task<byte[]> DownloadRecording(string recordingFile)
    {
        GetRecordingFilePaths(recordingFile, out string dataFilePath, out string metaFilePath);

        List<string> zipFiles = new List<string>() { dataFilePath, metaFilePath };

        using (var outStream = new MemoryStream())
        {
            using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
            {
                foreach (var filePath in zipFiles)
                {
                    var fileInArchive = archive.CreateEntry(Path.GetFileName(filePath), CompressionLevel.Optimal);
                    using (var entryStream = fileInArchive.Open())
                    {
                        using (var fileCompressionStream = new MemoryStream(await System.IO.File.ReadAllBytesAsync(filePath)))
                        {
                            await fileCompressionStream.CopyToAsync(entryStream);
                        }
                    }
                }
            }

            outStream.Position = 0;
            return outStream.ToArray();
        }
    }

    public void UploadRecording(Stream fileStream)
    {
        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read, true))
        {
            if (archive.Entries.Count != 2)
            {
                throw new BadRequestException("Invalid zip file provided.");
            }

            if (!archive.Entries.ToList().Exists(entry => entry.Name.EndsWith(RecordingDataFileExtention)))
            {
                throw new BadRequestException("Missing recording file in zip.");
            }

            if (!archive.Entries.ToList().Exists(entry => entry.Name.EndsWith(RecordingSettingFileExtention)))
            {
                throw new BadRequestException("Missing recording meta file in zip.");
            }
            
            foreach (var fileEntry in archive.Entries)
            {
                using (var entryStream = fileEntry.Open())
                {
                    System.Console.WriteLine($"Extracting {fileEntry.Name} to recording library...");
                    var targetPath = Path.Combine(RecordingsFolderPath, fileEntry.Name);
                    fileEntry.ExtractToFile(targetPath, true);
                }
            }
        }        

    }
}