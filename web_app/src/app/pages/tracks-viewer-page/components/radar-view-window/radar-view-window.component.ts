/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { AfterViewInit, Component, ElementRef, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { TracksViewerDataService } from '../../tracks-viewer-data.service';
import { DevicesService } from 'src/app/services/devices.service';
import { RadarDevice } from 'src/app/entities/radar-device';
import { Router } from '@angular/router';
import { DeviceWebsocketService, GateIdPrediction } from 'src/app/services/device-websocket.service';
import { ThreeJsView } from './threejs-view';
import { FrameData } from 'src/app/entities/frame-data';
import { Subject } from 'rxjs';


@Component({
  selector: 'radar-view-window',
  templateUrl: './radar-view-window.component.html',
  styleUrls: ['./radar-view-window.component.css'],
  providers: [DeviceWebsocketService]
})
export class RadarViewWindowComponent implements OnInit, OnDestroy, AfterViewInit {

  @ViewChild('canvas') private canvasRef : ElementRef;
  
  private threeJsView: ThreeJsView = new ThreeJsView()
  
  constructor(public tracksViewerData : TracksViewerDataService,
              private devicesService : DevicesService,
              private deviceWebsocketService : DeviceWebsocketService,
              private router : Router) { }


  private windowHeight = 0
  private windowWidth = 0
  private windowRefreshInterval : any
  @Input() windowSelected : boolean = false

  radarDevice : RadarDevice | null

  public frameDataSubject: Subject<FrameData> = new Subject<FrameData>()
  public gateIdPredictionsSubject: Subject<GateIdPrediction[]> = new Subject<GateIdPrediction[]>()

  ngOnInit(): void 
  {
    this.threeJsView.loadThreeJsFonts()

    this.tracksViewerData.selectedWindowSubject.subscribe({
      next : (selectedWindow) => {
        this.windowSelected = (selectedWindow === this)
      }
    })

  }

  ngOnDestroy(): void 
  {
    this.threeJsView.dispose()

    if (this.windowRefreshInterval != null)
    {
      clearInterval(this.windowRefreshInterval);
    }
  }

  ngAfterViewInit(): void 
  {
    this.windowHeight = this.canvasRef.nativeElement.offsetHeight
    this.windowWidth = this.canvasRef.nativeElement.offsetWidth

    this.threeJsView.initializeThreeJs(this.canvasRef)

    this.windowRefreshInterval = setInterval(() => 
    {
      this.onResize()
    }, 500)

    this.threeJsView.startRenderingLoop()
  }

  onResize()
  {
    // update the renderer size
    if (this.windowHeight != this.canvasRef.nativeElement.offsetHeight || this.windowWidth != this.canvasRef.nativeElement.offsetWidth)
    {
      this.windowHeight = this.canvasRef.nativeElement.offsetHeight
      this.windowWidth = this.canvasRef.nativeElement.offsetWidth
      this.threeJsView.resizeView(this.windowHeight, this.windowWidth)
    }
  }

  onClick()
  {
    this.tracksViewerData.setSelectedWindow(this)
  }

  detailsButtonClicked()
  {
    this.tracksViewerData.setSelectedWindow(this)
    this.tracksViewerData.drawer.open()
  }

  setRadarDevice(deviceId : string)
  {
    // request the radar device info based on the given device id
    this.devicesService.getRadarDevice(deviceId).subscribe({
      next : (device) => {
        this.radarDevice = device
        
        this.threeJsView.initScene(this.radarDevice)

        this.deviceWebsocketService.Connect(deviceId)

        // we have the radar info, now subscribe for tracks streaming
        this.deviceWebsocketService.GetFrameData().subscribe({
          next : (frameData) => 
          {
            // update the scene with the latest frame data
            this.threeJsView.updateTracks(frameData.tracks)
            this.threeJsView.updatePointsCloud(frameData.points)
            this.frameDataSubject.next(frameData)
          }
        })

        this.deviceWebsocketService.GetGateIdPredictions().subscribe({
          next : (predictions) => 
          {
            this.gateIdPredictionsSubject.next(predictions)
          }
        })
        
      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

  public getShowBoundingBox()
  {
    return this.threeJsView.showBoundingBox;
  }

  public getShowStaticBoundingBox()
  {
    return this.threeJsView.showStaticBoundingBox;
  }

  public getShowTracks()
  {
    return this.threeJsView.showTracks;
  }

  public getShowPointsCloud()
  {
    return this.threeJsView.showPointsCloud;
  }

  public setShowBoundingBox(isVisible : boolean)
  {
    this.threeJsView.setShowBoundingBox(isVisible)
  }

  public setShowStaticBoundingBox(isVisible : boolean)
  {
    this.threeJsView.setShowStaticBoundingBox(isVisible)
  }

  public setShowTracks(isVisible : boolean)
  {
    this.threeJsView.showTracks = isVisible
  }

  public setShowPointsCloud(isVisible : boolean)
  {
    this.threeJsView.showPointsCloud = isVisible
  }


}
