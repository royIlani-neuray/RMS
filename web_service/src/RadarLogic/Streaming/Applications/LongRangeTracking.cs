/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using Serilog;
using WebService.Entites;

namespace WebService.RadarLogic.Streaming.Applications;

public class LongRangeTracking : IFirmwareApplication 
{
    public const int FRAME_HEADER_SIZE = 40;
    public const ulong FRAME_HEADER_MAGIC = 0x708050603040102;
    public const int TLV_HEADER_SIZE = 8;

    public const int TRACK_OBJECT_TLV_SIZE = 112;
    public const int POINT_CLOUD_INFO_SIZE = 16;
    public const int POINT_CLOUD_SIDE_INFO_SIZE = 4;

    public const int TLV_TYPE_POINT_CLOUD = 1000;
    public const int TLV_TYPE_TRACKS_LIST = 1010;
    public const int TLV_TYPE_TARGETS_INDEX = 1011;
    public const int TLV_TYPE_POINT_CLOUD_SIDE_INFO = 7;

    private RadarSettings.SensorPositionParams radarPosition;

    public class LongRangeTrackingFrameData {

        public class FrameHeader 
        {
            public ulong MagicWord;
            public uint Version;
            public uint TotalPacketLen;
            public uint Platform;
            public uint FrameNumber;
            public uint TimeCpuCycles;
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
                TimeCpuCycles = reader.ReadUInt32();
                NumDetectedObjects = reader.ReadUInt32();
                NumTLVs = reader.ReadUInt32();
                SubFrameNumber = reader.ReadUInt32();
            }
        }

        public class Point
        {
            public float Range; /* Range in meters */
            public float Azimuth; /* Azimuth angle in degrees in the range [-90,90] */
            public float Elevation; /* Elevation angle in degrees in the range [-90,90] */
            public float Doppler; /* Doppler velocity estimate in m/s */
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
        public List<PointSideInfo> pointsSideInfoList = new List<PointSideInfo>();
        public List<byte> targetsIndexList = new List<byte>();
        public List<Track> tracksList = new List<Track>();

    }

    public LongRangeTracking(RadarSettings.SensorPositionParams radarPosition)
    {
        this.radarPosition = radarPosition;
    }

