
namespace WebService.Tracking.Applications;

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

    public class FrameData {
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

    public void GetNextFrame(ITrackingApplication.ReadTIData readTIDataFunction)
    {
        FrameData frameData = new FrameData();
        byte [] headerBytes = new byte[FRAME_HEADER_SIZE];
        var len = readTIDataFunction(headerBytes, headerBytes.Length);
        if (len != FRAME_HEADER_SIZE)
        {
            Console.WriteLine($"failed reading frame header. read len: {len}");
            return;
        }

        frameData.frameHeader = new FrameData.FrameHeader(headerBytes);
        
        if (frameData.frameHeader.MagicWord != FRAME_HEADER_MAGIC)
        {
            Console.WriteLine("invalid magic header");
            return;
        }

        Console.WriteLine($"Platform: {frameData.frameHeader.Platform:X}");
        Console.WriteLine($"Frame Number: {frameData.frameHeader.FrameNumber}");
        Console.WriteLine($"numTLVs: {frameData.frameHeader.NumTLVs}");

        for (int tlvIndex = 0; tlvIndex < frameData.frameHeader.NumTLVs; tlvIndex++)
        {
            byte [] tlvInfoBytes = new byte[TLV_HEADER_SIZE];
            if (readTIDataFunction(tlvInfoBytes, tlvInfoBytes.Length) != TLV_HEADER_SIZE)
            {
                Console.WriteLine("failed reading TLV info");
                return;
            }

            var reader = new BinaryReader(new MemoryStream(tlvInfoBytes));
            var tlvType = reader.ReadUInt32();
            var tlvLength = reader.ReadUInt32();

            byte [] tlvDataBytes = new byte[tlvLength];
            if (readTIDataFunction(tlvDataBytes, tlvDataBytes.Length) != tlvLength)
            {
                Console.WriteLine("failed reading TLV data!");
                return;
            }

            reader = new BinaryReader(new MemoryStream(tlvDataBytes));

            if (tlvType == TLV_TYPE_POINT_CLOUD)
            {
                var pointsCount = tlvLength / POINT_CLOUD_INFO_SIZE;
                for (int pointIndex = 0; pointIndex < pointsCount; pointIndex++)
                {
                    FrameData.Point point = new FrameData.Point();
                    point.Range = reader.ReadSingle();
                    point.Azimuth = reader.ReadSingle();
                    point.Elevation = reader.ReadSingle();
                    point.Doppler = reader.ReadSingle();
                    frameData.pointCloudList.Add(point);

                    // Console.WriteLine($"PointIndex: {pointIndex}, Range: {point.Range:0.00}, Azimuth: {point.Azimuth:0.00}, Elevation: {point.Elevation:0.00}, Doppler: {point.Doppler:0.00}");
                }
            }

            if (tlvType == TLV_TYPE_POINT_CLOUD_SIDE_INFO)
            {
                var pointsCount = tlvLength / POINT_CLOUD_SIDE_INFO_SIZE;
                for (int pointIndex = 0; pointIndex < pointsCount; pointIndex++)
                {
                    FrameData.PointSideInfo point = new FrameData.PointSideInfo();
                    point.SNR = reader.ReadInt16();
                    point.Noise = reader.ReadInt16();
                    frameData.pointCloudSideInfo.Add(point);

                    // Console.WriteLine($"PintSideInfo: {pointIndex}, SNR: {point.SNR}, Noise: {point.Noise}");
                }
            }

            if (tlvType == TLV_TYPE_TRACKS_LIST)
            {
                var tracksCount = tlvLength / TRACK_OBJECT_TLV_SIZE;
                for (int trackIndex = 0; trackIndex < tracksCount; trackIndex++)
                {
                    FrameData.Track track = new FrameData.Track();
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

                    Console.WriteLine($"Track ID: {track.TrackId}, posX: {track.PositionX:0.00}, posY: {track.PositionY:0.00}, posZ:{track.PositionZ:0.00}, velX:{track.VelocityX:0.00}, velY:{track.VelocityY:0.00},  velZ:{track.VelocityZ:0.00}");
                }
            }
        }

        System.Console.WriteLine("Done reading frame!!! :)");
    }

}