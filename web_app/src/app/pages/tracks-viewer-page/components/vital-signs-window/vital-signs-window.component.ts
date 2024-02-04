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
import { MatTableDataSource } from '@angular/material/table';
import { TrackData } from 'src/app/entities/frame-data';

@Component({
  selector: 'app-vital-signs-window',
  templateUrl: './vital-signs-window.component.html',
  styleUrls: ['./vital-signs-window.component.css'],
  providers: [RadarWebsocketService]
})
export class VitalSignsWindowComponent implements OnInit, OnDestroy {

  constructor(private radarsService : RadarsService,
              private deviceWebsocketService : RadarWebsocketService,
              private router : Router) { }

  radar : Radar | null
  frameDataSubscription! : any

  targetId = 0
  heartRate = 0
  breathRate = 0

  ngOnInit(): void {
  }

  ngOnDestroy(): void 
  {
    if (this.frameDataSubscription != null)
    {
      this.frameDataSubscription.unsubscribe()
      this.frameDataSubscription = null
    }
  }

  setRadar(radarId : string)
  {
    // request the radar device info based on the given device id
    this.radarsService.getRadar(radarId).subscribe({
      next : (radar) => {
        this.radar = radar
        
        this.deviceWebsocketService.Connect(radarId)

        if (this.frameDataSubscription != null)
        {
          this.frameDataSubscription.unsubscribe()
          this.frameDataSubscription = null
        }
    
        // we have the radar info, now subscribe for frame data streaming
        this.frameDataSubscription = this.deviceWebsocketService.GetFrameData().subscribe({
          next : (frameData) => 
          {
            if (frameData.vital_signs != null)
            {
              this.targetId = frameData.vital_signs.target_id
              this.heartRate = frameData.vital_signs.heart_rate
              this.breathRate = frameData.vital_signs.breathing_rate
            }
          }
        })

      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

}
