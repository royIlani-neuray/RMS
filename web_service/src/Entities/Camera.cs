/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.CameraLogic.CameraStream;
using WebService.Database;
using WebService.WebSockets;

namespace WebService.Entites;

public class Camera : DeviceEntity {
    
    [JsonIgnore]
    public override IEntity.EntityTypes EntityType => IEntity.EntityTypes.Camera;

    [JsonIgnore]
    public override string StoragePath => StorageDatabase.CameraStoragePath;

    [JsonPropertyName("rtsp_url")]
    public String RTSPUrl { get; set; }

    [JsonPropertyName("frame_rate")]
    public float FrameRate { get; set; }

    [JsonPropertyName("fov_x")]
    public int FieldOfViewX { get; set; }

    [JsonPropertyName("fov_y")]
    public int FieldOfViewY { get; set; }

    [JsonPropertyName("resolution_x")]
    public int ResolutionX { get; set; }

    [JsonPropertyName("resolution_y")]
    public int ResolutionY { get; set; }

    [JsonIgnore]
    public CameraWebSocketServer CameraWebSocket;

    [JsonIgnore]
    public CameraStreamer? cameraStreamer;

    public class CameraBrief : DeviceBrief
    {
        public CameraBrief(Camera camera) : base(camera)
        {
            // No additional camera specific details.
        }
    }

    public Camera() : base(DeviceTypes.Camera)
    {
        Id = Guid.NewGuid().ToString();
        RTSPUrl = String.Empty;
        CameraWebSocket = new CameraWebSocketServer();
    }
    
}