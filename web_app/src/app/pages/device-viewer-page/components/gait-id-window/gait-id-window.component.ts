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
  selector: 'app-gait-id-window',
  templateUrl: './gait-id-window.component.html',
  styleUrls: ['./gait-id-window.component.css'],
  providers: [RadarWebsocketService]
})
export class GaitIdWindowComponent implements OnInit, OnDestroy {

  constructor(private radarsService : RadarsService,
              private deviceWebsocketService : RadarWebsocketService,
              private router : Router) { }

  radar : Radar | null
  predictionsSubscription! : any
  frameDataSubscription! : any

  currentIdentity = "[No Fall Detected]"
  currentTrackId = -1

  ngOnInit(): void {
  }

  ngOnDestroy(): void 
  {
    if (this.frameDataSubscription != null)
    {
      this.frameDataSubscription.unsubscribe()
      this.frameDataSubscription = null
    }

    if (this.predictionsSubscription != null)
    {
      this.predictionsSubscription.unsubscribe()
      this.predictionsSubscription = null
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
    
        if (this.predictionsSubscription != null)
        {
          this.predictionsSubscription.unsubscribe()
          this.predictionsSubscription = null
        }

        // we have the radar info, now subscribe for tracks streaming
        this.frameDataSubscription = this.deviceWebsocketService.GetFrameData().subscribe({
          next : (frameData) => 
          {
            if (frameData.tracks.findIndex(track => track.track_id == this.currentTrackId) == -1)
            {
              this.currentIdentity = "[No Detection]"
              this.currentTrackId = -1
            }
          }
        })

        this.predictionsSubscription = this.deviceWebsocketService.GetGateIdPredictions().subscribe({
          next : (predictions) => 
          {
            this.currentIdentity = predictions[0].identity
            this.currentTrackId = predictions[0].track_id
          }
        })
        
      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

}
