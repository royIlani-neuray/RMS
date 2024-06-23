/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Camera, CameraBrief } from '../entities/camera';

export interface TestConnectionResponse {
  connected: boolean
  status_string: string
}

export interface RegisterCameraResponse {
  camera_id: string
}

@Injectable({
  providedIn: 'root'
})
export class CamerasService {

  constructor(private http:HttpClient) { }

  public getCameras()
  {
    return this.http.get<CameraBrief[]>("/api/cameras")
  }

  public getCamera(cameraId : string)
  {
    return this.http.get<Camera>("/api/cameras/" + cameraId)
  }

  public registerCamera(name : string, description : string, rtspUrl : string, frameRate : number, 
                        resolutionX : number, resolutionY : number, fovX : number, fovY : number)
  {
    return this.http.post<RegisterCameraResponse>("/api/cameras", 
    {
      name: name,
      description: description,
      rtsp_url: rtspUrl,
      enabled: true,
      frame_rate: frameRate,
      fov_x: fovX,
      fov_y: fovY,
      resolution_x: resolutionX,
      resolution_y: resolutionY
    })  
  }

  public testConnection(rtspUrl : string)
  {
    return this.http.post<TestConnectionResponse>("/api/cameras/test-connection", 
    {
      rtsp_url: rtspUrl
    })  
  }

  public enableCamera(cameraId : string)
  {
    return this.http.post("/api/cameras/" + cameraId + "/enable", "")
  }

  public disableCamera(cameraId : string)
  {
    return this.http.post("/api/cameras/" + cameraId + "/disable", "")
  }

  public deleteCamera(cameraId : string)
  {
    return this.http.delete("/api/cameras/" + cameraId)
  }

}
