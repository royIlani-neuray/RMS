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
