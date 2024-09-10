/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Injectable } from '@angular/core';
import { MatDrawer } from '@angular/material/sidenav';
import { Router } from '@angular/router';
import { Subject, combineLatest } from 'rxjs';
import { CameraBrief } from 'src/app/entities/camera';
import { RadarBrief } from 'src/app/entities/radar';
import { CamerasService } from 'src/app/services/cameras.service';
import { RadarsService } from 'src/app/services/radars.service';
import { RmsEventsService } from 'src/app/services/rms-events.service';
import { RadarViewWindowComponent } from './components/radar-view-window/radar-view-window.component';

@Injectable()
export class DeviceViewerDataService {

  private selectedWindow : RadarViewWindowComponent
  public selectedWindowSubject: Subject<RadarViewWindowComponent> = new Subject<RadarViewWindowComponent>()
  public windowsLayoutSubject: Subject<string> = new Subject<string>()

  public radarsList: RadarBrief[] = [];
  public camerasList: CameraBrief[] = [];

  public radarWindowsList: RadarViewWindowComponent[] = []; 

  public drawer: MatDrawer

  constructor (private radarsService : RadarsService,
               private camerasService : CamerasService,
               private rmsEventsService : RmsEventsService,
               private router : Router) 
  {
    this.getRadarList()
    this.getCameraList()

    combineLatest([
      this.rmsEventsService.radarUpdatedEvent,
      this.rmsEventsService.cameraUpdatedEvent,
    ]).subscribe({
      next: () => 
      {
        this.getRadarList()
      }
    })

  }
  
  private getRadarList()
  {
    this.radarsService.getRadars().subscribe({
      next : (response) => this.radarsList = response as RadarBrief[],
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  private getCameraList()
  {
    this.camerasService.getCameras().subscribe({
      next : (response) => this.camerasList = response as CameraBrief[],
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