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

public class PeopleTracking : IFirmwareApplication 
{
    public const int FRAME_HEADER_SIZE = 40;
    public const ulong FRAME_HEADER_MAGIC = 0x708050603040102;
    public const int TLV_HEADER_SIZE = 8;

    public const int TRACK_OBJECT_TLV_SIZE = 112;
    public const int POINT_CLOUD_UNIT_SIZE = 20;
    public const int POINT_CLOUD_INFO_SIZE = 8;
    public const int TARGET_HEIGHT_INFO_SIZE = 12;
    public const int VITAL_SIGNS_CIRCULAR_BUFFER_SIZE = 15;

    public const int TLV_TYPE_POINT_CLOUD = 1020;
    public const int TLV_TYPE_TRACKS_LIST = 1010;
    public const int TLV_TYPE_TARGETS_INDEX = 1011;
    public const int TLV_TYPE_TARGETS_HEIGHT = 1012;
    public const int TLV_TYPE_PRESENCE_INDICATION = 1021;
    public const int TLV_TYPE_VITAL_SIGNS = 1040;

    private RadarSettings.SensorPositionParams radarPosition;

    public class PeopleTrackingFrameData {

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
            public float Elevation;
            public float Azimuth;
            public float Doppler;
            public float Range;
            public float SNR;
        }

        public class PointUnit
        {
            public float ElevationUnit;
            public float AzimuthUnit;
            public float DopplerUnit;
            public float RangeUnit;
            public float SnrUnit;
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

        public class TargetHeight
        {
            public uint TargetId;
            public float MaxZ;
            public float MinZ;
        }

        public class VitalSignsInfo
        {
            public uint TargetId;                             // Target ID used for XYZ location
            public uint RangeBin;                             // range bin for XYZ location
            public float BreathingDeviation;                  // deviation of breathing measurement over time
            public float HeartRate;                           // Heart Rate Measurement
            public float BreathingRate;                       // Breath Rate Measurement
            public List<float> HeartCircularBuffer = new();   // Buffer of heartrate waveform
            public List<float> BreathCircularBuffer = new();  // Buffer of breathrate waveform
        }

        public FrameHeader? frameHeader;
        public List<Point> pointCloudList = new();
        public List<TargetHeight> targetsHeightList = new();
        public List<byte> targetsIndexList = new();
        public List<Track> tracksList = new();
        public VitalSignsInfo? vitalSigns;

    }

    public PeopleTracking(RadarSettings.SensorPositionParams radarPosition)
    {
        this.radarPosition = radarPosition;
    }

