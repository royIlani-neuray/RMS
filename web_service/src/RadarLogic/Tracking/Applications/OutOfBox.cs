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

public class OutOfBox : ITrackingApplication 
{
    public const int FRAME_HEADER_SIZE = 40;
    public const ulong FRAME_HEADER_MAGIC = 0x708050603040102;
    public const int TLV_HEADER_SIZE = 8;

    public const int POINT_CLOUD_INFO_SIZE = 16;
    public const int POINT_CLOUD_SIDE_INFO_SIZE = 4;


    public const int TLV_TYPE_DETECTED_POINTS = 1;
    public const int TLV_TYPE_RANGE_PROFILE = 2;
    public const int TLV_TYPE_NOISE_PROFILE = 3;
    public const int TLV_TYPE_AZIMUT_STATIC_HEAT_MAP = 4;
    public const int TLV_TYPE_RANGE_DOPPLER_HEAT_MAP = 5;
    public const int TLV_TYPE_STATS = 6;
    public const int TLV_TYPE_DETECTED_POINTS_SIDE_INFO = 7;
    public const int TLV_TYPE_AZIMUT_ELEVATION_STATIC_HEAT_MAP = 8;
    public const int TLV_TYPE_TEMPERATURE_STATS = 9;

    private RadarSettings.SensorPositionParams radarPosition;

    public class OutOfBoxFrameData {
    
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
            public uint SubFrameNumber;

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
                SubFrameNumber = reader.ReadUInt32();
            }
        }

        public class Point
        {
            public float PositionX;
            public float PositionY;
            public float PositionZ;
            public float Velocity;
        }

        public class PointSideInfo
        {
            public short SNR;
            public short Noise;
        }

        public FrameHeader? frameHeader;
        public List<Point> pointCloudList = new List<Point>();
        public List<PointSideInfo> pointsSideInfoList = new List<PointSideInfo>();
    }

    public OutOfBox(RadarSettings.SensorPositionParams radarPosition)
    {
        this.radarPosition = radarPosition;
    }

    public OutOfBoxFrameData ReadFrame(ITrackingApplication.ReadTIData readTIDataFunction)
    {
        var frameData = new OutOfBoxFrameData();

        while (true)
        {
            byte [] headerBytes = new byte[FRAME_HEADER_SIZE];
            
            var len = readTIDataFunction(headerBytes, headerBytes.Length);
            if (len != FRAME_HEADER_SIZE)
            {
                //Console.WriteLine($"failed reading frame header. read len: {len}");
                continue;
            }

            frameData.frameHeader = new OutOfBoxFrameData.FrameHeader(headerBytes);
            
            if (frameData.frameHeader.MagicWord != FRAME_HEADER_MAGIC)
            {
                Console.WriteLine("invalid magic header");
                continue;
            }

            break;
        }

        //Console.WriteLine($"Platform: {frameData.frameHeader.Platform:X}");
        //Console.WriteLine($"Frame Number: {frameData.frameHeader.FrameNumber}");
        //Console.WriteLine($"numTLVs: {frameData.frameHeader.NumTLVs}");

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

            //System.Console.WriteLine($"Tlv type: {tlvType}, size: {tlvLength}");

            byte [] tlvDataBytes = new byte[tlvLength];
            
            int bytesRead = readTIDataFunction(tlvDataBytes, tlvDataBytes.Length);
            if (bytesRead != tlvLength)
            {
                throw new Exception($"failed reading TLV data! expected: {tlvLength}, got {bytesRead} bytes");
            }

            reader = new BinaryReader(new MemoryStream(tlvDataBytes));

            if (tlvType == TLV_TYPE_DETECTED_POINTS)
            {
                var pointsCount = tlvLength / POINT_CLOUD_INFO_SIZE;
                for (int pointIndex = 0; pointIndex < pointsCount; pointIndex++)
                {
                    OutOfBoxFrameData.Point point = new OutOfBoxFrameData.Point();
                    point.PositionX = reader.ReadSingle();
                    point.PositionY = reader.ReadSingle();
                    point.PositionZ = reader.ReadSingle();
                    point.Velocity = reader.ReadSingle();
                    frameData.pointCloudList.Add(point);

                    //Console.WriteLine($"PointIndex: {pointIndex}, X: {point.PositionX:0.00}, Y: {point.PositionY:0.00}, Z: {point.PositionZ:0.00}, Doppler: {point.Velocity:0.00}");
                }
            }

            if (tlvType == TLV_TYPE_DETECTED_POINTS_SIDE_INFO)
            {
                var pointsCloudSideInfoCount = tlvLength / POINT_CLOUD_SIDE_INFO_SIZE;
                for (int pointIndex = 0; pointIndex < pointsCloudSideInfoCount; pointIndex++)
                {
                    OutOfBoxFrameData.PointSideInfo pointSideInfo = new OutOfBoxFrameData.PointSideInfo();
                    pointSideInfo.SNR = reader.ReadInt16();
                    pointSideInfo.Noise = reader.ReadInt16();
                    frameData.pointsSideInfoList.Add(pointSideInfo);
                }
            }

        }

        return frameData;
    }

    public FrameData GetNextFrame(ITrackingApplication.ReadTIData readTIDataFunction)
    {
        OutOfBoxFrameData frameData = ReadFrame(readTIDataFunction);
        
        // convert to generic frame data
        FrameData outFrameData = new Tracking.FrameData();
        outFrameData.FrameNumber = frameData.frameHeader!.FrameNumber;

        foreach (var point in frameData.pointCloudList)
        {
            var convertedPoint = new FrameData.Point {
                Doppler = point.Velocity,
                PositionX = point.PositionX,
                PositionY = point.PositionY,
                PositionZ = point.PositionZ + this.radarPosition.HeightMeters

                /* TODO: need to calculate and include angles */

                /* Note: SNR is currently not provided (need to fuse from side info list) */
            };

            outFrameData.PointsList.Add(convertedPoint);
        }

        return outFrameData;
    }

}