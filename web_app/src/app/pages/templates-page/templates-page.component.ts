import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { RadarTemplateBrief } from 'src/app/entities/radar-template';
import { TemplatesService } from '../../services/templates.service';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-templates-page',
  templateUrl: './templates-page.component.html',
  styleUrls: ['./templates-page.component.css']
})
export class TemplatesPageComponent implements OnInit {
 
  templateListLoaded = new Subject<boolean>();
  dataSource = new MatTableDataSource<RadarTemplateBrief>()
  displayedColumns: string[] = ['name', 'description', 'model', 'application'];
  updateTimer : any

  constructor(private templatesService : TemplatesService, private router : Router) { }

  ngOnInit(): void 
  {
    this.templateListLoaded.next(false);
    
    this.getDeviceList()
    // trigger periodic update
    this.updateTimer = setInterval(() => 
    {
      this.getDeviceList()
    }, 3000)
  }

  ngOnDestroy() 
  {
    if (this.updateTimer) 
    {
      clearInterval(this.updateTimer);
    }
  }

  public getDeviceList()
  {
    this.templatesService.getRadarTemplates().subscribe({
      next : (response) => 
      {
        this.dataSource.data = response as RadarTemplateBrief[]
        this.templateListLoaded.next(true);
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

}
