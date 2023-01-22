/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { DeviceMapping, RadarDevice, RadarDeviceBrief } from '../entities/radar-device';
import { BoundaryBoxParams, SensorPositionParams } from '../entities/radar-settings';

@Injectable({
  providedIn: 'root'
})
export class DevicesService {

  constructor(private http:HttpClient) { }

  public getRadarDevices()
  {
    return this.http.get<RadarDeviceBrief[]>("/api/devices")
  }

  public getRadarDevice(deviceId : string)
  {
    return this.http.get<RadarDevice>("/api/devices/" + deviceId)
  }

  public enableRadarDevice(deviceId : string)
  {
    return this.http.post("/api/devices/" + deviceId + "/enable", "")
  }

  public disableRadarDevice(deviceId : string)
  {
    return this.http.post("/api/devices/" + deviceId + "/disable", "")
  }

  public deleteRadarDevice(deviceId : string)
  {
    return this.http.delete("/api/devices/" + deviceId)
  }

  public getDeviceMapping()
  {
    return this.http.get<DeviceMapping[]>("/api/device-mapping")
  }

  public triggerDeviceMapping()
  {
    return this.http.post("/api/device-mapping", "")
  }

  public setNetwork(deviceId : string, ip : string, subnet : string, gateway : string, staticIP : boolean)
  {
    return this.http.put("/api/devices/" + deviceId + "/network", {
      ip : ip,
      subnet: subnet,
      gateway : gateway,
      static_ip : staticIP
    })
  }

  public updateRadarInfo(deviceId : string, name : string, description : string)
  {
    return this.http.put("/api/devices/" + deviceId + "/radar-info", {
      name : name,
      description: description
    })
  }

  public setTracksReports(deviceId : string, sendTracksReport : boolean)
  {
    return this.http.put("/api/devices/" + deviceId + "/tracks-reports", {
      send_tracks_report: sendTracksReport
    })
  }

  public registerRadarDevice(deviceId : string, name : string, description : string, templateId : string, enabled : boolean, sendTracksReport : boolean,
    height : number, azimuthTilt : number, elevationTilt : number, calibration : string)
  {
    return this.http.post("/api/devices", {
      name : name,
      description: description,
      device_id: deviceId,
      template_id: templateId,
      enabled: enabled,
      send_tracks_report : sendTracksReport,
      radar_position : {
        height : height,
        azimuth_tilt : azimuthTilt,
        elevation_tilt : elevationTilt
      },
      radar_calibration: calibration
    })
  }

  public setDeviceConfiguration(deviceId : string, templateId : string, sensorPosition : SensorPositionParams, 
                                boundaryBox : BoundaryBoxParams, staticBoundaryBox : BoundaryBoxParams, calibration : string)
  {
    return this.http.post("/api/devices/" + deviceId + "/config", 
    {
      template_id: templateId,
      sensor_position: sensorPosition,
      boundary_box: boundaryBox,
      static_boundary_box: staticBoundaryBox,
      radar_calibration : calibration
    })    
  }

  public enableRadarRecording(deviceId : string)
  {
    return this.http.post("/api/devices/" + deviceId + "/services", 
    {
      service_id : "RADAR_RECORDER",
      service_options : {}
    })
  }

  public disableRadarRecording(deviceId : string)
  {
    return this.http.delete("/api/devices/" + deviceId + "/services/RADAR_RECORDER")
  }

}
