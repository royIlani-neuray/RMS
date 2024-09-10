/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Radar } from 'src/app/entities/radar';
import { RadarWebsocketService } from 'src/app/services/radar-websocket.service';
import { RadarsService } from 'src/app/services/radars.service';

@Component({
  selector: 'app-fall-detection-window',
  templateUrl: './fall-detection-window.component.html',
  styleUrls: ['./fall-detection-window.component.css'],
  providers: [RadarWebsocketService]
})
export class FallDetectionWindowComponent implements OnInit, OnDestroy {

  constructor(private radarsService : RadarsService,
              private deviceWebsocketService : RadarWebsocketService,
              private router : Router) { }

  radar : Radar | null
  fallDetectionSubscription! : any

  currentDetection = "[No Detection]"
  clearDetectionTimer : any

  ngOnInit(): void {
  }

  ngOnDestroy(): void 
  {
    if (this.fallDetectionSubscription != null)
    {
      this.fallDetectionSubscription.unsubscribe()
      this.fallDetectionSubscription = null
    }
  }

  setRadar(radarId : string)
  {
    // request the radar device info based on the given device id
    this.radarsService.getRadar(radarId).subscribe({
      next : (radar) => {
        this.radar = radar
        
        this.deviceWebsocketService.Connect(radarId)
        
        if (this.fallDetectionSubscription != null)
        {
          this.fallDetectionSubscription.unsubscribe()
          this.fallDetectionSubscription = null
        }
        
        this.fallDetectionSubscription = this.deviceWebsocketService.GetFallDetectionData().subscribe({
          next : (fallDetectionData) => 
          {
            clearTimeout(this.clearDetectionTimer)

            this.currentDetection = `FALL DETECTED!!! [Track-${fallDetectionData.track_id}]`;

            this.clearDetectionTimer = setTimeout(() => 
            {
              this.currentDetection = "[No Detection]"
            }, 3000);
          }
        })
        
      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }
  
}
