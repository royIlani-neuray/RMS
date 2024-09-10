/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnInit, ViewChildren, ViewChild, QueryList, ElementRef, ViewContainerRef, AfterViewInit, ChangeDetectorRef, Input } from '@angular/core';
import { MatGridList } from '@angular/material/grid-list';
import { MatMenuTrigger } from '@angular/material/menu';
import { MatDrawer } from '@angular/material/sidenav';
import { RmsEventsService } from 'src/app/services/rms-events.service';
import { CameraViewWindowComponent } from './components/camera-view-window/camera-view-window.component';
import { GaitIdWindowComponent } from './components/gait-id-window/gait-id-window.component';
import { FallDetectionWindowComponent } from './components/fall-detection-window/fall-detection-window.component';
import { RadarViewWindowComponent } from './components/radar-view-window/radar-view-window.component';
import { DeviceViewerDataService } from './device-viewer-data.service';
import { FanGesturesWindowComponent } from './components/fan-gestures-window/fan-gestures-window.component';
import { RadarTrackerWindowComponent } from './components/radar-tracker-window/radar-tracker-window.component';
import { VitalSignsWindowComponent } from './components/vital-signs-window/vital-signs-window.component';
import { RadarHeatmapWindowComponent } from './components/radar-heatmap-window/radar-heatmap-window.component';

@Component({
  selector: 'dynamic-window',
  template: '<div *ngIf="!windowCreated">No view selected. (right click to select)</div>'
})
export class DynamicWindow {
  constructor(public viewContainerRef: ViewContainerRef) { }
  windowCreated = false
}

@Component({
  selector: 'app-tracks-viewer-page',
  templateUrl: './device-viewer-page.component.html',
  styleUrls: ['./device-viewer-page.component.css'],
  providers: [DeviceViewerDataService]
})
export class DeviceViewerPageComponent implements OnInit, AfterViewInit {
  
  @ViewChild(MatDrawer) drawer : MatDrawer;
  @ViewChild(MatGridList) windowsGrid: MatGridList;
  @ViewChildren(DynamicWindow) gridWindows: QueryList<DynamicWindow>;
  @ViewChild(MatMenuTrigger) contextMenu: MatMenuTrigger;

  contextMenuPosition = { x: '0px', y: '0px' };

  viewLayout = "1x1"
  windowsCount = 1
  selectedWindowIndex = 0

  constructor(public tracksViewerData : DeviceViewerDataService,
              private rmsEventsService : RmsEventsService) { }

  ngOnInit(): void 
  {
    this.tracksViewerData.windowsLayoutSubject.subscribe({
      next: (layout) => { this.ViewLayoutChanged(layout) }
    })

    this.rmsEventsService.radarUpdatedEvent.subscribe({
      next: (deviceId) => 
      {
        this.tracksViewerData.radarWindowsList.forEach((radarWindow) => 
        {
          if ((radarWindow.radar != null) && (radarWindow.radar.device_id == deviceId))
          {
            radarWindow.setRadar(deviceId)
          }
        })
      }
    })

  }

  ngAfterViewInit(): void {
    this.tracksViewerData.drawer = this.drawer
  }

  public ViewLayoutChanged(layout : string)
  {
    let rows: number = +layout.split("x")[0];
    let cols: number = +layout.split("x")[1];

    this.windowsGrid.cols = cols
    this.windowsCount = rows * cols
  }

  onRightClick(event: MouseEvent, windowIndex : number) 
  {
    this.selectedWindowIndex = windowIndex
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + 'px';
    this.contextMenuPosition.y = event.clientY + 'px';
    this.contextMenu.menu!.focusFirstItem('mouse');
    this.contextMenu.openMenu()
    //return false; // avoid default browser action on click
  }

  setRadarWindow(radarId : string)
  {
    this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.clear()
    this.gridWindows.get(this.selectedWindowIndex)!.windowCreated = true
    const component = this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.createComponent(RadarViewWindowComponent)
    component?.instance.setRadar(radarId)
  }

  setRadarTrackerWindow(radarId : string)
  {
    this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.clear()
    this.gridWindows.get(this.selectedWindowIndex)!.windowCreated = true
    const component = this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.createComponent(RadarTrackerWindowComponent)
    component?.instance.setRadar(radarId)
  }

  setRadarHeatmapWindow(radarId : string)
  {
    this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.clear()
    this.gridWindows.get(this.selectedWindowIndex)!.windowCreated = true
    const component = this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.createComponent(RadarHeatmapWindowComponent)
    component?.instance.setRadar(radarId)
  }

  setGaitIdWindow(radarId : string)
  {
    this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.clear()
    this.gridWindows.get(this.selectedWindowIndex)!.windowCreated = true
    const component = this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.createComponent(GaitIdWindowComponent)
    component?.instance.setRadar(radarId)
  }

  setFanGesturesWindow(radarId : string)
  {
    this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.clear()
    this.gridWindows.get(this.selectedWindowIndex)!.windowCreated = true
    const component = this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.createComponent(FanGesturesWindowComponent)
    component?.instance.setRadar(radarId)
  }

  setFallDetectionWindow(radarId : string)
  {
    this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.clear()
    this.gridWindows.get(this.selectedWindowIndex)!.windowCreated = true
    const component = this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.createComponent(FallDetectionWindowComponent)
    component?.instance.setRadar(radarId)
  }

  setVitalSignsWindow(radarId : string)
  {
    this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.clear()
    this.gridWindows.get(this.selectedWindowIndex)!.windowCreated = true
    const component = this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.createComponent(VitalSignsWindowComponent)
    component?.instance.setRadar(radarId)
  }

  setCameraWindow(cameraId : string)
  {
    this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.clear()
    this.gridWindows.get(this.selectedWindowIndex)!.windowCreated = true
    const component = this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.createComponent(CameraViewWindowComponent)
    component?.instance.setCamera(cameraId)
  }

}
