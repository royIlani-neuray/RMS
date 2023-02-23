/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Utils;
using WebService.RadarLogic.Tracking;
using System.Text.Json;

namespace WebService.Services.Recording;

public class RecordingContext : WorkerThread<FrameData>, IServiceContext
{
    public IServiceContext.ServiceState State { get; set; }

    public string deviceId;

    private Stream stream;
    private BinaryWriter binaryWriter;


    private const int MAX_QUEUE_CAPACITY = 5;
    
    public RecordingContext(string deviceId, string recordingPath, float frameRate) : base("RecordingContext", MAX_QUEUE_CAPACITY)
    {
        State = IServiceContext.ServiceState.Initialized;
        this.deviceId = deviceId;

        stream = new FileStream(recordingPath, FileMode.Create);
        binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(frameRate);
        binaryWriter.Flush();
    }

    public void RecordFrame(FrameData frameData)
    {
        Enqueue(frameData);
    }

    protected override Task DoWork(FrameData workItem)
    {
        if ((workItem.TracksList.Count == 0) && (workItem.PointsList.Count == 0) && (workItem.TargetsIndexList.Count == 0))
        {
            // empty frame, store just a 0 for optimization
            uint frameBytesSize = 0;
            binaryWriter.Write(frameBytesSize);
        }
        else
        {
            // serialize the frame as json (a bit expensive size wise)
            string jsonString = JsonSerializer.Serialize(workItem);
            byte [] frameBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
            UInt32 frameBytesSize = (uint) frameBytes.Length;
            binaryWriter.Write(frameBytesSize);
            binaryWriter.Write(frameBytes);
        }

        binaryWriter.Flush();
        
        return Task.CompletedTask;
    }

    ~RecordingContext()
    {
        binaryWriter.Close();
    }
}