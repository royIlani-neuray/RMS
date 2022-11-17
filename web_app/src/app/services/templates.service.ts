import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { RadarTemplate, RadarTemplateBrief } from '../entities/radar-template';

@Injectable({
  providedIn: 'root'
})
export class TemplatesService {

  constructor(private http:HttpClient) { }

  public getRadarTemplates()
  {
    return this.http.get<RadarTemplateBrief[]>("/api/templates")
  }

  public getRadarTemplate(templateId : string)
  {
    return this.http.get<RadarTemplate>("/api/templates/" + templateId)
  }

  public deleteRadarTemplate(templateId : string)
  {
    return this.http.delete("/api/templates/" + templateId)
  }
  
}
