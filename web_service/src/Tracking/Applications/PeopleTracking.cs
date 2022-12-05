/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;

namespace WebService.Tracking.Applications;

public class PeopleTracking : ITrackingApplication 
{
    public const int FRAME_HEADER_SIZE = 40;
    public const ulong FRAME_HEADER_MAGIC = 0x708050603040102;
    public const int TLV_HEADER_SIZE = 8;

    public const int TRACK_OBJECT_TLV_SIZE = 112;
    public const int POINT_CLOUD_UNIT_SIZE = 20;
    public const int POINT_CLOUD_INFO_SIZE = 8;

    public const int TLV_TYPE_POINT_CLOUD = 1020;
    public const int TLV_TYPE_TRACKS_LIST = 1010;
    public const int TLV_TYPE_TARGETS_INDEX = 1011;
    public const int TLV_TYPE_PRESENCE_INDICATION = 1021;

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

        public FrameHeader? frameHeader;
        public List<Point> pointCloudList = new List<Point>();
        public List<byte> targetsIndexList = new List<byte>();
        public List<Track> tracksList = new List<Track>();

    }

    public PeopleTracking(RadarSettings.SensorPositionParams radarPosition)
    {
        this.radarPosition = radarPosition;
    }

    public PeopleTrackingFrameData ReadFrame(ITrackingApplication.ReadTIData readTIDataFunction)
    {
        PeopleTrackingFrameData frameData = new PeopleTrackingFrameData();

        while (true)
        {
            byte [] headerBytes = new byte[FRAME_HEADER_SIZE];
            
            var len = readTIDataFunction(headerBytes, headerBytes.Length);
            if (len != FRAME_HEADER_SIZE)
            {
                //Console.WriteLine($"failed reading frame header. read len: {len}");
                continue;
            }

            frameData.frameHeader = new PeopleTrackingFrameData.FrameHeader(headerBytes);
            
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
            if (readTIDataFunction(tlvDataBytes, tlvDataBytes.Length) != tlvLength)
            {
                throw new Exception("failed reading TLV data!");
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

                    // Console.WriteLine($"PointIndex: {pointIndex}, Range: {point.Range:0.00}, Azimuth: {point.Azimuth:0.00}, Elevation: {point.Elevation:0.00}, Doppler: {point.Doppler:0.00}");
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

                    // Console.WriteLine($"Track ID: {track.TrackId}, posX: {track.PositionX:0.00}, posY: {track.PositionY:0.00}, posZ: {track.PositionZ:0.00}, velX: {track.VelocityX:0.00}, velY: {track.VelocityY:0.00},  velZ: {track.VelocityZ:0.00}");
                }
            }

            if (tlvType == TLV_TYPE_TARGETS_INDEX)
            {
                for (int i=0; i<tlvLength; i++)
                {
                    frameData.targetsIndexList.Add(reader.ReadByte());
                }

                // Console.WriteLine($"Number of points: {frameData.targetsIndexList.Count}");
            }

        }

        return frameData;
    }

    public FrameData GetNextFrame(ITrackingApplication.ReadTIData readTIDataFunction)
    {
        PeopleTrackingFrameData frameData = ReadFrame(readTIDataFunction);
        
        // convert to generic frame data
        FrameData outFrameData = new Tracking.FrameData();

        foreach (var point in frameData.pointCloudList)
        {
            var convertedPoint = new FrameData.Point {
                Azimuth = point.Azimuth,
                Elevation = point.Elevation,
                Range = point.Range,
                Doppler = point.Doppler
            };

            CalcCartesianFromSpherical(convertedPoint);

            float rotatedX, rotatedY, rotatedZ;
            RotatePoint(convertedPoint.PositionX, convertedPoint.PositionY, convertedPoint.PositionZ, out rotatedX, out rotatedY, out rotatedZ);

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
            RotatePoint(track.PositionX, track.PositionY, track.PositionZ, out rotatedX, out rotatedY, out rotatedZ);

            convertedTrack.PositionX = rotatedX;
            convertedTrack.PositionY = rotatedY;
            convertedTrack.PositionZ = rotatedZ + this.radarPosition.HeightMeters;

            outFrameData.tracksList.Add(convertedTrack);
        }

        outFrameData.TargetsIndexList = frameData.targetsIndexList;

        return outFrameData;
    }

    private void CalcCartesianFromSpherical(FrameData.Point point)
    {
        point.PositionX = (float) (point.Range * Math.Sin(point.Azimuth) * Math.Cos(point.Elevation));
        point.PositionY = (float) (point.Range * Math.Cos(point.Azimuth) * Math.Cos(point.Elevation));
        point.PositionZ = (float) (point.Range * Math.Sin(point.Elevation));
    }

    private void RotatePoint(float x, float y, float z, out float rotatedX, out float rotatedY, out float rotatedZ)
    {
        var azimuthDegrees = this.radarPosition.AzimuthTiltDegrees;
        var elevationDegrees = this.radarPosition.ElevationTiltDegrees;

        double azimuth = (Math.PI / 180) * azimuthDegrees; 
        double elevation = (Math.PI / 180) * elevationDegrees; 

        double[,] rotationMatrix = new double[3, 3] { { Math.Cos(azimuth), Math.Cos(elevation) * Math.Sin(azimuth), Math.Sin(elevation) * Math.Sin(azimuth) }, 
                                                      { -Math.Sin(azimuth), Math.Cos(elevation) * Math.Cos(azimuth), Math.Sin(elevation) * Math.Cos(azimuth) }, 
                                                      { 0, -Math.Sin(elevation),  Math.Cos(elevation) } };
        
        rotatedX = (float) (x * rotationMatrix[0,0] + y * rotationMatrix[0,1] + z * rotationMatrix[0,2]);
        rotatedY = (float) (x * rotationMatrix[1,0] + y * rotationMatrix[1,1] + z * rotationMatrix[1,2]);
        rotatedZ = (float) (x * rotationMatrix[2,0] + y * rotationMatrix[2,1] + z * rotationMatrix[2,2]);

        // System.Console.WriteLine($"Point XYZ = [{x}, {y}, {z}], Rotation: [{rotatedX}, {rotatedY}, {rotatedZ}]");   
    }

}