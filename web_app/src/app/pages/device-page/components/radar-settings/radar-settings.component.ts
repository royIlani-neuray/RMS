/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnInit } from '@angular/core';
import { RadarDevice } from 'src/app/entities/radar-device';
import { DevicePageDataService } from '../../device-page-data.service';
import { HostListener } from '@angular/core';
import { ConfigScriptDialogComponent } from '../config-script-dialog/config-script-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-radar-settings',
  templateUrl: './radar-settings.component.html',
  styleUrls: ['./radar-settings.component.css']
})
export class RadarSettingsComponent implements OnInit {

  constructor(private devicePageData : DevicePageDataService,
              private notification: MatSnackBar,
              private dialog: MatDialog) { }

  radarDevice : RadarDevice

  ngOnInit(): void {
    this.radarDevice = this.devicePageData.radarDevice
    this.devicePageData.radarDeviceSubject.subscribe({
      next : (radarDevice) => {
        this.radarDevice = radarDevice
      }
    })
  }

  @HostListener('document:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) { 
    if (event.key == "F2")
    {
      this.openConfigScriptDialog()
    }
  }

  public openConfigScriptDialog()
  {
    let dialogRef = this.dialog.open(ConfigScriptDialogComponent, {
      width: '850px',
      height: '690px',
      data: { radarDevice: this.radarDevice }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result)
      {
        this.notification.open("Radar configuration updated.", "Close", { duration : 2500, horizontalPosition : 'right', verticalPosition : 'top' })
      }
    });    
  }

  private showNotification(message : string)
  {
    this.notification.open(message, "Close", { duration : 4000 })
  }

}
