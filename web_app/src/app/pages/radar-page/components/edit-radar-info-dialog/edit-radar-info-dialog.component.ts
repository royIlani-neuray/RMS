/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { ChangeDetectorRef, Component, Inject, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatInput } from '@angular/material/input';
import { Router } from '@angular/router';
import { Radar } from 'src/app/entities/radar';
import { RadarsService } from 'src/app/services/radars.service';


export interface DialogData {
  radarDevice: Radar
}

@Component({
  selector: 'app-edit-radar-info-dialog',
  templateUrl: './edit-radar-info-dialog.component.html',
  styleUrls: ['./edit-radar-info-dialog.component.css']
})
export class EditRadarInfoDialogComponent implements OnInit {

  @ViewChildren(MatInput) inputComponents: QueryList<MatInput>;
  
  radarDevice: Radar

  radarNameFC: FormControl
  
  constructor(public dialogRef: MatDialogRef<EditRadarInfoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData, private cd : ChangeDetectorRef,
    private router : Router,
    private radarsService : RadarsService) { }

  ngOnInit(): void 
  {
    this.radarDevice = this.data.radarDevice
    this.radarNameFC = new FormControl(this.radarDevice.name, [Validators.required])
  }

  private getNameInput() {
    return this.inputComponents.get(0)!
  }

  private getDescriptionInput() {
    return this.inputComponents.get(1)!
  }


  onSaveClicked() 
  {
    if (!this.radarNameFC.valid)
      return

    let name = this.getNameInput().value
    let description = this.getDescriptionInput().value

    this.radarsService.updateRadarInfo(this.radarDevice.device_id, name, description).subscribe({
      next : (response) => this.dialogRef.close(true),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

  onCancelClicked() 
  {
    this.dialogRef.close(false);
  }

}
