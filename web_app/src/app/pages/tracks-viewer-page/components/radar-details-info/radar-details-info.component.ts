import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatTableDataSource } from '@angular/material/table';
import { TrackData } from 'src/app/entities/frame-data';
import { GateIdPrediction } from 'src/app/services/device-websocket.service';
import { TracksViewerDataService } from '../../tracks-viewer-data.service';
import { RadarViewWindowComponent } from '../radar-view-window/radar-view-window.component';


@Component({
  selector: 'app-radar-details-info',
  templateUrl: './radar-details-info.component.html',
  styleUrls: ['./radar-details-info.component.css']
})
export class RadarDetailsInfoComponent implements OnInit {

  showTracksFC = new FormControl(false)
  showBoundryBoxFC = new FormControl(false)
  showStaticBoundryBoxFC = new FormControl(false)
  showPointCloudFC = new FormControl(false)

  tracksDataSource = new MatTableDataSource<TrackData>()
  //tracksTableDisplayedColumns: string[] = ['track_id', 'range', 'position_x', 'position_y', 'position_z', 'velocity_x', 'velocity_y', 'velocity_z'];
  tracksTableDisplayedColumns: string[] = ['track_id', 'range', 'position_x', 'position_y', 'position_z'];

  gateIdPredictionsSource = new MatTableDataSource<GateIdPrediction>()
  gateIdDisplayedColumns: string[] = ['track_id', 'identity'];

  numberOfPoints = 0

  selectedRadarWindow! : RadarViewWindowComponent
  frameDataSubscription! : any
  predictionsSubscription! : any

  constructor(public tracksViewerData : TracksViewerDataService) { }

  ngOnInit(): void 
  {
    this.tracksViewerData.selectedWindowSubject.subscribe({
      next : (selectedWindow) => 
      {

        this.numberOfPoints = 0
        this.tracksDataSource.data = []
        this.gateIdPredictionsSource.data = []

        if (this.frameDataSubscription != null)
        {
          console.log("Unsubscribe from frame data!")
          this.frameDataSubscription.unsubscribe()
          this.frameDataSubscription = null
        }

        if (this.predictionsSubscription != null)
        {
          this.predictionsSubscription.unsubscribe()
          this.predictionsSubscription = null
        }

        this.selectedRadarWindow = selectedWindow
        
        if (this.selectedRadarWindow.radarDevice != null)
        {
          this.initViewFilters()
        }

        this.frameDataSubscription = this.selectedRadarWindow.frameDataSubject.subscribe({
          next: (frameData) => 
          {
            console.log("Got Frame data!!!")
            this.tracksDataSource.data = frameData.tracks
            this.numberOfPoints = frameData.points.length
          }
        })

        this.predictionsSubscription = this.selectedRadarWindow.gateIdPredictionsSubject.subscribe({
          next: (predictions) =>
          {
            this.gateIdPredictionsSource.data = predictions
          }
        })
      }
    })
  }

  private initViewFilters()
  {
    this.showTracksFC.setValue(this.selectedRadarWindow.getShowTracks())
    this.showBoundryBoxFC.setValue(this.selectedRadarWindow.getShowBoundingBox())
    this.showStaticBoundryBoxFC.setValue(this.selectedRadarWindow.getShowStaticBoundingBox())
    this.showPointCloudFC.setValue(this.selectedRadarWindow.getShowPointsCloud())
  }

  updateViewFilters()
  {
    this.selectedRadarWindow.setShowTracks(this.showTracksFC.value!)
    this.selectedRadarWindow.setShowBoundingBox(this.showBoundryBoxFC.value!)
    this.selectedRadarWindow.setShowStaticBoundingBox(this.showStaticBoundryBoxFC.value!)
    this.selectedRadarWindow.setShowPointsCloud(this.showPointCloudFC.value!)
  }

  public getTrackRange(track : TrackData)
  {
    let radarHeight = this.selectedRadarWindow.radarDevice!.radar_settings.sensor_position.height

    // track x,y,z is in reference to the floor which is the origin (0,0,0).
    // in order to get the range from the radar and not from the floor we reduce the radar height.

    return Math.sqrt(Math.pow(track.position_x,2) + Math.pow(track.position_y,2) + Math.pow((track.position_z - radarHeight),2))
  }

  closeButtonClicked()
  {
    this.tracksViewerData.drawer.close()
  }

}
