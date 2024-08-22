/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Serilog;

namespace WebService.Database;

public class StorageDatabase
{
    public const string RadarStoragePath = "./data/radars";
    public const string CameraStoragePath = "./data/cameras";
    public const string TemplateStoragePath = "./data/templates";
    public const string UserStoragePath = "./data/users";
    public const string RecordingSchedulesStoragePath = "./data/recordings_schedules";


    public static void DatabaseInit()
    {
        // create storage folders for entities

        if (!System.IO.Directory.Exists(RadarStoragePath))
        {
            Log.Information("Creating radars storage folder.");
            System.IO.Directory.CreateDirectory(RadarStoragePath);
        }

        if (!System.IO.Directory.Exists(UserStoragePath))
        {
            Log.Information("Creating users storage folder.");
            System.IO.Directory.CreateDirectory(UserStoragePath);
        }

        if (!System.IO.Directory.Exists(CameraStoragePath))
        {
            Log.Information("Creating cameras storage folder.");
            System.IO.Directory.CreateDirectory(CameraStoragePath);
        }

        if (!System.IO.Directory.Exists(RecordingSchedulesStoragePath))
        {
            Log.Information("Creating recording schedules storage folder.");
            System.IO.Directory.CreateDirectory(RecordingSchedulesStoragePath);
        }


        if (!System.IO.Directory.Exists(TemplateStoragePath))
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
    }
}