/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnInit, ViewChild } from '@angular/core';
import { MatInput } from '@angular/material/input';
import { MatSelectionList } from '@angular/material/list';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { combineLatest } from 'rxjs';
import { CameraBrief } from 'src/app/entities/camera';
import { RadarBrief } from 'src/app/entities/radar';
import { AddRecordingScheduleArgs } from 'src/app/entities/recording-schedule';
import { CamerasService } from 'src/app/services/cameras.service';
import { RadarsService } from 'src/app/services/radars.service';
import { RecordingSchedulesService } from 'src/app/services/recording-schedules.service';
import { RecordingsService } from 'src/app/services/recordings.service';
import { RmsEventsService } from 'src/app/services/rms-events.service';
import { SettingsService } from 'src/app/services/settings.service';

@Component({
  selector: 'app-device-recorder',
  templateUrl: './device-recorder.component.html',
  styleUrls: ['./device-recorder.component.css']
})
export class DeviceRecorderComponent implements OnInit {

  radars: RadarBrief[] = []
  cameras: CameraBrief[] = []

  cloudUploadSupport = false;
  uploadS3 = false;
  isSchedule = false;
  days = ['S', 'M', 'T', 'W', 'T', 'F', 'S'];
  schedule: AddRecordingScheduleArgs = {
    name: '',
    start_time: '',
    end_time: '',
    start_days: [],
    end_days: [],
    radars: [],
    cameras: [],
    upload_s3: false,
  };

  @ViewChild("radarSelectionList") radarSelectionList: MatSelectionList;
  @ViewChild("cameraSelectionList") cameraSelectionList: MatSelectionList;

  @ViewChild(MatInput) nameInput: MatInput;

  notificationConfig: any = { duration : 2500, horizontalPosition : 'right', verticalPosition : 'bottom' };

  constructor(private radarsService : RadarsService,
              private camerasService : CamerasService,
              private recordingsService : RecordingsService,
              private recordingSchedulesService : RecordingSchedulesService,
              private rmsEventsService : RmsEventsService,
              private settingsService : SettingsService,
              private notification: MatSnackBar,
              private router : Router) { }

  ngOnInit(): void 
  {
    this.getRadarsList()
    this.getCamerasList()
    
    combineLatest([
      this.rmsEventsService.radarUpdatedEvent,
      this.rmsEventsService.cameraUpdatedEvent,
      this.rmsEventsService.radarDeletedEvent,
      this.rmsEventsService.recordingStartedEvent,
      this.rmsEventsService.recordingStoppedEvent,
    ]).subscribe({
      next: () => 
      {
        this.getRadarsList()
      }
    })

    this.settingsService.getCloudUploadSupport().subscribe({
      next : (result) => {
        this.cloudUploadSupport = result.support
      }
    })
  }

  public getRadarsList()
  {
    this.radarsService.getRadars().subscribe({
      next : (radars) => 
      {
        this.radars = radars
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  public getCamerasList()
  {
    this.camerasService.getCameras().subscribe({
      next : (cameras) => 
      {
        this.cameras = cameras
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  public startRecording()
  {
    let selectedRadars : string[] = this.radarSelectionList.selectedOptions.selected.map(item => item.value)
    let selectedCameras : string[] = this.cameraSelectionList.selectedOptions.selected.map(item => item.value)

    let recordingName : string = this.nameInput.value

    if (selectedCameras.length + selectedRadars.length == 0)
    {
      this.notification.open("No device selected.", "Close", { duration : 1000 })
      return
    }

    this.recordingsService.startRecording(recordingName, selectedRadars, selectedCameras, this.uploadS3).subscribe(
    {
      next : () => this.notification.open("Recording started.", "Close", this.notificationConfig),
      error: (err) => 
      {
        if (err.status == 504)
        {
          this.router.navigate(['/no-service'])
        }
        else if (err.status == 400)
        {
          this.notification.open("Error: recording with the given name already exist.", "Close", { duration : 2500 })
        }
        else 
        {
          this.notification.open("Error: Start recording failed.", "Close", { duration : 4000 })
        }
      }
    })

  }

  public stopRecording()
  {
    let selectedRadars : string[] = this.radarSelectionList.selectedOptions.selected.map(item => item.value)
    let selectedCameras : string[] = this.cameraSelectionList.selectedOptions.selected.map(item => item.value)

    if (selectedCameras.length + selectedRadars.length == 0)
    {
      this.notification.open("No device selected.", "Close", { duration : 1000 })
      return
    }

    this.recordingsService.stopRecording(selectedRadars, selectedCameras).subscribe(
    {
      next : () => this.notification.open("Recording stopped.", "Close", this.notificationConfig),
      error: (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.notification.open("Error: Stop recording failed.", "Close", { duration : 4000 })
    })
  }

  public getSelectedDevicesCount(): number
  {
    if (this.radarSelectionList == null)
      return 0
      
    let selectedRadars : string[] = this.radarSelectionList.selectedOptions.selected.map(item => item.value)
    let selectedCameras : string[] = this.cameraSelectionList.selectedOptions.selected.map(item => item.value)

    return selectedCameras.length + selectedRadars.length;
  }

  public isTriggerButtonDisabled()
  {
    return this.getSelectedDevicesCount() == 0;
  }

  public isValidSchedule(): boolean
  {
    if (this.getSelectedDevicesCount() == 0) return false;
    if (this.nameInput.value == "") return false;
    if (this.schedule.start_time == "") return false;
    if (this.schedule.end_time == "") return false;
    if (this.schedule.start_days.length == 0) return false;
    if (this.schedule.start_days.length != this.schedule.end_days.length) return false;
    return true;
  }

  public onSaveSchedule()
  {
    console.log(this.schedule);
    if (!this.isValidSchedule()) {
      this.notification.open("Not all schedule parameters set correctly.", "Close", this.notificationConfig);
      return;
    }
    this.schedule.name = this.nameInput.value;
    this.schedule.radars = this.radarSelectionList.selectedOptions.selected.map(item => item.value);
    this.schedule.cameras = this.cameraSelectionList.selectedOptions.selected.map(item => item.value);
    this.schedule.upload_s3 = this.uploadS3;
    if (this.schedule.start_time.length == 5) this.schedule.start_time += ':00';
    if (this.schedule.end_time.length == 5) this.schedule.end_time += ':00';
    
    this.recordingSchedulesService.addSchedule(this.schedule).subscribe({
      next : (response) => this.notification.open("Schedule saved and set.", "Close", this.notificationConfig),
      error : (err) => this.notification.open("Server Error: could not save schedule.", "Close", this.notificationConfig)
    })
  }
}
