/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Serilog;

namespace WebService.RadarLogic.Tracking.Applications;

public class TrafficMonitoring : ITrackingApplication 
{
    public const int FRAME_HEADER_SIZE = 52;
    public const ulong FRAME_HEADER_MAGIC = 0x708050603040102;
    public const int TLV_HEADER_SIZE = 8;
    public const int TRACK_OBJECT_TLV_SIZE = 112;
    public const int POINT_CLOUD_INFO_SIZE = 16;
    public const int POINT_CLOUD_SIDE_INFO_SIZE = 4;

    public const int TLV_TYPE_POINT_CLOUD = 6;
    public const int TLV_TYPE_TRACKS_LIST = 7;
    public const int TLV_TYPE_TARGETS_INDEX = 8;
    public const int TLV_TYPE_POINT_CLOUD_SIDE_INFO = 9;

    public class TrafficAppFrameData {
        public class FrameHeader 
        {
            public ulong MagicWord;
            public uint Version;
            public uint Platform;
            public uint TimeStamp;
            public uint TotalPacketLen;
            public uint FrameNumber;
            public uint SubFrameNumber;
            public uint ChirpProcessingMargin;
            public uint FrameProcessingMargin;
            public uint TrackingProcessingTime;
            public uint UARTSendingTime;
            public ushort NumTLVs;
            public ushort Checksum;

            public FrameHeader(byte [] headerBytes)
            {
                var reader = new BinaryReader(new MemoryStream(headerBytes));
                MagicWord = reader.ReadUInt64();
                Version = reader.ReadUInt32();
                Platform = reader.ReadUInt32();
                TimeStamp = reader.ReadUInt32();
                TotalPacketLen = reader.ReadUInt32();
                FrameNumber = reader.ReadUInt32();
                SubFrameNumber = reader.ReadUInt32();
                ChirpProcessingMargin = reader.ReadUInt32();
                FrameProcessingMargin = reader.ReadUInt32();
                TrackingProcessingTime = reader.ReadUInt32();
                UARTSendingTime = reader.ReadUInt32();
                NumTLVs = reader.ReadUInt16();
                Checksum = reader.ReadUInt16();
            }
        }

        public class Point
        {
            public float Range;
            public float Azimuth;
            public float Elevation;
            public float Doppler;
        }

        public class PointSideInfo
        {
            public short SNR;
            public short Noise;
        }

        public class Track
        {
            public uint TrackId;
            public float PositionX;
            public float PositionY;
            public float PositionZ;
            public float VelocityX;
            public float VelocityY;
            public float VelocityZ;
            public float AccelerationX;
            public float AccelerationY;
            public float AccelerationZ;
            public byte [] ErrorCovarianceMatrix = {};
            public float GatingGain;
            public float ConfidenceLevel;
        }

        public FrameHeader? frameHeader;
        public List<Point> pointCloudList = new List<Point>();
        public List<PointSideInfo> pointCloudSideInfo = new List<PointSideInfo>();
        public List<Track> tracksList = new List<Track>();
    }

