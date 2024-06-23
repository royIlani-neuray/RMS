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

export interface RMSSettings {
  reports_interval: number
  reports_url: string
}

export interface RMSVersion {
  version: string
}

export interface CloudUploadSupport {
  support: boolean
}

@Injectable({
  providedIn: 'root'
})
export class SettingsService {

  constructor(private http:HttpClient) { }

  public getTrackingReportURL()
  {
    return this.http.get("/api/settings/tracking-report-url")
  }

  public getReportsInterval()
  {
    return this.http.get("/api/settings/reports-interval")
  }

  public getSettings()
  {
    return this.http.get<RMSSettings>("/api/settings")
  }

  public updateSettings(reportsInterval : number, reportsURL : string)
  {
    return this.http.put("/api/settings", 
    {
      reports_interval : reportsInterval,
      reports_url: reportsURL
    })
  }

  public getRMSVersion()
  {
    return this.http.get<RMSVersion>("/api/settings/version")
  }

  public getCloudUploadSupport()
  {
    return this.http.get<CloudUploadSupport>("/api/settings/cloud-upload-support")
  }
}
