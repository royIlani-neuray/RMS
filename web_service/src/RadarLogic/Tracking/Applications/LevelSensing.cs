/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;

namespace WebService.RadarLogic.Tracking.Applications;

public class LevelSensing : ITrackingApplication 
{
    public const int FRAME_HEADER_SIZE = 36;
    public const ulong FRAME_HEADER_MAGIC = 0x708050603040102;
    public const int TLV_HEADER_SIZE = 8;
    public const int TLV_TYPE_DETECTED_POINTS = 1;
    public const int TLV_TYPE_RANGE_PROFILE = 2;
    public const float Q_VALUE = 1048576f;  // 2 ^ 20

    public class LevelSensingFrameData {

        public class FrameHeader 
        {
            public ulong MagicWord;
            public uint Version;
            public uint TotalPacketLen;
            public uint Platform;
            public uint FrameNumber;
            public uint TimeStamp;
            public uint NumDetectedObjects;
            public uint NumTLVs;

            public FrameHeader(byte [] headerBytes)
            {
                var reader = new BinaryReader(new MemoryStream(headerBytes));
                MagicWord = reader.ReadUInt64();
                Version = reader.ReadUInt32();
                TotalPacketLen = reader.ReadUInt32();
                Platform = reader.ReadUInt32();
                FrameNumber = reader.ReadUInt32();
                TimeStamp = reader.ReadUInt32();
                NumDetectedObjects = reader.ReadUInt32();
                NumTLVs = reader.ReadUInt32();
            }
        }

        public class DetectedObject
        {
            public float peak1;
            public float peak2;
            public float peak3;
        }

        public FrameHeader? frameHeader;
        public DetectedObject? detectedObject;
        public List<float> fft1Dinput = new();

    }

    public LevelSensingFrameData ReadFrame(ITrackingApplication.ReadTIData readTIDataFunction)
    {
        LevelSensingFrameData frameData = new();

        while (true)
        {
            byte [] headerBytes = new byte[FRAME_HEADER_SIZE];
            
            var len = readTIDataFunction(headerBytes, headerBytes.Length);
            if (len != FRAME_HEADER_SIZE)
            {
                //Console.WriteLine($"failed reading frame header. read len: {len}");
                continue;
            }

            frameData.frameHeader = new LevelSensingFrameData.FrameHeader(headerBytes);
            
            if (frameData.frameHeader.MagicWord != FRAME_HEADER_MAGIC)
            {
                Console.WriteLine("invalid magic header");
                continue;
            }

            break;
        }

        // Console.WriteLine($"Platform: {frameData.frameHeader.Platform:X}");
        // Console.WriteLine($"Frame Number: {frameData.frameHeader.FrameNumber}");
        // Console.WriteLine($"numTLVs: {frameData.frameHeader.NumTLVs}");

        for (int tlvIndex = 0; tlvIndex < frameData.frameHeader.NumTLVs; tlvIndex++)
        {
            byte [] tlvInfoBytes = new byte[TLV_HEADER_SIZE];
            if (readTIDataFunction(tlvInfoBytes, tlvInfoBytes.Length) != TLV_HEADER_SIZE)
            {
                throw new Exception("failed reading TLV info");
            }

            var reader = new BinaryReader(new MemoryStream(tlvInfoBytes));
            var tlvType = reader.ReadUInt32();
            var tlvLength = reader.ReadUInt32();

            byte [] tlvDataBytes = new byte[tlvLength];
            
            int bytesRead = readTIDataFunction(tlvDataBytes, tlvDataBytes.Length);
            if (bytesRead != tlvLength)
            {
                throw new Exception($"failed reading TLV data! expected: {tlvLength}, got {bytesRead} bytes");
            }

            reader = new BinaryReader(new MemoryStream(tlvDataBytes));

            if (tlvType == TLV_TYPE_DETECTED_POINTS)
            {
                ushort rangeIdx = reader.ReadUInt16();
                short dopplerIdx = reader.ReadInt16();
                ushort peakVal = reader.ReadUInt16();
                short x = reader.ReadInt16();
                short y = reader.ReadInt16();
                short z = reader.ReadInt16();

                frameData.detectedObject = new()
                {
                    peak1 = ((x << 16) + rangeIdx) / Q_VALUE,
                    peak2 = ((y << 16) + peakVal) / Q_VALUE,
                    peak3 = ((z << 16) + (ushort)dopplerIdx) / Q_VALUE
                };
            }

            if (tlvType == TLV_TYPE_RANGE_PROFILE)
            {
                var fftSize = tlvLength / (2*sizeof(float));
                for (int i=0; i<fftSize; i++)
                {
                    frameData.fft1Dinput.Add(reader.ReadSingle());
                }
            }
        }

        return frameData;
    }

    public FrameData GetNextFrame(ITrackingApplication.ReadTIData readTIDataFunction)
    {
        LevelSensingFrameData frameData = ReadFrame(readTIDataFunction);
        
        // convert to generic frame data
        FrameData outFrameData = new();
        if (frameData.detectedObject != null)
        {
            outFrameData.TracksList.Add(new()
            {
                TrackId = 0,
                PositionX = frameData.detectedObject.peak1,
                PositionY = frameData.detectedObject.peak2,
                PositionZ = frameData.detectedObject.peak3,
            });
        }

        return outFrameData;
    }
}