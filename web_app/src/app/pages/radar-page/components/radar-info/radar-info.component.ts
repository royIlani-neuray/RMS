/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Radar } from 'src/app/entities/radar';
import { RadarsService } from 'src/app/services/radars.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../../components/confirm-dialog/confirm-dialog.component';
import { SetDeviceConfigDialogComponent } from 'src/app/pages/radar-page/components/set-device-config-dialog/set-device-config-dialog.component';
import { EditRadarInfoDialogComponent } from 'src/app/pages/radar-page/components/edit-radar-info-dialog/edit-radar-info-dialog.component';
import { RadarPageDataService } from '../../radar-page-data.service';

@Component({
  selector: 'app-radar-info',
  templateUrl: './radar-info.component.html',
  styleUrls: ['./radar-info.component.css'],
})
export class RadarInfoComponent implements OnInit {

  constructor(private radarsService : RadarsService, 
              private radarPageData : RadarPageDataService,
              private router : Router, 
              private notification: MatSnackBar,
              public dialog: MatDialog) { }

  radar : Radar

  ngOnInit(): void {

    this.radar = this.radarPageData.radar
    this.radarPageData.radarSubject.subscribe({
      next : (radar) => {
        this.radar = radar
      }
    })

  }

  public enableRadar()
  {
    this.radarsService.enableRadar(this.radar.device_id).subscribe({
      next : (response) => this.radarPageData.getRadar(this.radar.device_id),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not enable radar device")
    })
  }

  public disableRadar()
  {
    this.radarsService.disableRadar(this.radar.device_id).subscribe({
      next : (response) => this.radarPageData.getRadar(this.radar.device_id),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not disable radar device")
    })
  }

  public deleteRadar()
  {
    let dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '360px',
      height: '140px',
      data: { message : "Are you sure you want to delete the device?" }
    });

    dialogRef.afterClosed().subscribe(result => {

      if (result)
      {
        this.radarsService.deleteRadar(this.radar.device_id).subscribe({
          next : (response) => this.router.navigate(['/radars']),
          error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not delete radar device")
        })
      }
    });
  }

  public editInfoClicked()
  {
    let dialogRef = this.dialog.open(EditRadarInfoDialogComponent, {
      width: '550px',
      height: '390px',
      data: { radar: this.radar }
    });
  }

  public setDeviceConfiguration()
  {
    let dialogRef = this.dialog.open(SetDeviceConfigDialogComponent, {
      width: '850px',
      height: '690px',
      data: { radar: this.radar }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result)
      {
        this.notification.open("Radar configuration updated.", "Close", { duration : 2500, horizontalPosition : 'right', verticalPosition : 'top' })
      }
    });    
  }

  public setTracksReports(sendReports : boolean)
  {
    this.radarsService.setTracksReports(this.radar.device_id, sendReports).subscribe({
      next : (response) => this.radarPageData.getRadar(this.radar.device_id),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not enable radar device")
    })    
  }

  private showNotification(message : string)
  {
    this.notification.open(message, "Close", { duration : 4000 })
  }

  public isRadarRecordingEnabled()
  {
    let recordService = this.radar.linked_services.find(linkedService => { return linkedService.service_id == "RADAR_RECORDER"})
    return recordService != null;
  }

  public isRadarCalibrated()
  {
    return (this.radar.radar_settings.radar_calibration != "0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0")
  }

  public enableRadarRecording()
  {
    this.radarsService.enableRadarRecording(this.radar.device_id).subscribe({
      next : (response) => this.radarPageData.getRadar(this.radar.device_id),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not enable radar recording")
    })
  }

  public disableRadarRecording()
  {
    this.radarsService.disableRadarRecording(this.radar.device_id).subscribe({
      next : (response) => this.radarPageData.getRadar(this.radar.device_id),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not disable radar recording")
    })
  }

}
