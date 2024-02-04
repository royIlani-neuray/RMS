/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Radar } from 'src/app/entities/radar';
import { RmsEventsService } from 'src/app/services/rms-events.service';
import { RadarsService } from '../../services/radars.service';
import { RadarPageDataService } from './radar-page-data.service';

@Component({
  selector: 'app-radar-page',
  templateUrl: './radar-page.component.html',
  styleUrls: ['./radar-page.component.css'],
  providers: [RadarPageDataService]
})
export class RadarPageComponent implements OnInit {

  constructor(private rmsEventsService : RmsEventsService, 
              private radarPageData : RadarPageDataService,
              private router : Router, 
              private activatedRoute:ActivatedRoute) { }

  radar : Radar

  ngOnInit(): void {
    let radarId = this.activatedRoute.snapshot.paramMap.get("radar_id");

    if (radarId == null)
    {
      this.router.navigate(['/error-404'])
      return
    }

    this.radarPageData.radarSubject.subscribe({
      next : (radar) => {
        this.radar = radar
      }
    })

    this.radarPageData.getRadar(radarId)

    this.rmsEventsService.radarUpdatedEvent.subscribe({
      next: (updatedRadarId) => 
      {
        if (radarId == updatedRadarId)
        {
          this.radarPageData.getRadar(radarId)
        }
      }
    })
    
  }

  getRadarStatus()
  {
    if (!this.radar.enabled)
    {
      return "Disabled"
    }
    else
    {
      return this.radar.state
    }
  }
}
