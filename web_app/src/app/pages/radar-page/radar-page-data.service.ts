/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { Radar } from 'src/app/entities/radar';
import { RadarsService } from 'src/app/services/radars.service';

@Injectable()
export class RadarPageDataService {

  public radarDeviceSubject: Subject<Radar> = new Subject<Radar>()
  public radarDevice : Radar;

  constructor (private radarsService : RadarsService,
               private router : Router) {}
  
  public getDevice(deviceId : string)
  {
    this.radarsService.getRadarDevice(deviceId).subscribe({
      next : (device) => 
      {
        this.radarDevice = device
        this.radarDeviceSubject.next(this.radarDevice)
      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

}