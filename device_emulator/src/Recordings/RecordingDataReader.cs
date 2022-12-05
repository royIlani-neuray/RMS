/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.IO;
using System.Text.Json;
using System.Text.Encodings;

namespace DeviceEmulator.Recordings;

public class RecordingDataReader 
{
    private Stream stream;
    private BinaryReader binaryReader;

    private float frameRate;

    public float FrameRate { get => frameRate; }
    
    public RecordingDataReader(string dataFilePath)
    {
        stream = new FileStream(dataFilePath, FileMode.Open);
        binaryReader = new BinaryReader(stream);

        frameRate = binaryReader.ReadSingle();

        System.Console.WriteLine($"Recording frame rate: {frameRate} fps");
    }

    public bool GetNextFrame(out byte[]? frameBytes)
    {
        frameBytes = null;

        try
        {
            UInt32 frameDataSize = binaryReader.ReadUInt32();

            // frameDataSize == 0 ==> means an empty frame

            if (frameDataSize > 0)
            {
                frameBytes = binaryReader.ReadBytes((int) frameDataSize); 
            }

            return true;
        }
        catch (EndOfStreamException)
        {
            return false;
        }
    }

    public void Rewind()
    {
        stream.Seek(4, SeekOrigin.Begin);
    } 
}