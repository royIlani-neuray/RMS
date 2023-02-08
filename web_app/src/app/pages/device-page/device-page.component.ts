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
import { RadarDevice } from 'src/app/entities/radar-device';
import { DevicesService } from '../../services/devices.service';
import { DevicePageDataService } from './device-page-data.service';

@Component({
  selector: 'app-device-page',
  templateUrl: './device-page.component.html',
  styleUrls: ['./device-page.component.css'],
  providers: [DevicePageDataService]
})
export class DevicePageComponent implements OnInit, OnDestroy {

  constructor(private devicesService : DevicesService, 
              private devicePageData : DevicePageDataService,
              private router : Router, 
              private activatedRoute:ActivatedRoute) { }

  radarDevice : RadarDevice
  deviceId : string
  updateTimer : any

  ngOnInit(): void {
    let deviceId = this.activatedRoute.snapshot.paramMap.get("device_id");

    if (deviceId == null)
    {
      this.router.navigate(['/error-404'])
      return
    }

    this.deviceId = deviceId

    this.devicePageData.radarDeviceSubject.subscribe({
      next : (radarDevice) => {
        this.radarDevice = radarDevice
      }
    })

    this.devicePageData.getDevice(this.deviceId)

    this.updateTimer = setInterval(() => 
    {
      this.devicePageData.getDevice(this.deviceId)
    }, 3000)
    
  }

  ngOnDestroy(): void 
  {
    if (this.updateTimer) 
    {
      clearInterval(this.updateTimer);
    }
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
