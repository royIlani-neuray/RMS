import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TemplatesService {

  constructor(private http:HttpClient) { }

  public getRadarTemplates()
  {
    return this.http.get("/api/templates")
  }

  public getRadarTemplate(templateId : string)
  {
    return this.http.get("/api/templates/" + templateId)
  }

  public deleteRadarTemplate(templateId : string)
  {
    return this.http.delete("/api/templates/" + templateId)
  }
  
}
