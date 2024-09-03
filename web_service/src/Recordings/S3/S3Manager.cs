/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Transfer;
using Serilog;

public sealed class S3Manager {

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile S3Manager? instance; 

    public static S3Manager Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new S3Manager();
                }
            }

            return instance;
        }
    }

    #endregion
    private AmazonS3Client? client;
    private string? rmsName;

    private const string S3_BUCKET = "neuray-rms";
    private const string S3_UPLOAD_DIRECTORY = "recently_recorded";

    public void InitS3Connection(string rmsName)
    {
        var AccessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY");
        var SecretKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY");
        var region = Environment.GetEnvironmentVariable("S3_REGION");
        RMSSettings.Instance.CloudUploadSupport = (AccessKey != null) && (SecretKey != null) && (region != null);
        if (!RMSSettings.Instance.CloudUploadSupport) {
            Log.Warning("No S3 credentials found, not supporting recording uploads");
            return;
        }
        client = new AmazonS3Client(AccessKey, SecretKey, Amazon.RegionEndpoint.GetBySystemName(region));
        this.rmsName = rmsName;
    }

    public async Task UploadDirectoryAsync(string directoryPath, bool withRoot = true)
    {
        if (!RMSSettings.Instance.CloudUploadSupport) {
            Log.Error($"Cannot upload to S3 as cloud is not supported");
            return;
        }
        using var transferUtility = new TransferUtility(client);
        string keyPrefix = $"{S3_UPLOAD_DIRECTORY}/{this.rmsName}/";
        if (withRoot) {
            string dirName = new DirectoryInfo(directoryPath).Name;
            keyPrefix += dirName;
        }
        var uploadRequest = new TransferUtilityUploadDirectoryRequest
        {
            Directory = directoryPath,
            BucketName = S3_BUCKET,
            KeyPrefix = keyPrefix,
            SearchOption = SearchOption.AllDirectories
        };

        await transferUtility.UploadDirectoryAsync(uploadRequest);

        // uploading an empty "done" file at the end
        using var emptyStream = new MemoryStream();
        string doneKeyPrefix = keyPrefix + "/done";
        var fileTransferUtilityRequest = new TransferUtilityUploadRequest
        {
            BucketName = S3_BUCKET,
            InputStream = emptyStream,
            Key = doneKeyPrefix,
            StorageClass = S3StorageClass.Standard,
            CannedACL = S3CannedACL.Private
        };

        await transferUtility.UploadAsync(fileTransferUtilityRequest);
    }
}
