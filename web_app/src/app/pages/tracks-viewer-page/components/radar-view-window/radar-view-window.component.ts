/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { AfterViewInit, Component, ElementRef, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { TracksViewerDataService } from '../../tracks-viewer-data.service';
import { RadarsService } from 'src/app/services/radars.service';
import { Radar } from 'src/app/entities/radar';
import { Router } from '@angular/router';
import { RadarWebsocketService, GateIdPrediction } from 'src/app/services/radar-websocket.service';
import { ThreeJsView } from './threejs-view';
import { FrameData } from 'src/app/entities/frame-data';
import { Subject } from 'rxjs';


@Component({
  selector: 'radar-view-window',
  templateUrl: './radar-view-window.component.html',
  styleUrls: ['./radar-view-window.component.css'],
  providers: [RadarWebsocketService]
})
export class RadarViewWindowComponent implements OnInit, OnDestroy, AfterViewInit {

  @ViewChild('canvas') private canvasRef : ElementRef;
  
  private threeJsView: ThreeJsView = new ThreeJsView()
  
  constructor(public tracksViewerData : TracksViewerDataService,
              private radarsService : RadarsService,
              private deviceWebsocketService : RadarWebsocketService,
              private router : Router) { }


  private windowHeight = 0
  private windowWidth = 0
  private windowRefreshInterval : any
  @Input() windowSelected : boolean = false

  radar : Radar | null

  public frameDataSubject: Subject<FrameData> = new Subject<FrameData>()
  public gateIdPredictionsSubject: Subject<GateIdPrediction[]> = new Subject<GateIdPrediction[]>()

  frameDataSubscription! : any
  predictionsSubscription! : any

  ngOnInit(): void 
  {
    this.threeJsView.loadThreeJsFonts()

    this.tracksViewerData.selectedWindowSubject.subscribe({
      next : (selectedWindow) => {
        this.windowSelected = (selectedWindow === this)
      }
    })

    this.tracksViewerData.radarWindowsList.push(this)
  }

  ngOnDestroy(): void 
  {
    if (this.deviceWebsocketService.IsConnected())
    {
      this.deviceWebsocketService.Disconnect()
    }

    this.threeJsView.dispose()

    if (this.windowRefreshInterval != null)
    {
      clearInterval(this.windowRefreshInterval);
    }

    this.tracksViewerData.radarWindowsList = this.tracksViewerData.radarWindowsList.filter(window => {return window !== this})
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

  setRadar(radarId : string)
  {
    // request the radar device info based on the given device id
    this.radarsService.getRadar(radarId).subscribe({
      next : (radar) => {
        this.radar = radar
        
        this.threeJsView.initScene(this.radar)

        if (!this.deviceWebsocketService.IsConnected())
        {
          this.deviceWebsocketService.Connect(radarId)
        }
        
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
            // update the scene with the latest frame data
            this.threeJsView.updateTracks(frameData.tracks)
            this.threeJsView.updatePointsCloud(frameData.points)
            this.frameDataSubject.next(frameData)
          }
        })

        this.predictionsSubscription = this.deviceWebsocketService.GetGateIdPredictions().subscribe({
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
