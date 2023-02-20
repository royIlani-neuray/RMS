/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Radar } from 'src/app/entities/radar';
import { RmsEventsService } from 'src/app/services/rms-events.service';
import { RadarsService } from '../../services/radars.service';
import { RadarPageDataService } from './radar-page-data.service';

@Component({
  selector: 'app-radar-page',
  templateUrl: './radar-page.component.html',
  styleUrls: ['./radar-page.component.css'],
  providers: [RadarPageDataService]
})
export class RadarPageComponent implements OnInit {

  constructor(private rmsEventsService : RmsEventsService, 
              private radarPageData : RadarPageDataService,
              private router : Router, 
              private activatedRoute:ActivatedRoute) { }

  radarDevice : Radar
  deviceId : string

  ngOnInit(): void {
    let deviceId = this.activatedRoute.snapshot.paramMap.get("device_id");

    if (deviceId == null)
    {
      this.router.navigate(['/error-404'])
      return
    }

    this.deviceId = deviceId

    this.radarPageData.radarDeviceSubject.subscribe({
      next : (radarDevice) => {
        this.radarDevice = radarDevice
      }
    })

    this.radarPageData.getDevice(this.deviceId)

    this.rmsEventsService.radarUpdatedEvent.subscribe({
      next: (deviceId) => 
      {
        if (deviceId == this.deviceId)
        {
          this.radarPageData.getDevice(this.deviceId)
        }
      }
    })
    
  }

  getDeviceStatus()
  {
    if (!this.radarDevice.enabled)
    {
      return "Disabled"
    }
    else
    {
      return this.radarDevice.state
    }
  }
}
