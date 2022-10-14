import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { RadarDeviceBrief } from 'src/app/entities/radar-device';


@Injectable({
  providedIn: 'root'
})
export class DevicesService {

  constructor(private http:HttpClient) { }

  public getRadarDevices()
  {
    //console.log('in getRadarDevices service')
    return this.http.get("http://localhost:4200/api/devices")
  }

  public getRadarDevice(deviceId : string)
  {
    return this.http.get("http://localhost:4200/api/devices/" + deviceId)
  }

  public enableRadarDevice(deviceId : string)
  {
    return this.http.post("http://localhost:4200/api/devices/" + deviceId + "/enable", "")
  }

  public disableRadarDevice(deviceId : string)
  {
    return this.http.post("http://localhost:4200/api/devices/" + deviceId + "/disable", "")
  }
}
