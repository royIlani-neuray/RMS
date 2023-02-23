import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Camera, CameraBrief } from '../entities/camera';

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

  

}
