import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Radar } from 'src/app/entities/radar';
import { RadarWebsocketService } from 'src/app/services/radar-websocket.service';
import { RadarsService } from 'src/app/services/radars.service';
import { MatTableDataSource } from '@angular/material/table';
import { TrackData } from 'src/app/entities/frame-data';

@Component({
  selector: 'app-radar-tracker-window',
  templateUrl: './radar-tracker-window.component.html',
  styleUrls: ['./radar-tracker-window.component.css'],
  providers: [RadarWebsocketService]
})
export class RadarTrackerWindowComponent implements OnInit, OnDestroy {

  constructor(private radarsService : RadarsService,
              private deviceWebsocketService : RadarWebsocketService,
              private router : Router) { }

  radar : Radar | null
  frameDataSubscription! : any

  tracksDataSource = new MatTableDataSource<TrackData>()
  tracksTableDisplayedColumns: string[] = ['track_id', 'range', 'position_x', 'position_y', 'position_z', 'velocity_x', 'velocity_y', 'velocity_z'];
  //tracksTableDisplayedColumns: string[] = ['track_id', 'range', 'position_x', 'position_y', 'position_z'];

  numberOfPoints = 0

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
    
        // we have the radar info, now subscribe for tracks streaming
        this.frameDataSubscription = this.deviceWebsocketService.GetFrameData().subscribe({
          next : (frameData) => 
          {
            this.tracksDataSource.data = frameData.tracks
            this.numberOfPoints = frameData.points.length
          }
        })

      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

  public getTrackRange(track : TrackData)
  {
    let radarHeight = this.radar!.radar_settings.sensor_position.height

    // track x,y,z is in reference to the floor which is the origin (0,0,0).
    // in order to get the range from the radar and not from the floor we reduce the radar height.

    return Math.sqrt(Math.pow(track.position_x,2) + Math.pow(track.position_y,2) + Math.pow((track.position_z - radarHeight),2))
  }

}
