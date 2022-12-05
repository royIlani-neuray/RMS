/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RadarTemplate } from 'src/app/entities/radar-template';
import { TemplatesService } from 'src/app/services/templates.service';

@Component({
  selector: 'app-template-page',
  templateUrl: './template-page.component.html',
  styleUrls: ['./template-page.component.css']
})
export class TemplatePageComponent implements OnInit {

  template : RadarTemplate
  templateId : string

  constructor(private templatesService : TemplatesService, 
              private router : Router, 
              private activatedRoute:ActivatedRoute) { }

  ngOnInit(): void 
  {
    let templateId = this.activatedRoute.snapshot.paramMap.get("template_id");

    if (templateId == null)
    {
      this.router.navigate(['/error-404'])
      return
    }

    this.templateId = templateId

    this.getTemplate(templateId)
  }

  public getTemplate(templateId : string)
  {
    this.templatesService.getRadarTemplate(templateId).subscribe({
      next : (template) => this.template = template,
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

}
