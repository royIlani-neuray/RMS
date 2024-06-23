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

public class ServiceSettings 
{
    public static readonly string SettingsFilePath = "./data/settings.json";

    public class SettingsData
    {
        [JsonPropertyName("reports_interval")]
        public int ReportsIntervalSec { get; set; } = 1; 
        
        [JsonPropertyName("reports_url")]
        public string ReportsURL { get; set; } = String.Empty;
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

    private SettingsData? Settings;

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile ServiceSettings? instance; 

    public static ServiceSettings Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new ServiceSettings();
                }
            }

            return instance;
        }
    }

    public string RMSVersion { get; set; } = String.Empty;
    public bool CloudUploadSupport { get; set; } = false;

    private ServiceSettings() 
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