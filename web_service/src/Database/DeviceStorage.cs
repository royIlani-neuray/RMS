/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using System.Text.Json;

namespace WebService.Database;

public class DeviceStorage {
    public static readonly string StoragePath = "./data/devices";
    public static readonly string DeviceFileExtention = ".json";
    
    public static void SaveDevice(RadarDevice device)
    {
        string jsonString = JsonSerializer.Serialize(device);
        File.WriteAllText(System.IO.Path.Combine(StoragePath, device.Id + DeviceFileExtention), jsonString);
    }

    public static void DeleteDevice(RadarDevice device)
    {
        string filePath = System.IO.Path.Combine(StoragePath, device.Id + DeviceFileExtention);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        else
        {
            Console.WriteLine($"Warning: could not delete device file for device: {device.Id}");
        }
    }

    public static Dictionary<string, RadarDevice> LoadAllDevices()
    {
        Dictionary<string, RadarDevice> devices = new Dictionary<string, RadarDevice>();

        var files = System.IO.Directory.GetFiles(StoragePath, "*" + DeviceFileExtention);
        foreach (string filePath in files)
        {
            string jsonString = File.ReadAllText(filePath);
            
            RadarDevice? device = JsonSerializer.Deserialize<RadarDevice>(jsonString);

            if (device == null)
                throw new Exception("deserialze failed!");

            devices.Add(device.Id, device);
        }

        Console.WriteLine($"Loaded {devices.Keys.Count} devices from storage.");
        return devices;
    }


}