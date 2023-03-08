import { Component, OnInit, ViewChild } from '@angular/core';
import { MatInput } from '@angular/material/input';
import { MatSelectionList } from '@angular/material/list';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { CameraBrief } from 'src/app/entities/camera';
import { RadarBrief } from 'src/app/entities/radar';
import { CamerasService } from 'src/app/services/cameras.service';
import { RadarsService } from 'src/app/services/radars.service';
import { RecordingsService } from 'src/app/services/recordings.service';
import { RmsEventsService } from 'src/app/services/rms-events.service';

@Component({
  selector: 'app-device-recorder',
  templateUrl: './device-recorder.component.html',
  styleUrls: ['./device-recorder.component.css']
})
export class DeviceRecorderComponent implements OnInit {

  radars: RadarBrief[] = []
  cameras: CameraBrief[] = []

  @ViewChild("radarSelectionList") radarSelectionList: MatSelectionList;
  @ViewChild("cameraSelectionList") cameraSelectionList: MatSelectionList;

  constructor(private radarsService : RadarsService,
              private camerasService : CamerasService,
              private recordingsService : RecordingsService,
              private rmsEventsService : RmsEventsService,
              private notification: MatSnackBar,
              private router : Router) { }

  ngOnInit(): void 
  {
    this.getRadarsList()
    this.getCamerasList()

    this.rmsEventsService.radarUpdatedEvent.subscribe({
      next: () => 
      {
        this.getRadarsList()
      }
    })

    this.rmsEventsService.cameraUpdatedEvent.subscribe({
      next: () => 
      {
        this.getCamerasList()
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

    if (selectedCameras.length + selectedRadars.length == 0)
    {
      this.notification.open("No device selected.", "Close", { duration : 1000 })
      return
    }

    this.recordingsService.startRecording("", selectedRadars, selectedCameras).subscribe(
    {
      next : () => this.notification.open("Recording started.", "Close", { duration : 2500, horizontalPosition : 'right', verticalPosition : 'top' }),
      error: (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.notification.open("Error: Start recording failed.", "Close", { duration : 4000 })
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
      next : () => this.notification.open("Recording stopped.", "Close", { duration : 2500, horizontalPosition : 'right', verticalPosition : 'top' }),
      error: (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.notification.open("Error: Stop recording failed.", "Close", { duration : 4000 })
    })

  }

  public isTriggerButtonDisabled()
  {
    if (this.radarSelectionList == null)
      return true
      
    let selectedRadars : string[] = this.radarSelectionList.selectedOptions.selected.map(item => item.value)
    let selectedCameras : string[] = this.cameraSelectionList.selectedOptions.selected.map(item => item.value)

    if (selectedCameras.length + selectedRadars.length == 0)
    {
      return true
    } 

    return false  
  }

}
