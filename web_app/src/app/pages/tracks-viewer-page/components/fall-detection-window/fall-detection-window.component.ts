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
    if (this.fallDetectionSubscription != null)
    {
      this.fallDetectionSubscription.unsubscribe()
      this.fallDetectionSubscription = null
    }

    // request the radar device info based on the given device id
    this.radarsService.getRadar(radarId).subscribe({
      next : (radar) => {
        this.radar = radar
        
        this.deviceWebsocketService.Connect(radarId)
        
        this.fallDetectionSubscription = this.deviceWebsocketService.GetFallDetectionData().subscribe({
          next : (fallDetectionData) => 
          {
            this.currentDetection = "[No Detection]";

            fallDetectionData.forEach((detection) => {
              if (detection.fall_detected)
              {
                this.currentDetection = "FALL DETECTED!!!";
              }
            })
          }
        })
        
        
      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }
  
}
