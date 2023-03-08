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
using System.Globalization;

namespace WebService.Services.RadarRecording;

public class CameraRecordingContext : WorkerThread<RawFrame>, IServiceContext
{
    public IServiceContext.ServiceState State { get; set; }


    private Stream stream;
    private BinaryWriter frameBinaryWriter;
    private StreamWriter timestampWriter;
    private uint frameCounter;
    private bool gotFirstIFrame;

    private const int MAX_QUEUE_CAPACITY = 20;
    
    public CameraRecordingContext(string recordingVideoPath, string recordingTimestampPath) : base("CameraRecordingContext", MAX_QUEUE_CAPACITY)
    {
        State = IServiceContext.ServiceState.Initialized;
        this.gotFirstIFrame = false;
        stream = new FileStream(recordingVideoPath, FileMode.Create);
        frameBinaryWriter = new BinaryWriter(stream);
        timestampWriter = new StreamWriter(recordingTimestampPath);
        frameCounter = 0;
    }

    public void RecordFrame(RawFrame frame)
    {
        Enqueue(frame);
    }

    private void WriteFrameData(RawFrame frame)
    {
        if (frame is RtspRawVideo.RawH264IFrame iFrame)
        {
            gotFirstIFrame = true;
            frameBinaryWriter.Write(iFrame.SpsPpsSegment);
            frameBinaryWriter.Write(iFrame.FrameSegment);
        }
        else if (frame is RtspRawVideo.RawH264PFrame pFrame)
        {
            if (!gotFirstIFrame)
                return;

            frameBinaryWriter.Write(pFrame.FrameSegment);
        }

        frameBinaryWriter.Flush();
    }

    private async Task WriteTimeStampAsync(DateTime timestamp)
    {
        if (frameCounter == 1)
        {
            await timestampWriter.WriteLineAsync("frame,datetime");
        }

        await timestampWriter.WriteLineAsync($"{frameCounter},{timestamp.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture)}");
        await timestampWriter.FlushAsync();
    }

    protected override async Task DoWork(RawFrame workItem)
    {
        frameCounter++;
        WriteFrameData(workItem);
        await WriteTimeStampAsync(workItem.Timestamp);
    }

    ~CameraRecordingContext()
    {
        frameBinaryWriter.Close();
        timestampWriter.Close();
    }
}