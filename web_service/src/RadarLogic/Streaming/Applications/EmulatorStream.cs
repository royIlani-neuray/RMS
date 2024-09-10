/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json;
using Serilog;

namespace WebService.RadarLogic.Streaming.Applications;

public class EmulatorStream : IFirmwareApplication
{
    private string deviceId;
    private string deviceName;
    private uint frameNumber;

    public EmulatorStream(string deviceName, string deviceId)
    {
        this.deviceId = deviceId;
        this.deviceName = deviceName;
        frameNumber = 0;
    }

    public FrameData GetNextFrame(IFirmwareApplication.ReadTIData readTIDataFunction)
    {
        FrameData frame;
        byte [] frameSizeBytes = new byte[4];
            
        var len = readTIDataFunction(frameSizeBytes, frameSizeBytes.Length);

        if (len != sizeof(uint))
        {
            Log.Error("Failed to get frame size from emulator stream.");
            throw new Exception("Error: Failed to get frame size from emulator stream.");
        }

        uint frameSize = BitConverter.ToUInt32(frameSizeBytes);

        if (frameSize == 0)
        {
            // the emulator is not streaming a recording yet. generate an empty frame
            frame = new FrameData();
        }
        else
        {
            byte [] frameBytes = new byte[frameSize];

            len = readTIDataFunction(frameBytes, frameBytes.Length);
            
            if (len != frameSize)
            {
                Log.Error("Failed to get frame data from emulator stream.");
                throw new Exception("Error: Failed to get frame data from emulator stream.");
            }

            string jsonString = System.Text.Encoding.UTF8.GetString(frameBytes);
            frame = JsonSerializer.Deserialize<FrameData>(jsonString)!;
        }

        // override the original device details
        frame.DeviceId = deviceId;
        frame.DeviceName = deviceName;
        frame.Timestamp = DateTime.UtcNow;
        frame.FrameNumber = frameNumber;
        frameNumber++;

        return frame;
    }
}