/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;

namespace WebService.Entites;

public class RadarSettings
{
    public class BoundaryBoxParams
    {
        [JsonPropertyName("x_min")]
        public float X_Min_Meters {get; set;} = -1;

        [JsonPropertyName("x_max")]
        public float X_Max_Meters {get; set;} = -1;

        [JsonPropertyName("y_min")]
        public float Y_Min_Meters {get; set;} = -1;

        [JsonPropertyName("y_max")]
        public float Y_Max_Meters {get; set;} = -1;

        [JsonPropertyName("z_min")]
        public float Z_Min_Meters {get; set;} = -1;

        [JsonPropertyName("z_max")]
        public float Z_Max_Meters {get; set;} = -1;
    }

    public class SensorPositionParams 
    {
        [JsonPropertyName("height")]
        public float HeightMeters {get; set;} = -1;

        [JsonPropertyName("azimuth_tilt")]
        public float AzimuthTiltDegrees {get; set;} = -1;

        [JsonPropertyName("elevation_tilt")]
        public float ElevationTiltDegrees {get; set;} = -1;
    }

    public class DetectionParameters
    {
        [JsonPropertyName("range_resolution")]
        public float RangeResolution {get; set;} = -1;

        [JsonPropertyName("velocity_resolution")]
        public float VelocityResolution {get; set;} = -1;

        [JsonPropertyName("max_range")]
        public float MaxRange {get; set;} = -1;

        [JsonPropertyName("max_velocity")]
        public float MaxVelocity {get; set;} = -1;

        [JsonPropertyName("frame_rate")]
        public float FrameRate {get; set;} = -1;

        [JsonPropertyName("frame_size")]
        public float FrameSize {get; set;} = -1;

        [JsonPropertyName("tx_count")]
        public float TxCount {get; set;} = -1;

        [JsonPropertyName("rx_count")]
        public float RxCount {get; set;} = -1;

    }

    public class AllocationParameters
    {
        [JsonPropertyName("snr_threshold")]
        public float SNRThreshold {get; set;} = -1;

        [JsonPropertyName("snr_threshold_obscured")]
        public float SNRThresholdObscured {get; set;} = -1;

        [JsonPropertyName("velocity_threshold")]
        public float VelocityThreshold {get; set;} = -1;

        [JsonPropertyName("points_threshold")]
        public float PointsThreshold {get; set;} = -1;

        [JsonPropertyName("max_distance_threshold")]
        public float MaxDistanceThreshold {get; set;} = -1;

        [JsonPropertyName("max_velocity_threshold")]
        public float MaxVeloctyThreshold {get; set;} = -1;
    }

    public class GatingParameters
    {
        [JsonPropertyName("gain")]
        public float Gain {get; set;} = -1;

        [JsonPropertyName("limit_width")]
        public float LimitWidth {get; set;} = -1;

        [JsonPropertyName("limit_depth")]
        public float LimitDepth {get; set;} = -1;

        [JsonPropertyName("limit_height")]
        public float LimitHeight {get; set;} = -1;

        [JsonPropertyName("limit_velocity")]
        public float LimitVelocity {get; set;} = -1;
    }

    // BoundaryBox - The physical dimensions of the space in which the tracker will operate
    [JsonPropertyName("boundary_box")]
    public BoundaryBoxParams? BoundaryBox {get; set;} = null;

    // StaticBoundaryBox - Defines the zone in which targets are expected to become static for a long time
    [JsonPropertyName("static_boundary_box")]
    public BoundaryBoxParams? StaticBoundaryBox {get; set;} = null;

    // PresenceBoundaryBox - Determines if any target exists within the occupancy area specified by the presence boundary box.
    [JsonPropertyName("presence_boundary_box")]
    public BoundaryBoxParams? PresenceBoundaryBox {get; set;} = null;

    [JsonPropertyName("sensor_position")]
    public SensorPositionParams? SensorPosition {get; set;} = null;

    [JsonPropertyName("radar_calibration")]
    public string RadarCalibration {get; set;} = String.Empty;

    [JsonPropertyName("allocation_params")]
    public AllocationParameters? AllocationParams {get; set;} = null;

    [JsonPropertyName("gating_params")]
    public GatingParameters? GatingParams {get; set;} = null;

    [JsonPropertyName("detection_params")]
    public DetectionParameters? DetectionParams {get; set;} = null;
    
}