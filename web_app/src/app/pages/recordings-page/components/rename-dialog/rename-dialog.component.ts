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
import { Observable } from 'rxjs';
import { RecordingInfo, RecordingsService } from 'src/app/services/recordings.service';

export interface DialogData {
  name: string
  onSave: (newName: string) => Observable<Object>
}

@Component({
  selector: 'app-rename-dialog',
  templateUrl: './rename-dialog.component.html',
  styleUrls: ['./rename-dialog.component.css']
})
export class RenameDialogComponent implements OnInit {

  @ViewChild(MatInput) textInput: MatInput;
  
  constructor(public dialogRef: MatDialogRef<RenameDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData, private cd : ChangeDetectorRef,
    private recordingsService : RecordingsService) { }

  ngOnInit(): void {
  }

  ngAfterViewInit(): void 
  {
    this.textInput.value = this.data.name
    this.cd.detectChanges()
  }

  public onSaveClicked()
  {
    this.dialogRef.close(this.textInput.value);
  }

  public onCancelClicked()
  {
    this.dialogRef.close(false);
  }
}
