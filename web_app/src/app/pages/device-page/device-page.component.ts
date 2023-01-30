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
import { RadarDevice } from 'src/app/entities/radar-device';
import { DevicesService } from '../../services/devices.service';


@Component({
  selector: 'app-device-page',
  templateUrl: './device-page.component.html',
  styleUrls: ['./device-page.component.css']
})
export class DevicePageComponent implements OnInit {

  constructor(private devicesService : DevicesService, 
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

    this.getDevice(this.deviceId)

    this.updateTimer = setInterval(() => 
    {
      this.getDevice(this.deviceId)
    }, 3000)
    
  }

  ngOnDestroy() 
  {
    if (this.updateTimer) 
    {
      clearInterval(this.updateTimer);
    }
  }
  
  public getDevice(deviceId : string)
  {
    this.devicesService.getRadarDevice(deviceId).subscribe({
      next : (device) => this.radarDevice = device,
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }



}
