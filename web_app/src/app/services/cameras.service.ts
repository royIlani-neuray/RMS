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

  public registerCamera(name : string, description : string, rtspUrl : string)
  {
    return this.http.post<RegisterCameraResponse>("/api/cameras", 
    {
      name: name,
      description: description,
      rtsp_url: rtspUrl,
      enabled: true
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

  public enableCameraRecording(cameraId : string)
  {
    return this.http.post("/api/cameras/" + cameraId + "/services", 
    {
      service_id : "CAMERA_RECORDER",
      service_options : {}
    })
  }

  public disableCameraRecording(cameraId : string)
  {
    return this.http.delete("/api/cameras/" + cameraId + "/services/CAMERA_RECORDER")
  }

}
