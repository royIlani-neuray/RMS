/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnInit } from '@angular/core';
import { Radar } from 'src/app/entities/radar';
import { RadarPageDataService } from '../../radar-page-data.service';
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

  constructor(private radarPageData : RadarPageDataService,
              private notification: MatSnackBar,
              private dialog: MatDialog) { }

  radar : Radar

  ngOnInit(): void {
    this.radar = this.radarPageData.radar
    this.radarPageData.radarSubject.subscribe({
      next : (radar) => {
        this.radar = radar
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
      data: { radar: this.radar }
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
