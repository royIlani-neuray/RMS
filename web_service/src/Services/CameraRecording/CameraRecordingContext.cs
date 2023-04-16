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

    private const int MAX_QUEUE_CAPACITY = 200;
    
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

    private bool WriteFrameData(RawFrame frame)
    {
        if (frame is RtspRawVideo.RawH264IFrame iFrame)
        {
            if (!gotFirstIFrame)
            {
                gotFirstIFrame = true;
                frameCounter = 1;
            }

            frameBinaryWriter.Write(iFrame.SpsPpsSegment);
        }
        else if (!gotFirstIFrame)
        {
            return false;  
        }

        frameBinaryWriter.Write(frame.FrameSegment);
        frameBinaryWriter.Flush();
        return true;
    }

    private async Task WriteTimeStampAsync(RawFrame frame)
    {
        int frameSizeBytes = frame.FrameSegment.Count;
        byte isKeyFrame = 0;

        if (frame is RtspRawVideo.RawH264IFrame iFrame)
        {
            frameSizeBytes += iFrame.SpsPpsSegment.Count;
            isKeyFrame = 1;
        }

        if (frameCounter == 1)
        {
            await timestampWriter.WriteLineAsync("frame,datetime,frame_size_bytes,key_frame");
        }

        await timestampWriter.WriteLineAsync($"{frameCounter},{frame.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture)},{frameSizeBytes},{isKeyFrame}");
        await timestampWriter.FlushAsync();
    }

    protected override async Task DoWork(RawFrame workItem)
    {
        frameCounter++;
        
        if (WriteFrameData(workItem))
        {
            await WriteTimeStampAsync(workItem);
        }
    }

    ~CameraRecordingContext()
    {
        frameBinaryWriter.Close();
        timestampWriter.Close();
    }
}