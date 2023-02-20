/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Injectable } from '@angular/core';
import { MatDrawer } from '@angular/material/sidenav';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { RadarBrief } from 'src/app/entities/radar';
import { RadarsService } from 'src/app/services/radars.service';
import { RmsEventsService } from 'src/app/services/rms-events.service';
import { RadarViewWindowComponent } from './components/radar-view-window/radar-view-window.component';

@Injectable()
export class TracksViewerDataService {

  private selectedWindow : RadarViewWindowComponent
  public selectedWindowSubject: Subject<RadarViewWindowComponent> = new Subject<RadarViewWindowComponent>()
  public windowsLayoutSubject: Subject<string> = new Subject<string>()

  public radarsList: RadarBrief[] = [];

  public radarWindowsList: RadarViewWindowComponent[] = []; 

  public drawer: MatDrawer

  constructor (private radarsService : RadarsService,
               private rmsEventsService : RmsEventsService,
               private router : Router) 
  {
    this.getDeviceList()

    this.rmsEventsService.radarUpdatedEvent.subscribe({
      next: (deviceId) => 
      {
        this.getDeviceList()
      }
    })
    
  }
  
  private getDeviceList()
  {
    this.radarsService.getRadars().subscribe({
      next : (response) => this.radarsList = response as RadarBrief[],
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  public setSelectedWindow(window : RadarViewWindowComponent)
  {
    //console.log("window selected.")
    this.selectedWindow = window
    this.selectedWindowSubject.next(this.selectedWindow)
  }
  
  public setWindowsViewLayout(layout : string)
  {
    this.windowsLayoutSubject.next(layout)
  }

}