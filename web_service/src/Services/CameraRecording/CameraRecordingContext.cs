/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using RtspClientSharpCore.RawFrames;
using RtspRawVideo = RtspClientSharpCore.RawFrames.Video;

using WebService.Utils;

namespace WebService.Services.RadarRecording;

public class CameraRecordingContext : WorkerThread<RawFrame>, IServiceContext
{
    public IServiceContext.ServiceState State { get; set; }


    private Stream stream;
    private BinaryWriter binaryWriter;
    private bool gotFirstIFrame;

    private const int MAX_QUEUE_CAPACITY = 20;
    
    public CameraRecordingContext(string recordingPath) : base("CameraRecordingContext", MAX_QUEUE_CAPACITY)
    {
        State = IServiceContext.ServiceState.Initialized;
        this.gotFirstIFrame = false;
        stream = new FileStream(recordingPath, FileMode.Create);
        binaryWriter = new BinaryWriter(stream);
    }

    public void RecordFrame(RawFrame frame)
    {
        Enqueue(frame);
    }

    protected override Task DoWork(RawFrame workItem)
    {
        if (workItem is RtspRawVideo.RawH264IFrame iFrame)
        {
            gotFirstIFrame = true;
            binaryWriter.Write(iFrame.SpsPpsSegment);
            binaryWriter.Write(iFrame.FrameSegment);
            binaryWriter.Flush();

        }
        else if (workItem is RtspRawVideo.RawH264PFrame pFrame)
        {
            if (!gotFirstIFrame)
                return Task.CompletedTask;

            binaryWriter.Write(pFrame.FrameSegment);
            binaryWriter.Flush();
        }

        return Task.CompletedTask;
    }

    ~CameraRecordingContext()
    {
        binaryWriter.Close();
    }
}