/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
namespace WebService.Database;

public class Database
{
    public static void DatabaseInit()
    {
        // create storage folders for entities

        if (!System.IO.Directory.Exists(DeviceStorage.StoragePath))
        {
            System.Console.WriteLine("Creating devices storage folder.");
            System.IO.Directory.CreateDirectory(DeviceStorage.StoragePath);
        }

        if (!System.IO.Directory.Exists(UserStorage.StoragePath))
        {
            System.Console.WriteLine("Creating users storage folder.");
            System.IO.Directory.CreateDirectory(UserStorage.StoragePath);
        }

        if (!System.IO.Directory.Exists(TemplateStorage.StoragePath))
        {
            System.Console.WriteLine("Creating templates storage folder.");
            System.IO.Directory.CreateDirectory(TemplateStorage.StoragePath);

            // copy default templates

            foreach (string templateFilePath in Directory.GetFiles("./default_templates"))
            {

                if (!templateFilePath.EndsWith(TemplateStorage.TemplateFileExtention))
                    continue;

                string filename = System.IO.Path.GetFileName(templateFilePath);
                string targetPath = System.IO.Path.Combine(TemplateStorage.StoragePath, filename);
                File.Copy(templateFilePath, targetPath);
            }

        }
    }
}