    public LongRangeTrackingFrameData ReadFrame(IFirmwareApplication.ReadTIData readTIDataFunction)
    {
        LongRangeTrackingFrameData frameData = new LongRangeTrackingFrameData();

        while (true)
        {
            byte [] headerBytes = new byte[FRAME_HEADER_SIZE];
            
            var len = readTIDataFunction(headerBytes, headerBytes.Length);
            if (len != FRAME_HEADER_SIZE)
            {
                //Log.Error($"failed reading frame header. read len: {len}");
                continue;
            }

            frameData.frameHeader = new LongRangeTrackingFrameData.FrameHeader(headerBytes);
            
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

            Log.Verbose($"Tlv type: {tlvType}, size: {tlvLength}");

            byte [] tlvDataBytes = new byte[tlvLength];
            
            int bytesRead = readTIDataFunction(tlvDataBytes, tlvDataBytes.Length);
            if (bytesRead != tlvLength)
            {
                throw new Exception($"failed reading TLV data! expected: {tlvLength}, got {bytesRead} bytes");
            }

            reader = new BinaryReader(new MemoryStream(tlvDataBytes));

            if (tlvType == TLV_TYPE_POINT_CLOUD)
            {
                var pointsCount = tlvLength / POINT_CLOUD_INFO_SIZE;
                for (int pointIndex = 0; pointIndex < pointsCount; pointIndex++)
                {
                    LongRangeTrackingFrameData.Point point = new LongRangeTrackingFrameData.Point();
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
                var pointsCloudSideInfoCount = tlvLength / POINT_CLOUD_SIDE_INFO_SIZE;
                for (int pointIndex = 0; pointIndex < pointsCloudSideInfoCount; pointIndex++)
                {
                    LongRangeTrackingFrameData.PointSideInfo pointSideInfo = new LongRangeTrackingFrameData.PointSideInfo();
                    pointSideInfo.SNR = reader.ReadInt16();
                    pointSideInfo.Noise = reader.ReadInt16();
                    frameData.pointsSideInfoList.Add(pointSideInfo);
                }
            }

            if (tlvType == TLV_TYPE_TRACKS_LIST)
            {
                var tracksCount = tlvLength / TRACK_OBJECT_TLV_SIZE;

                for (int trackIndex = 0; trackIndex < tracksCount; trackIndex++)
                {
                    LongRangeTrackingFrameData.Track track = new LongRangeTrackingFrameData.Track();
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

                    Log.Verbose($"Track ID: {track.TrackId}, posX: {track.PositionX:0.00}, posY: {track.PositionY:0.00}, posZ: {track.PositionZ:0.00}, velX: {track.VelocityX:0.00}, velY: {track.VelocityY:0.00},  velZ: {track.VelocityZ:0.00}");
                }
            }

            if (tlvType == TLV_TYPE_TARGETS_INDEX)
            {
                for (int i=0; i<tlvLength; i++)
                {
                    frameData.targetsIndexList.Add(reader.ReadByte());
                }

                Log.Verbose($"Number of points: {frameData.targetsIndexList.Count}");
            }

        }

        return frameData;
    }

    public FrameData GetNextFrame(IFirmwareApplication.ReadTIData readTIDataFunction)
    {
        LongRangeTrackingFrameData frameData = ReadFrame(readTIDataFunction);
        
        // convert to generic frame data
        FrameData outFrameData = new Streaming.FrameData();
        outFrameData.FrameNumber = frameData.frameHeader!.FrameNumber;

        foreach (var point in frameData.pointCloudList)
        {
            var convertedPoint = new FrameData.Point {
                Azimuth = point.Azimuth, 
                Elevation = point.Elevation,
                Range = point.Range,
                Doppler = point.Doppler,

                /* Note: SNR is currently not provided (need to fuse from side info list) */
            };

            FWApplicationUtils.CalcCartesianFromSpherical(convertedPoint);

            float rotatedX, rotatedY, rotatedZ;
            FWApplicationUtils.RotatePoint(radarPosition.AzimuthTiltDegrees, radarPosition.ElevationTiltDegrees, convertedPoint.PositionX, convertedPoint.PositionY, convertedPoint.PositionZ, out rotatedX, out rotatedY, out rotatedZ);

            convertedPoint.PositionX = rotatedX;
            convertedPoint.PositionY = rotatedY;
            convertedPoint.PositionZ = rotatedZ + this.radarPosition.HeightMeters;
            
            outFrameData.PointsList.Add(convertedPoint);
        }

        foreach (var track in frameData.tracksList)
        {
            var convertedTrack = new FrameData.Track {
                TrackId = track.TrackId,
                VelocityX = track.VelocityX,
                VelocityY = track.VelocityY,
                VelocityZ = track.VelocityZ,
                AccelerationX = track.AccelerationX,
                AccelerationY = track.AccelerationY,
                AccelerationZ = track.AccelerationZ
            };

            // rotate the track according to radar azimuth and elevation, and add height

            float rotatedX, rotatedY, rotatedZ;
            FWApplicationUtils.RotatePoint(radarPosition.AzimuthTiltDegrees, radarPosition.ElevationTiltDegrees, track.PositionX, track.PositionY, track.PositionZ, out rotatedX, out rotatedY, out rotatedZ);

            convertedTrack.PositionX = rotatedX;
            convertedTrack.PositionY = rotatedY;
            convertedTrack.PositionZ = rotatedZ + this.radarPosition.HeightMeters;

            outFrameData.TracksList.Add(convertedTrack);
        }

        outFrameData.TargetsIndexList = frameData.targetsIndexList;

        return outFrameData;
    }

}