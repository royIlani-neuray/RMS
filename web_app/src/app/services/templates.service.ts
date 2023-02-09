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
  
  public addRadarTemplate(templateName : string, model : string, application : string, description : string, configScript : string[])
  {
    return this.http.post("/api/templates", 
    {
      name : templateName,
      description : description,
      model : model,
      application : application,
      config_script : configScript
    })
  }
}
