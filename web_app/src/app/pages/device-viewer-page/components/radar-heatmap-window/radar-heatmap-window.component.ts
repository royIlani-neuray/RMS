/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { AfterViewInit, Component, ElementRef, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Radar } from 'src/app/entities/radar';
import { RadarWebsocketService } from 'src/app/services/radar-websocket.service';
import { RadarsService } from 'src/app/services/radars.service';
import * as L from 'leaflet';
import 'leaflet.heat';

@Component({
  selector: 'app-radar-heatmap-window',
  templateUrl: './radar-heatmap-window.component.html',
  styleUrls: ['./radar-heatmap-window.component.css'],
  providers: [RadarWebsocketService]
})
export class RadarHeatmapWindowComponent implements OnInit, OnDestroy  {

  constructor(private radarsService : RadarsService,
              private deviceWebsocketService : RadarWebsocketService,
              private router : Router) { }

  radar : Radar | null
  frameDataSubscription! : any

  @Input() coordinates: { x: number, y: number }[] = [];

  private map!: L.Map;
  private heatLayer!: any;
  private xLength : number = 500
  private yLength : number = 500
  private bounds = [[0, 0], [this.xLength, this.yLength]]; // Define the size of the region

  ngOnInit(): void {
  }

  private initMap(): void {
    // Define a simple CRS for a rectangular region
    const crs = L.CRS.Simple;

    if (this.radar == null)
      return;

    this.xLength = this.radar.radar_settings.boundary_box.x_max * 100
    this.yLength = this.radar.radar_settings.boundary_box.y_max * 100

    this.bounds = [[0, 0], [this.yLength, this.xLength]]; 

    console.log(`bounds: ${this.bounds}`)

    this.map = L.map('map', {
      crs: crs,
      maxBounds: this.bounds as L.LatLngBoundsExpression,
      maxBoundsViscosity: 1.0,
      minZoom: -1,
      maxZoom: 1,
      zoom: 0,
      center: [this.xLength / 2, this.yLength / 2],
      zoomControl: false, // Disable zoom controls
      attributionControl: false // Disable Leaflet attribution
    });

    // Add a simple rectangle as the background
    //L.rectangle(this.bounds as L.LatLngBoundsExpression, { color: "#ffffff", weight: 1 }).addTo(this.map);
  }

  private calculateHeatPoints(): { heatPoints: [number, number, number][], maxIntensity: number } {
    const pointMap: { [key: string]: number } = {};
    let maxIntensity = 0;

    this.coordinates.forEach(coord => {
      const key = `${coord.x},${coord.y}`;
      if (pointMap[key]) {
        pointMap[key] += 1;
      } else {
        pointMap[key] = 1;
      }
      if (pointMap[key] > maxIntensity) {
        maxIntensity = pointMap[key];
      }
    });

    const heatPoints = Object.keys(pointMap).map(key => {
      const [x, y] = key.split(',').map(Number);
      const intensity = pointMap[key];
      return [y, x, intensity] as [number, number, number];
    });

    return { heatPoints, maxIntensity };
  }

  private updateHeatmapLayer(): void {
    if (this.heatLayer) {
      this.map.removeLayer(this.heatLayer);
    }
    const { heatPoints, maxIntensity } = this.calculateHeatPoints();

    this.heatLayer = (L as any).heatLayer(heatPoints, { radius: 20, max: maxIntensity }).addTo(this.map);
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
        
        this.initMap();

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
            frameData.tracks.forEach((track) => {
              this.coordinates.push({ x: (this.xLength/2) + Math.round(track.position_x * 10) * 10, y: (Math.round(track.position_y * 10) * 10) });
            })

            // Keep only the last 4000 coordinates
            if (this.coordinates.length > 4000) {
              this.coordinates = this.coordinates.slice(-4000);
            }

            this.updateHeatmapLayer();
          }
        })

      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

}
