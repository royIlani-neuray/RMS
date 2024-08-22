/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System;
using System.IO;
using System.Text.Json.Serialization;
using Serilog;

namespace WebService.Actions.SystemActions;

public class GetStorageInfoResults
{
    [JsonPropertyName("total_size_bytes")]
    public long TotalSizeBytes { get; set; }
    
    [JsonPropertyName("free_space_bytes")]
    public long FreeSpaceBytes { get; set; }

    [JsonPropertyName("free_space_percent")]
    public double FreeSpacePercent { get; set; } 
}

public class GetStorageInfoAction : IAction 
{
    const int BYTES_IN_MEGABYTE = (1024 * 1024);

    public GetStorageInfoResults? Results { get; set; }

    public void Run()
    {
        DriveInfo drive = new DriveInfo("/");
        
        long totalSpaceMB = drive.TotalSize / BYTES_IN_MEGABYTE;
        long freeSpaceMB = drive.TotalFreeSpace  / BYTES_IN_MEGABYTE;
        double percentFree = ((double) drive.TotalFreeSpace / drive.TotalSize) * 100;

        Log.Information($"Storage Info: {freeSpaceMB} MB is available out of {totalSpaceMB} MB [{percentFree:0.00}% free]");

        Results = new GetStorageInfoResults()
        {
            TotalSizeBytes = drive.TotalSize,
            FreeSpaceBytes = drive.TotalFreeSpace,
            FreeSpacePercent = percentFree
        };
    }
} 