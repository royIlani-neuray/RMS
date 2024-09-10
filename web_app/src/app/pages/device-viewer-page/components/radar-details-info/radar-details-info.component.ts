/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatTableDataSource } from '@angular/material/table';
import { TrackData } from 'src/app/entities/frame-data';
import { GateIdPrediction } from 'src/app/services/radar-websocket.service';
import { DeviceViewerDataService } from '../../device-viewer-data.service';
import { RadarViewWindowComponent } from '../radar-view-window/radar-view-window.component';


@Component({
  selector: 'app-radar-details-info',
  templateUrl: './radar-details-info.component.html',
  styleUrls: ['./radar-details-info.component.css']
})
export class RadarDetailsInfoComponent implements OnInit, OnDestroy {

  showTracksFC = new FormControl(false)
  showBoundryBoxFC = new FormControl(false)
  showStaticBoundryBoxFC = new FormControl(false)
  showPointCloudFC = new FormControl(false)

  tracksDataSource = new MatTableDataSource<TrackData>()
  tracksTableDisplayedColumns: string[] = ['track_id', 'range', 'position_x', 'position_y', 'position_z', 'velocity_x', 'velocity_y', 'velocity_z'];
  //tracksTableDisplayedColumns: string[] = ['track_id', 'range', 'position_x', 'position_y', 'position_z'];

  gateIdPredictionsSource = new MatTableDataSource<GateIdPrediction>()
  gateIdDisplayedColumns: string[] = ['track_id', 'identity'];

  numberOfPoints = 0

  selectedRadarWindow! : RadarViewWindowComponent
  frameDataSubscription! : any
  predictionsSubscription! : any

  constructor(public tracksViewerData : DeviceViewerDataService) { }

  ngOnInit(): void 
  {
    this.tracksViewerData.selectedWindowSubject.subscribe({
      next : (selectedWindow) => 
      {
        if (selectedWindow === this.selectedRadarWindow)
          return;

        this.numberOfPoints = 0
        this.tracksDataSource.data = []
        this.gateIdPredictionsSource.data = []

        if (this.frameDataSubscription != null)
        {
          //console.log("Unsubscribe from frame data!")
          this.frameDataSubscription.unsubscribe()
          this.frameDataSubscription = null
        }

        if (this.predictionsSubscription != null)
        {
          this.predictionsSubscription.unsubscribe()
          this.predictionsSubscription = null
        }

        this.selectedRadarWindow = selectedWindow
        
        if (this.selectedRadarWindow.radar != null)
        {
          this.initViewFilters()
        }

        this.frameDataSubscription = this.selectedRadarWindow.frameDataSubject.subscribe({
          next: (frameData) => 
          {
            //console.log("Got Frame data!!!")
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
    let radarHeight = this.selectedRadarWindow.radar!.radar_settings.sensor_position.height

    // track x,y,z is in reference to the floor which is the origin (0,0,0).
    // in order to get the range from the radar and not from the floor we reduce the radar height.

    return Math.sqrt(Math.pow(track.position_x,2) + Math.pow(track.position_y,2) + Math.pow((track.position_z - radarHeight),2))
  }

  closeButtonClicked()
  {
    this.tracksViewerData.drawer.close()
  }

}
