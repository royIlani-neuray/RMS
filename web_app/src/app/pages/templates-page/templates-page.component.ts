/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { RadarTemplateBrief } from 'src/app/entities/radar-template';
import { TemplatesService } from '../../services/templates.service';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { RmsEventsService } from 'src/app/services/rms-events.service';
import { CreateTemplateDialogComponent } from './components/create-template-dialog/create-template-dialog.component';
import { MatSort } from '@angular/material/sort';

@Component({
  selector: 'app-templates-page',
  templateUrl: './templates-page.component.html',
  styleUrls: ['./templates-page.component.css']
})
export class TemplatesPageComponent implements OnInit {
 
  @ViewChild(MatSort) sort: MatSort;

  templateListLoaded = new Subject<boolean>();
  dataSource = new MatTableDataSource<RadarTemplateBrief>()
  displayedColumns: string[] = ['name', 'description', 'model', 'application'];

  constructor(private templatesService : TemplatesService, 
              private rmsEventsService : RmsEventsService, 
              private dialog: MatDialog,
              private router : Router) { }

  ngOnInit(): void 
  {
    this.templateListLoaded.next(false);
    
    this.getTemplatesList()

    this.rmsEventsService.templateAddedEvent.subscribe({
      next: (templateId) => 
      {
        this.getTemplatesList()
      }
    })

    this.rmsEventsService.templateDeletedEvent.subscribe({
      next: (templateId) => 
      {
        this.getTemplatesList()
      }
    })

  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  public getTemplatesList()
  {
    this.templatesService.getRadarTemplates().subscribe({
      next : (templates) => 
      {
        this.dataSource.data = templates
        this.templateListLoaded.next(true);
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  addTemplateClicked()
  {
    let dialogRef = this.dialog.open(CreateTemplateDialogComponent, {
      width: '850px',
      height: '690px'
    });   
  }

}
