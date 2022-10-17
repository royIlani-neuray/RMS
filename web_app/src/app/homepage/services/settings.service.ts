import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SettingsService {

  constructor(private http:HttpClient) { }

  public getTrackingReportURL()
  {
    return this.http.get("http://localhost:4200/api/settings/tracking-report-url")
  }

  public getReportsInterval()
  {
    return this.http.get("http://localhost:4200/api/settings/reports-interval")
  }

}