    public PeopleTrackingFrameData ReadFrame(IFirmwareApplication.ReadTIData readTIDataFunction)
    {
        PeopleTrackingFrameData frameData = new PeopleTrackingFrameData();

        while (true)
        {
            byte [] headerBytes = new byte[FRAME_HEADER_SIZE];
            
            var len = readTIDataFunction(headerBytes, headerBytes.Length);
            if (len != FRAME_HEADER_SIZE)
            {
                //Log.Error($"failed reading frame header. read len: {len}");
                continue;
            }

            frameData.frameHeader = new PeopleTrackingFrameData.FrameHeader(headerBytes);
            
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
            
            int bytesRead = readTIDataFunction(tlvDataBytes, tlvDataBytes.Length);
            if (bytesRead != tlvLength)
            {
                throw new Exception($"failed reading TLV data! expected: {tlvLength}, got {bytesRead} bytes");
            }

            reader = new BinaryReader(new MemoryStream(tlvDataBytes));

            if (tlvType == TLV_TYPE_POINT_CLOUD)
            {
                // get point unit data first
                PeopleTrackingFrameData.PointUnit pointUnit = new PeopleTrackingFrameData.PointUnit();
                pointUnit.ElevationUnit = reader.ReadSingle();
                pointUnit.AzimuthUnit = reader.ReadSingle();
                pointUnit.DopplerUnit = reader.ReadSingle();
                pointUnit.RangeUnit = reader.ReadSingle();
                pointUnit.SnrUnit = reader.ReadSingle();

                var pointsCount = (tlvLength - POINT_CLOUD_UNIT_SIZE) / POINT_CLOUD_INFO_SIZE;
                for (int pointIndex = 0; pointIndex < pointsCount; pointIndex++)
                {
                    PeopleTrackingFrameData.Point point = new PeopleTrackingFrameData.Point();
                    point.Elevation = reader.ReadSByte() * pointUnit.ElevationUnit;
                    point.Azimuth = reader.ReadSByte() * pointUnit.AzimuthUnit;
                    point.Doppler = reader.ReadInt16() * pointUnit.DopplerUnit;
                    point.Range = reader.ReadUInt16() * pointUnit.RangeUnit;
                    point.SNR = reader.ReadUInt16() * pointUnit.SnrUnit;
                    frameData.pointCloudList.Add(point);

                    Log.Verbose($"PointIndex: {pointIndex}, Range: {point.Range:0.00}, Azimuth: {point.Azimuth:0.00}, Elevation: {point.Elevation:0.00}, Doppler: {point.Doppler:0.00}");
                }
            }

            if (tlvType == TLV_TYPE_TRACKS_LIST)
            {
                var tracksCount = tlvLength / TRACK_OBJECT_TLV_SIZE;

                for (int trackIndex = 0; trackIndex < tracksCount; trackIndex++)
                {
                    PeopleTrackingFrameData.Track track = new PeopleTrackingFrameData.Track();
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

            if (tlvType == TLV_TYPE_TARGETS_HEIGHT)
            {
                var targetsCount = tlvLength / TARGET_HEIGHT_INFO_SIZE;
                for (int targetIndex = 0; targetIndex < targetsCount; targetIndex++)
                {
                    PeopleTrackingFrameData.TargetHeight target = new PeopleTrackingFrameData.TargetHeight();
                    target.TargetId = reader.ReadUInt32();
                    target.MaxZ = reader.ReadSingle();
                    target.MinZ = reader.ReadSingle();
                    frameData.targetsHeightList.Add(target);
                    
                    Log.Verbose($"Target Height: Track-{target.TargetId}, Max-Z: {target.MaxZ:0.00}, Min-Z: {target.MinZ:0.00}");
                }
            }

            if (tlvType == TLV_TYPE_VITAL_SIGNS)
            {
                frameData.vitalSigns = new PeopleTrackingFrameData.VitalSignsInfo
                {
                    TargetId = reader.ReadUInt16(),
                    RangeBin = reader.ReadUInt16(),
                    BreathingDeviation = reader.ReadSingle(),
                    HeartRate = reader.ReadSingle(),
                    BreathingRate = reader.ReadSingle()
                };

                for (int i=0; i<VITAL_SIGNS_CIRCULAR_BUFFER_SIZE; i++)
                {
                    frameData.vitalSigns.HeartCircularBuffer.Add(reader.ReadSingle());
                }

                for (int i=0; i<VITAL_SIGNS_CIRCULAR_BUFFER_SIZE; i++)
                {
                    frameData.vitalSigns.BreathCircularBuffer.Add(reader.ReadSingle());
                }

                Log.Verbose($"Vital Signs: Track-{frameData.vitalSigns.TargetId}, Heart Rate: {frameData.vitalSigns.HeartRate:0.00}, Breathing Rate: {frameData.vitalSigns.BreathingRate:0.00}");
            }
        }

        return frameData;
    }

    public FrameData GetNextFrame(IFirmwareApplication.ReadTIData readTIDataFunction)
    {
        PeopleTrackingFrameData frameData = ReadFrame(readTIDataFunction);
        
        // convert to generic frame data
        FrameData outFrameData = new();
        outFrameData.FrameNumber = frameData.frameHeader!.FrameNumber;

        foreach (var point in frameData.pointCloudList)
        {
            var convertedPoint = new FrameData.Point {
                Azimuth = point.Azimuth,
                Elevation = point.Elevation,
                Range = point.Range,
                Doppler = point.Doppler,
                SNR = point.SNR
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

        outFrameData.TargetsHeightList = frameData.targetsHeightList.ConvertAll(target => new FrameData.TargetHeight() {
            TargetId = target.TargetId,
            MaxZ = target.MaxZ,
            MinZ = target.MinZ
        });

        if (frameData.vitalSigns != null)
        {
            outFrameData.VitalSigns = new FrameData.VitalSignsInfo()
            {
                TargetId = frameData.vitalSigns.TargetId,
                RangeBin = frameData.vitalSigns.RangeBin,
                BreathingDeviation = frameData.vitalSigns.BreathingDeviation,
                HeartRate = frameData.vitalSigns.HeartRate,
                BreathingRate = frameData.vitalSigns.BreathingRate,
                HeartCircularBuffer = frameData.vitalSigns.HeartCircularBuffer,
                BreathCircularBuffer = frameData.vitalSigns.BreathCircularBuffer
            };
        }

        return outFrameData;
    }

}