/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { AfterViewInit, ChangeDetectorRef, Component, Inject, OnInit, ViewChild } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatInput } from '@angular/material/input';
import { RecordingInfo, RecordingsService } from 'src/app/services/recordings.service';

export interface DialogData {
  recording: RecordingInfo
}

@Component({
  selector: 'app-rename-recording-dialog',
  templateUrl: './rename-recording-dialog.component.html',
  styleUrls: ['./rename-recording-dialog.component.css']
})
export class RenameRecordingDialogComponent implements OnInit {

  @ViewChild(MatInput) textInput: MatInput;
  
  constructor(public dialogRef: MatDialogRef<RenameRecordingDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData, private cd : ChangeDetectorRef,
    private recordingsService : RecordingsService) { }

  ngOnInit(): void {
  }

  ngAfterViewInit(): void 
  {
    this.textInput.value = this.data.recording.name
    this.cd.detectChanges()
  }

  public onSaveClicked()
  {
    let configScript = this.textInput.value.split("\n")

    this.recordingsService.renameRecording(this.data.recording.name, this.textInput.value).subscribe({
      next : (response) => this.dialogRef.close(true),
      error : (err) => this.dialogRef.close(false)
    })
  }

  public onCancelClicked()
  {
    this.dialogRef.close(false);
  }

}
