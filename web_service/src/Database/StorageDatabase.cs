/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Serilog;
using WebService.Actions.Users;

namespace WebService.Database;

public class StorageDatabase
{
    public const string RadarStoragePath = "./data/radars";
    public const string CameraStoragePath = "./data/cameras";
    public const string DeviceGroupStoragePath = "./data/device_groups";
    public const string TemplateStoragePath = "./data/templates";
    public const string UserStoragePath = "./data/users";
    public const string RecordingSchedulesStoragePath = "./data/recordings_schedules";


    private static void InitTemplatesStorage()
    {
        Log.Information("Creating templates storage folder.");
        System.IO.Directory.CreateDirectory(TemplateStoragePath);

        // copy default templates
        foreach (string templateFilePath in Directory.GetFiles("./default_templates"))
        {
            if (!templateFilePath.EndsWith(EntityStorage<Entites.RadarTemplate>.StorageFileExtention))
                continue;

            string filename = System.IO.Path.GetFileName(templateFilePath);
            string targetPath = System.IO.Path.Combine(TemplateStoragePath, filename);
            File.Copy(templateFilePath, targetPath);
        }
    }

    private static void InitUsersStorage()
    {
        Log.Information("Creating users storage folder.");
        System.IO.Directory.CreateDirectory(UserStoragePath);

        // in case there are no users registered in the system we create a default user for initial access
        var addUserAction = new AddUserAction(new AddUserArgs()
        {
            FirstName = "neuRayLabs",
            LastName = "Admin",
            Email = "admin@neuRay.ai",
            Password = "DefaultPassw0rd!",
            Roles = ["Administrator"],
            EmployeeId = "00000000"
        });

        addUserAction.Run();
    }

    public static void DatabaseInit()
    {
        // create storage folders for entities
        if (!System.IO.Directory.Exists(UserStoragePath))
        {
            InitUsersStorage();
        }

        if (!System.IO.Directory.Exists(TemplateStoragePath))
        {
            InitTemplatesStorage();
        }

        if (!System.IO.Directory.Exists(RadarStoragePath))
        {
            Log.Information("Creating radars storage folder.");
            System.IO.Directory.CreateDirectory(RadarStoragePath);
        }

        if (!System.IO.Directory.Exists(CameraStoragePath))
        {
            Log.Information("Creating cameras storage folder.");
            System.IO.Directory.CreateDirectory(CameraStoragePath);
        }

        if (!System.IO.Directory.Exists(DeviceGroupStoragePath))
        {
            Log.Information("Creating device groups storage folder.");
            System.IO.Directory.CreateDirectory(DeviceGroupStoragePath);
        }

        if (!System.IO.Directory.Exists(RecordingSchedulesStoragePath))
        {
            Log.Information("Creating recording schedules storage folder.");
            System.IO.Directory.CreateDirectory(RecordingSchedulesStoragePath);
        }
    }
}