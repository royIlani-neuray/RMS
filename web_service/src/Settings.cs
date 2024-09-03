/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json;
using System.Text.Json.Serialization;

public class RMSSettings 
{
    public static readonly string SettingsFilePath = "./data/settings.json";

    public class RecordingsRetention
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("expiration_days")]
        public int ExpirationDays { get; set; } = 7;

        [JsonPropertyName("delete_uploaded_only")]
        public bool DeleteUploadedOnly { get; set; } = true;
    }

    public class SettingsData
    {
        [JsonPropertyName("reports_interval")]
        public int ReportsIntervalSec { get; set; } = 1; 
        
        [JsonPropertyName("reports_url")]
        public string ReportsURL { get; set; } = String.Empty;

        [JsonPropertyName("recordings_retention")]
        public RecordingsRetention RecordingsRetentionSettings { get; set; } = new();
    }

    public int ReportsIntervalSec 
    { 
        get { return Settings!.ReportsIntervalSec; }
        set 
        { 
            Settings!.ReportsIntervalSec = value;
            SaveSettings();
        }
    }

    public string ReportsURL 
    { 
        get { return Settings!.ReportsURL; }
        set 
        { 
            Settings!.ReportsURL = value;
            SaveSettings();
        }
    }

    public RecordingsRetention RecordingsRetentionSettings
    { 
        get { return Settings!.RecordingsRetentionSettings; }
        set 
        {
            Settings!.RecordingsRetentionSettings = value;
            SaveSettings();
        }
    }

    private SettingsData? Settings;

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile RMSSettings? instance; 

    public static RMSSettings Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new RMSSettings();
                }
            }

            return instance;
        }
    }

    public string RMSVersion { get; set; } = String.Empty;
    public bool CloudUploadSupport { get; set; } = false;

    private RMSSettings() 
    {
        if (File.Exists(SettingsFilePath))
        {
            string jsonString = File.ReadAllText(SettingsFilePath);
            Settings = JsonSerializer.Deserialize<SettingsData>(jsonString);
        }
        else 
        {
            Settings = new SettingsData();
            SaveSettings();
        }
    }

    #endregion

    private void SaveSettings()
    {
        string jsonString = JsonSerializer.Serialize(Settings);
        File.WriteAllText(SettingsFilePath, jsonString);
    }

    public SettingsData GetSettings()
    {
        return Settings!;
    }

    public void UpdateSettings(SettingsData settings)
    {
        this.Settings = settings;
    }

}