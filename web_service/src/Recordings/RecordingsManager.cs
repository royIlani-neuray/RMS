/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/

using WebService.Entites;
using System.IO.Compression;
using System.Text.Json.Serialization;

namespace WebService.Recordings;

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
        if (!Directory.Exists(RecordingsStoragePath))
        {
            System.Console.WriteLine($"Creating recordings folder: {RecordingsStoragePath}");
            Directory.CreateDirectory(RecordingsStoragePath);
        }
    }

    #endregion

    public class RenameRecordingArgs
    {
        [JsonPropertyName("new_name")]
        public string NewRecordingName { get; set; } = String.Empty;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(NewRecordingName))
                throw new BadRequestException("new_name wasn't provided.");
        }
    }

    public readonly string RecordingsStoragePath = "./data/recordings";
    public readonly string TempArchiveStoragePath = "/tmp/rms_recordings";

    public readonly string RecordingMetaFileName = "recording.json";

    private object syncLock = new Object();

    public const string RECORDING_OVERRIDE_KEY = "RECORDING_NAME";

    private bool IsEntryNameValid(string recordingName)
    {
        return recordingName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
    }

    private string GetRecordingPath(string recordingName)
    {
        return Path.Combine(RecordingsStoragePath, recordingName);
    }

    private string GetRecordingMetaFilePath(string recordingName)
    {
        return Path.Combine(GetRecordingPath(recordingName), RecordingMetaFileName);
    }

    private bool IsRecordingExist(string recordingName)
    {
        return File.Exists(GetRecordingMetaFilePath(recordingName));
    }

    private RecordingInfo CreateRecordingFolder(string? recordingName)
    {
        var recording = new RecordingInfo();

        if (!string.IsNullOrWhiteSpace(recordingName))
        {
            if (!IsEntryNameValid(recordingName))
                throw new BadRequestException("Invalid recording name provided.");
            
            recording.Name = recordingName;
        }

        if (IsRecordingExist(recording.Name))
        {
            recording = RecordingInfo.LoadFromFile(GetRecordingMetaFilePath(recording.Name));
        }
        else
        {
            Directory.CreateDirectory(GetRecordingPath(recording.Name));
            recording.SaveToFile(GetRecordingMetaFilePath(recording.Name));
        }

        return recording;
    }

    public void CreateRecordingEntry(DeviceEntity device, out string entryPath, string? recordingNameOverride = "")
    {
        lock(syncLock)
        {
            RecordingInfo recording = CreateRecordingFolder(recordingNameOverride);
            string recordingPath = GetRecordingPath(recording.Name);
            entryPath = Path.Combine(recordingPath, $"{device.Type}_{device.Id}");
            Directory.CreateDirectory(entryPath);

            recording.RecordingEntries.Add(new RecordingInfo.RecordingEntry() {
                DeviceId = device.Id,
                DeviceName = device.Name,
                DeviceType = device.Type
            });

            recording.SaveToFile(GetRecordingMetaFilePath(recording.Name));
        }
    }

    public void DeleteRecording(string recordingName)
    {
        lock(syncLock)
        {
            if (!IsRecordingExist(recordingName))
                throw new NotFoundException($"There is no recording entry named: {recordingName}");

            var recordingPath = GetRecordingPath(recordingName);
            System.IO.DirectoryInfo directoryInfo = new DirectoryInfo(recordingPath);
            directoryInfo.Delete(true);
        }
    }

    public void RenameRecording(string recordingName, string newName)
    {
        if (!IsEntryNameValid(newName))
            throw new BadRequestException("Invalid new recording name provided.");

        lock(syncLock)
        {
            if (!IsRecordingExist(recordingName))
                throw new NotFoundException($"There is no recording entry named: {recordingName}");

            var recordingPath = GetRecordingPath(recordingName);
            var newRecordingPath = GetRecordingPath(newName);

            System.IO.DirectoryInfo directoryInfo = new DirectoryInfo(recordingPath);
            directoryInfo.MoveTo(newRecordingPath);
            
            // rename the name in the mata file
            var recording = RecordingInfo.LoadFromFile(GetRecordingMetaFilePath(newName));
            recording.Name = newName;
            recording.SaveToFile(GetRecordingMetaFilePath(recording.Name));
        }       
    }

    private void UpdateEntrySizeBytes(RecordingInfo recording)
    {
        foreach (var entry in recording.RecordingEntries)
        {
            float entrySizeBytes = 0;
            string recordingPath = GetRecordingPath(recording.Name);
            var entryPath = Path.Combine(recordingPath, $"{entry.DeviceType}_{entry.DeviceId}");
            DirectoryInfo entryDirInfo = new DirectoryInfo(entryPath);

            foreach (var entryFile in entryDirInfo.EnumerateFiles())
            {
                entrySizeBytes += (new FileInfo(entryFile.FullName).Length);
            }

            entry.EntrySizeBytes = entrySizeBytes;
        }
    }

    public List<RecordingInfo> GetRecordings()
    {
        List<RecordingInfo> recordings = new List<RecordingInfo>();

        lock(syncLock)
        {
            DirectoryInfo storageDir = new DirectoryInfo(RecordingsStoragePath);

            foreach (var recordingDir in storageDir.EnumerateDirectories())
            {
                try
                {
                    RecordingInfo recording = RecordingInfo.LoadFromFile(GetRecordingMetaFilePath(recordingDir.Name));
                    UpdateEntrySizeBytes(recording);
                    recordings.Add(recording);
                }
                catch 
                {
                    System.Console.WriteLine($"Failed to load recording info from folder: {recordingDir.Name}");
                }
            }
        }

        return recordings;
    }

    private void ClearTempStorage()
    {
        var tempFiles = Directory.GetFiles(TempArchiveStoragePath);

        foreach (string tempFile in tempFiles)
        {
            try
            {
                System.Console.WriteLine($"Debug: delete existing file: {tempFile}...");
                File.Delete(tempFile);
            }
            catch {}
        }
    }

    public Stream DownloadRecording(string recordingName, out string archiveFileName)
    {
        Directory.CreateDirectory(TempArchiveStoragePath);

        if (!IsRecordingExist(recordingName))
            throw new NotFoundException($"There is no recording entry named: {recordingName}");
        
        var recordingPath = GetRecordingPath(recordingName);
        archiveFileName = recordingName + ".zip";
        string zipFilePath = Path.Combine(TempArchiveStoragePath, archiveFileName);

        ClearTempStorage();
        
        ZipFile.CreateFromDirectory(recordingPath, zipFilePath, CompressionLevel.Optimal, true);

        FileStream stream = new FileStream(zipFilePath, FileMode.Open);

        return stream; 
    }

    public void UploadRecording(Stream fileStream)
    {
        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read, true))
        {
            if (!archive.Entries.ToList().Exists(entry => entry.Name == RecordingMetaFileName))
            {
                throw new BadRequestException("Missing recording meta file in zip.");
            }

            foreach (var fileEntry in archive.Entries)
            {
                using (var entryStream = fileEntry.Open())
                {
                    var targetPath = Path.Combine(RecordingsStoragePath, fileEntry.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                    fileEntry.ExtractToFile(targetPath, true);
                }
            }
        }        
    }
    
}