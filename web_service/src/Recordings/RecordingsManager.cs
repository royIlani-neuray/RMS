/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/

using WebService.Entites;
using System.IO.Compression;
using System.Text.Json.Serialization;
using WebService.Context;
using WebService.Services.RadarRecording;
using WebService.Events;
using Serilog;
using WebService.Actions.Services;
using WebService.Services.CameraRecording;
using WebService.Scheduler;
using WebService.Actions.Recordings;

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
            Log.Information($"Creating recordings folder: {RecordingsStoragePath}");
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

    public const string RECORDING_NAME = "RECORDING_NAME";
    public const string UPLOAD_S3 = "UPLOAD_S3";

    public void Init()
    {
        FixRecordingsUploadState();
        StopAllRunningRecordings();
        RecordingScheduler.Instance.RestoreSchedules();
    }

    public void FixRecordingsUploadState()
    {
        lock(syncLock)
        {
            Log.Information("Fixing recordings upload state");
            foreach(var recording in GetRecordings().FindAll(recording => recording.IsUploading)) {
                recording.IsUploading = false;
                recording.SaveToFile(GetRecordingMetaFilePath(recording.Name));
            }
        }
    }

    public void StopAllRunningRecordings()
    {
        Log.Information($"Stopping all running recordings");
        StopRecordingArgs args = new()
        {
            RadarIds = RadarContext.Instance.GetRadarsBrief().Select(radar => radar.Id).ToList(),
            CameraIds = CameraContext.Instance.GetCamerasBrief().Select(camera => camera.Id).ToList(),
        };
        var action = new StopRecordingAction(args);
        action.Run();
    }

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

    public bool IsRecordingExist(string recordingName)
    {
        return File.Exists(GetRecordingMetaFilePath(recordingName));
    }

    private RecordingInfo CreateRecordingFolder(string? recordingName, string? uploadS3)
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
            if (!string.IsNullOrWhiteSpace(uploadS3))
            {
                recording.UploadS3 = bool.Parse(uploadS3);
            }
            Directory.CreateDirectory(GetRecordingPath(recording.Name));
            recording.SaveToFile(GetRecordingMetaFilePath(recording.Name));
        }

        return recording;
    }

    public void CreateRecordingEntry(DeviceEntity device, out string entryPath, string? recordingNameOverride = "", string? uploadS3 = "")
    {
        lock(syncLock)
        {
            RecordingInfo recording = CreateRecordingFolder(recordingNameOverride, uploadS3);
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

    public RecordingInfo GetRecording(string recordingName)
    {
        lock(syncLock)
        {
            if (!IsRecordingExist(recordingName))
                throw new NotFoundException($"There is no recording entry named: {recordingName}");

            RecordingInfo recording = RecordingInfo.LoadFromFile(GetRecordingMetaFilePath(recordingName));
            return recording;
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
                catch (Exception ex) 
                {
                    Log.Error($"Failed to load recording info from folder: {recordingDir.Name}", ex);
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
                //Log.Debug($"delete existing file: {tempFile}...");
                File.Delete(tempFile);
            }
            catch {}
        }
    }

    public void MarkDeviceRecordingFinished(string recordingName, string deviceId)
    {
        lock(syncLock)
        {
            if (!IsRecordingExist(recordingName))
                throw new NotFoundException($"There is no recording entry named: {recordingName}");

            RecordingInfo recording = RecordingInfo.LoadFromFile(GetRecordingMetaFilePath(recordingName));
            var deviceIndex = recording.RecordingEntries.FindIndex(entry => entry.DeviceId == deviceId);
            if (deviceIndex != -1) {
                recording.RecordingEntries[deviceIndex].IsFinished = true;
                recording.SaveToFile(GetRecordingMetaFilePath(recording.Name));
            }
        }
    }

    public void MarkUploadStart(string recordingName)
    {
        lock(syncLock)
        {
            if (!IsRecordingExist(recordingName))
                throw new NotFoundException($"There is no recording entry named: {recordingName}");

            RecordingInfo recording = RecordingInfo.LoadFromFile(GetRecordingMetaFilePath(recordingName));
            recording.IsUploading = true;
            recording.SaveToFile(GetRecordingMetaFilePath(recording.Name));
        }
    }

    public void MarkUploadEnd(string recordingName)
    {
        lock(syncLock)
        {
            if (!IsRecordingExist(recordingName))
                throw new NotFoundException($"There is no recording entry named: {recordingName}");

            RecordingInfo recording = RecordingInfo.LoadFromFile(GetRecordingMetaFilePath(recordingName));
            recording.IsUploading = false;
            recording.LastUploaded = DateTime.UtcNow;
            recording.SaveToFile(GetRecordingMetaFilePath(recording.Name));
        }
    }

    public bool IsRecordingFinished(string recordingName)
    {
        lock(syncLock)
        {
            if (!IsRecordingExist(recordingName))
                throw new NotFoundException($"There is no recording entry named: {recordingName}");

            RecordingInfo recording = RecordingInfo.LoadFromFile(GetRecordingMetaFilePath(recordingName));
            return recording.IsFinished();
        }
    }

    public bool IsRecordingUploading(string recordingName)
    {
        lock(syncLock)
        {
            if (!IsRecordingExist(recordingName))
                throw new NotFoundException($"There is no recording entry named: {recordingName}");

            RecordingInfo recording = RecordingInfo.LoadFromFile(GetRecordingMetaFilePath(recordingName));
            return recording.IsUploading;
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

    public bool CanUploadRecordingToS3(string recordingName, bool raiseOnError)
    {
        string message = "";
        if (!RMSSettings.Instance.CloudUploadSupport) {
            message = $"Not supporting cloud upload, not uploading recording {recordingName}";
        } else if (!IsRecordingFinished(recordingName)) {
            message = $"Recording {recordingName} did not finish, not uploading to cloud";
        } else if (IsRecordingUploading(recordingName)) {
            message = $"Recording {recordingName} is already uploading, not uploading again";
        }

        if (message != "") {
            if (raiseOnError) {
                throw new Exception(message);
            }
            return false;
        }
        return true;
    }

    public void UploadRecordingToS3(string recordingName, bool raiseOnError=false)
    {
        if (!CanUploadRecordingToS3(recordingName, raiseOnError)) {
            return;
        }

        lock(syncLock)
        {
            if (!IsRecordingExist(recordingName))
                throw new NotFoundException($"There is no recording entry named: {recordingName}");

            var recordingPath = GetRecordingPath(recordingName);
            Task.Run(async () => {
                Log.Information($"Uploading recording to cloud: {recordingName}");
                RMSEvents.Instance.RecordingUploadCloudStartedEvent(recordingName);
                MarkUploadStart(recordingName);
                await S3Manager.Instance.UploadDirectoryAsync(recordingPath);
                MarkUploadEnd(recordingName);
                Log.Information($"Done uploading recording to cloud: {recordingName}");
                RMSEvents.Instance.RecordingUploadCloudFinishedEvent(recordingName);
            });
        }
    }
}