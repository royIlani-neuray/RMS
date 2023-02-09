/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { RadarDeviceBrief } from 'src/app/entities/radar-device';
import { DevicesService } from '../../services/devices.service';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { RmsEventsService } from 'src/app/services/rms-events.service';

@Component({
  selector: 'app-devices-page',
  templateUrl: './devices-page.component.html',
  styleUrls: ['./devices-page.component.css']
})
export class DevicesPageComponent implements OnInit {

  deviceListLoaded = new Subject<boolean>();
  dataSource = new MatTableDataSource<RadarDeviceBrief>()
  displayedColumns: string[] = ['name', 'state', 'enabled', 'send_tracks_report', 'device_id', 'description'];

  constructor(private devicesService : DevicesService, 
              private rmsEventsService : RmsEventsService,
              private router : Router) { }

  ngOnInit(): void 
  {
    this.deviceListLoaded.next(false);
    
    this.getDeviceList()

    this.rmsEventsService.deviceUpdatedEvent.subscribe({
      next: (deviceId) => 
      {
        this.getDeviceList()
      }
    })

    this.rmsEventsService.deviceAddedEvent.subscribe({
      next: (deviceId) => 
      {
        this.getDeviceList()
      }
    })

    this.rmsEventsService.deviceDeletedEvent.subscribe({
      next: (deviceId) => 
      {
        this.getDeviceList()
      }
    })

  }

  public getDeviceList()
  {
    this.devicesService.getRadarDevices().subscribe({
      next : (devices) => 
      {
        this.dataSource.data = devices
        this.deviceListLoaded.next(true);
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }
}
