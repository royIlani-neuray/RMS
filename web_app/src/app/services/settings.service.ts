import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

export interface RMSSettings {
  reports_interval: number
  reports_url: string
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
    return this.http.get("/api/settings")
  }

  public updateSettings(reportsInterval : number, reportsURL : string)
  {
    return this.http.put("/api/settings", 
    {
      reports_interval : reportsInterval,
      reports_url: reportsURL
    })
  }
}
