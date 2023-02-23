/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnInit, ViewChildren, ViewChild, QueryList, ElementRef, ViewContainerRef, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { MatGridList } from '@angular/material/grid-list';
import { MatMenuTrigger } from '@angular/material/menu';
import { MatDrawer } from '@angular/material/sidenav';
import { RmsEventsService } from 'src/app/services/rms-events.service';
import { CameraViewWindowComponent } from './components/camera-view-window/camera-view-window.component';
import { RadarViewWindowComponent } from './components/radar-view-window/radar-view-window.component';
import { TracksViewerDataService } from './tracks-viewer-data.service';

@Component({
  selector: 'dynamic-window',
  template: '<div>No device selected. (right click to select)</div>'
})
export class DynamicWindow {
  constructor(public viewContainerRef: ViewContainerRef) { }
}

@Component({
  selector: 'app-tracks-viewer-page',
  templateUrl: './tracks-viewer-page.component.html',
  styleUrls: ['./tracks-viewer-page.component.css'],
  providers: [TracksViewerDataService]
})
export class TracksViewerPageComponent implements OnInit, AfterViewInit {
  
  @ViewChild(MatDrawer) drawer : MatDrawer;
  @ViewChild(MatGridList) windowsGrid: MatGridList;
  @ViewChildren(DynamicWindow) gridWindows: QueryList<DynamicWindow>;
  @ViewChild(MatMenuTrigger) contextMenu: MatMenuTrigger;

  contextMenuPosition = { x: '0px', y: '0px' };

  viewLayout = "1x1"
  windowsCount = 1
  selectedWindowIndex = 0

  constructor(public tracksViewerData : TracksViewerDataService,
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

  setRadar(radarId : string)
  {
    this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.clear()
    const component = this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.createComponent(RadarViewWindowComponent)
    component?.instance.setRadar(radarId)
  }

  setCamera(cameraId : string)
  {
    this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.clear()
    const component = this.gridWindows.get(this.selectedWindowIndex)?.viewContainerRef.createComponent(CameraViewWindowComponent)
    component?.instance.setCamera(cameraId)
  }

}
