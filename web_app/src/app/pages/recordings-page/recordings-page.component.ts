/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { DeviceEmulatorService } from 'src/app/services/device-emulator.service';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from 'src/app/components/confirm-dialog/confirm-dialog.component';
import { MatSort } from '@angular/material/sort';
import { RecordingEntry, RecordingInfo, RecordingsService } from 'src/app/services/recordings.service';
import {animate, state, style, transition, trigger} from '@angular/animations';

@Component({
  selector: 'app-recordings-page',
  templateUrl: './recordings-page.component.html',
  styleUrls: ['./recordings-page.component.css'],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({height: '0px', minHeight: '0'})),
      state('expanded', style({height: '*'})),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ],
})
export class RecordingsPageComponent implements OnInit {

  @ViewChild(MatSort) sort: MatSort;
  
  recordingListLoaded = new Subject<boolean>();
  dataSource = new MatTableDataSource<RecordingInfo>()
  displayedColumns: string[] = ['created_at', 'name', 'recording_size', 'recording_actions'];
  displayedColumnsWithExpend: string[] = [...this.displayedColumns, 'expand'];
  expandedElement: RecordingInfo | null;


  constructor(private deviceEmulatorService : DeviceEmulatorService, 
              private recordingsService : RecordingsService,
              private router : Router, private notification: MatSnackBar,
              public dialog: MatDialog) { }

  ngOnInit(): void 
  {
    this.recordingListLoaded.next(false);
    
    this.getRecordingsList()
  }

  public getRecordingsList()
  {
    this.recordingsService.getRadarRecordings().subscribe({
      next : (recordings) => 
      {
        this.dataSource.data = recordings
        this.recordingListLoaded.next(true);
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  public getLocalDateString(timestamp : string)
  {
    const matches = timestamp.match(/(\d+)-(\d+)-(\d+)T(\d+):(\d+):(\d+)/)!;

    const year = parseInt(matches[1], 10);
    const month = parseInt(matches[2], 10) - 1; // Note: months are 0-based in the Date constructor
    const day = parseInt(matches[3], 10);
    const hour = parseInt(matches[4], 10);
    const minute = parseInt(matches[5], 10);
    const second = parseInt(matches[6], 10);

    const utcDate = new Date(year, month, day, hour, minute, second);
    const timezoneOffset = utcDate.getTimezoneOffset() * -1;
    const localDate = new Date(utcDate.getTime() + timezoneOffset * 60 * 1000);

    return localDate.toLocaleDateString() + ' ' + localDate.toLocaleTimeString()
  }

  public getRecordingSize(recording : RecordingInfo)
  {
    let recordingSize = 0
    recording.entries.forEach(entry => { recordingSize += entry.entry_size_bytes })
    return recordingSize / (1024 * 1024)
  }

  public runPlayback(recordingName : string, recordingEntry : RecordingEntry)
  {
    this.deviceEmulatorService.setPlaybackSettings(recordingName, recordingEntry.device_id).subscribe({
      next : (response) => this.notification.open("Device emulator playback set", "Close", { duration : 2500, horizontalPosition : 'right', verticalPosition : 'top' }),
      error : (err) => err.status == 504 ? this.showNotification("Error: could not connect to device emulator") : this.showNotification("Error: could not set playback")
    })
  }
  
  public deleteRecording(recording : RecordingInfo)
  {
    let dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '360px',
      height: '140px',
      data: { message : "Delete the given recording?" }
    });

    dialogRef.afterClosed().subscribe(result => {

      if (result)
      {
        this.recordingsService.deleteRecording(recording.name).subscribe({
          next : (response) => this.getRecordingsList(),
          error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not delete radar device")
        })
      }
    });
  }

  private showNotification(message : string)
  {
    this.notification.open(message, "Close", { duration : 4000 })
  }

  onUploadFile(event : any) {

    const file:File = event.target.files[0];

    if (file) 
    {
        const formData = new FormData();

        formData.append("thumbnail", file);

        this.recordingsService.uploadRecording(formData).subscribe({
          next : (response) => this.notification.open("Recording uploaded.", "Close", { duration : 2500, horizontalPosition : 'right', verticalPosition : 'top' }),
          error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: upload recording failed!")
        })
    }
  }

}