    public TrafficAppFrameData ReadFrame(ITrackingApplication.ReadTIData readTIDataFunction)
    {
        TrafficAppFrameData frameData = new TrafficAppFrameData();

        while (true)
        {
            byte [] headerBytes = new byte[FRAME_HEADER_SIZE];
            
            var len = readTIDataFunction(headerBytes, headerBytes.Length);
            if (len != FRAME_HEADER_SIZE)
            {
                //Log.Error($"failed reading frame header. read len: {len}");
                continue;
            }

            frameData.frameHeader = new TrafficAppFrameData.FrameHeader(headerBytes);
            
            if (frameData.frameHeader.MagicWord != FRAME_HEADER_MAGIC)
            {
                Log.Error("invalid magic header");
                continue;
            }

            break;
        }

        Log.Verbose($"Platform: {frameData.frameHeader.Platform:X}");
        Log.Verbose($"Frame Number: {frameData.frameHeader.FrameNumber}");
        Log.Verbose($"numTLVs: {frameData.frameHeader.NumTLVs}");

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
            if (readTIDataFunction(tlvDataBytes, tlvDataBytes.Length) != tlvLength)
            {
                throw new Exception("failed reading TLV data!");
            }

            reader = new BinaryReader(new MemoryStream(tlvDataBytes));

            if (tlvType == TLV_TYPE_POINT_CLOUD)
            {
                var pointsCount = tlvLength / POINT_CLOUD_INFO_SIZE;
                for (int pointIndex = 0; pointIndex < pointsCount; pointIndex++)
                {
                    TrafficAppFrameData.Point point = new TrafficAppFrameData.Point();
                    point.Range = reader.ReadSingle();
                    point.Azimuth = reader.ReadSingle();
                    point.Elevation = reader.ReadSingle();
                    point.Doppler = reader.ReadSingle();
                    frameData.pointCloudList.Add(point);

                    Log.Verbose($"PointIndex: {pointIndex}, Range: {point.Range:0.00}, Azimuth: {point.Azimuth:0.00}, Elevation: {point.Elevation:0.00}, Doppler: {point.Doppler:0.00}");
                }
            }

            if (tlvType == TLV_TYPE_POINT_CLOUD_SIDE_INFO)
            {
                var pointsCount = tlvLength / POINT_CLOUD_SIDE_INFO_SIZE;
                for (int pointIndex = 0; pointIndex < pointsCount; pointIndex++)
                {
                    TrafficAppFrameData.PointSideInfo point = new TrafficAppFrameData.PointSideInfo();
                    point.SNR = reader.ReadInt16();
                    point.Noise = reader.ReadInt16();
                    frameData.pointCloudSideInfo.Add(point);

                    Log.Verbose($"PintSideInfo: {pointIndex}, SNR: {point.SNR}, Noise: {point.Noise}");
                }
            }

            if (tlvType == TLV_TYPE_TRACKS_LIST)
            {
                var tracksCount = tlvLength / TRACK_OBJECT_TLV_SIZE;
                for (int trackIndex = 0; trackIndex < tracksCount; trackIndex++)
                {
                    TrafficAppFrameData.Track track = new TrafficAppFrameData.Track();
                    track.TrackId = reader.ReadUInt32();
                    track.PositionX = reader.ReadSingle();
                    track.PositionY = reader.ReadSingle();
                    track.PositionZ = reader.ReadSingle();
                    track.VelocityX = reader.ReadSingle();
                    track.VelocityY = reader.ReadSingle();
                    track.VelocityZ = reader.ReadSingle();
                    track.AccelerationX = reader.ReadSingle();
                    track.AccelerationY = reader.ReadSingle();
                    track.AccelerationZ = reader.ReadSingle();
                    track.ErrorCovarianceMatrix = reader.ReadBytes(64);
                    track.GatingGain = reader.ReadSingle();
                    track.ConfidenceLevel = reader.ReadSingle();
                    frameData.tracksList.Add(track);

                    Log.Verbose($"Track ID: {track.TrackId}, posX: {track.PositionX:0.00}, posY: {track.PositionY:0.00}, posZ:{track.PositionZ:0.00}, velX:{track.VelocityX:0.00}, velY:{track.VelocityY:0.00},  velZ:{track.VelocityZ:0.00}");
                }
            }
        }

        return frameData;  
    }

    public FrameData GetNextFrame(ITrackingApplication.ReadTIData readTIDataFunction)
    {
        TrafficAppFrameData trafficAppFrameData = ReadFrame(readTIDataFunction);
        
        // convert to generic frame data
        FrameData outFrameData = new Tracking.FrameData();
        
        foreach (var point in trafficAppFrameData.pointCloudList)
        {
            var convertedPoint = new FrameData.Point {
                Azimuth = point.Azimuth,
                Elevation = point.Elevation,
                Range = point.Range,
                Doppler = point.Doppler
            };
            
            outFrameData.PointsList.Add(convertedPoint);
        }

        foreach (var track in trafficAppFrameData.tracksList)
        {
            var convertedTrack = new FrameData.Track {
                TrackId = track.TrackId,
                PositionX = track.PositionX,
                PositionY = track.PositionY,
                PositionZ = track.PositionZ,
                VelocityX = track.VelocityX,
                VelocityY = track.VelocityY,
                VelocityZ = track.VelocityZ,
                AccelerationX = track.AccelerationX,
                AccelerationY = track.AccelerationY,
                AccelerationZ = track.AccelerationZ
            };

            outFrameData.TracksList.Add(convertedTrack);
        }

        return outFrameData;
    }

}