import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Radar } from 'src/app/entities/radar';
import { RadarWebsocketService } from 'src/app/services/radar-websocket.service';
import { RadarsService } from 'src/app/services/radars.service';

@Component({
  selector: 'app-fan-gestures-window',
  templateUrl: './fan-gestures-window.component.html',
  styleUrls: ['./fan-gestures-window.component.css'],
  providers: [RadarWebsocketService]
})
export class FanGesturesWindowComponent implements OnInit, OnDestroy {

  constructor(private radarsService : RadarsService,
              private deviceWebsocketService : RadarWebsocketService,
              private router : Router) { }

  radar : Radar | null
  predictionsSubscription! : any

  currentGesture = "No Gesture"
  currentTrackId = -1

  clearGestureTimer : any

  ngOnInit(): void {
  }

  ngOnDestroy(): void 
  {
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

        if (this.predictionsSubscription != null)
        {
          this.predictionsSubscription.unsubscribe()
          this.predictionsSubscription = null
        }
        
        // we have the radar info, now subscribe for tracks streaming
        this.predictionsSubscription = this.deviceWebsocketService.GetFanGesturesPredictions().subscribe({
          next : (predictions) => 
          {
            if (predictions.length > 0)
            {
              clearTimeout(this.clearGestureTimer)
              this.currentGesture = predictions[0].gesture
              this.currentTrackId = predictions[0].track_id

              this.clearGestureTimer = setTimeout(() => 
              {
                this.currentGesture = "No Gesture"
                this.currentTrackId = -1
              }, 3000);
            }
            else
            {
              clearTimeout(this.clearGestureTimer)
              this.currentGesture = "No Gesture"
              this.currentTrackId = -1
            }

          }
        })
        
      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }
  
}