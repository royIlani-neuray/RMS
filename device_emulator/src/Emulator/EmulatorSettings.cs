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

namespace DeviceEmulator;

public class EmulatorSettings 
{
    public static readonly string SettingsFilePath = "./data/settings.json";

    public class SettingsData
    {
        [JsonPropertyName("device_id")]
        public Guid DeviceId { get; set; } = Guid.NewGuid(); 
    }

    public Guid DeviceId 
    { 
        get { return Settings!.DeviceId; }
        set 
        { 
            Settings!.DeviceId = value;
            SaveSettings();
        }
    }

    private SettingsData? Settings;

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile EmulatorSettings? instance; 

    public static EmulatorSettings Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new EmulatorSettings();
                }
            }

            return instance;
        }
    }

    public string EmulatorVersion { get; set; } = String.Empty;

    private EmulatorSettings() 
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

}