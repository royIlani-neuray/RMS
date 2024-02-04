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
import { DeviceMapping, Radar, RadarBrief } from '../entities/radar';
import { BoundaryBoxParams, SensorPositionParams } from '../entities/radar-settings';

@Injectable({
  providedIn: 'root'
})
export class RadarsService {

  constructor(private http:HttpClient) { }

  public getRadars()
  {
    return this.http.get<RadarBrief[]>("/api/radars")
  }

  public getRadar(radarId : string)
  {
    return this.http.get<Radar>("/api/radars/" + radarId)
  }

  public enableRadar(radarId : string)
  {
    return this.http.post("/api/radars/" + radarId + "/enable", "")
  }

  public disableRadar(radarId : string)
  {
    return this.http.post("/api/radars/" + radarId + "/disable", "")
  }

  public deleteRadar(radarId : string)
  {
    return this.http.delete("/api/radars/" + radarId)
  }

  public getDeviceMapping()
  {
    return this.http.get<DeviceMapping[]>("/api/device-mapping")
  }

  public triggerDeviceMapping()
  {
    return this.http.post("/api/device-mapping", "")
  }

  public setNetwork(radarId : string, ip : string, subnet : string, gateway : string, staticIP : boolean)
  {
    return this.http.put("/api/radars/" + radarId + "/network", {
      ip : ip,
      subnet: subnet,
      gateway : gateway,
      static_ip : staticIP
    })
  }

  public updateRadarInfo(radarId : string, name : string, description : string)
  {
    return this.http.put("/api/radars/" + radarId + "/radar-info", {
      name : name,
      description: description
    })
  }

  public setTracksReports(radarId : string, sendTracksReport : boolean)
  {
    return this.http.put("/api/radars/" + radarId + "/tracks-reports", {
      send_tracks_report: sendTracksReport
    })
  }

  public registerRadar(radarId : string, name : string, description : string, templateId : string, enabled : boolean, sendTracksReport : boolean,
    height : number, azimuthTilt : number, elevationTilt : number, calibration : string)
  {
    return this.http.post("/api/radars", {
      name : name,
      description: description,
      radar_id: radarId,
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

  public setRadarConfiguration(radarId : string, templateId : string, sensorPosition : SensorPositionParams, 
                                boundaryBox : BoundaryBoxParams, staticBoundaryBox : BoundaryBoxParams, calibration : string)
  {
    return this.http.post("/api/radars/" + radarId + "/config", 
    {
      template_id: templateId,
      sensor_position: sensorPosition,
      boundary_box: boundaryBox,
      static_boundary_box: staticBoundaryBox,
      radar_calibration : calibration
    })    
  }

  public setRadarConfigScript(radarId : string, configScript: string[])
  {
    return this.http.post("/api/radars/" + radarId + "/config", 
    {
      config: configScript
    })  
  }

  public enableRadarRecording(radarId : string)
  {
    return this.http.post("/api/radars/" + radarId + "/services", 
    {
      service_id : "RADAR_RECORDER",
      service_options : {}
    })
  }

  public disableRadarRecording(radarId : string)
  {
    return this.http.delete("/api/radars/" + radarId + "/services/RADAR_RECORDER")
  }

}
