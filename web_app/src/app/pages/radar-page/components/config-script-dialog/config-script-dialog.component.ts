/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { AfterViewInit, ChangeDetectorRef, Component, Inject, OnInit, ViewChild } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatInput } from '@angular/material/input';
import { Radar } from 'src/app/entities/radar';
import { RadarsService } from 'src/app/services/radars.service';

export interface DialogData {
  radar: Radar
}

@Component({
  selector: 'app-config-script-dialog',
  templateUrl: './config-script-dialog.component.html',
  styleUrls: ['./config-script-dialog.component.css']
})
export class ConfigScriptDialogComponent implements OnInit , AfterViewInit{

  @ViewChild(MatInput) textInput: MatInput;

  radar: Radar

  constructor(public dialogRef: MatDialogRef<ConfigScriptDialogComponent>,
              @Inject(MAT_DIALOG_DATA) public data: DialogData,
              private radarsService : RadarsService,
              private cd : ChangeDetectorRef) 
              { }

  ngOnInit(): void 
  {
    this.radar = this.data.radar
  }

  ngAfterViewInit(): void 
  {
    let config = ""
    this.radar.config_script.forEach((line) => { config += line + "\n" })
    this.textInput.value = config
    this.cd.detectChanges()
  }

  public onSaveClicked()
  {
    let configScript = this.textInput.value.split("\n")

    this.radarsService.setRadarConfigScript(this.radar.device_id, configScript).subscribe({
      next : (response) => this.dialogRef.close(true),
      error : (err) => this.dialogRef.close(false)
    })
  }

  public onCancelClicked()
  {
    this.dialogRef.close(false);
  }
}
