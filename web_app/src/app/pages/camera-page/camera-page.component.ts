import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/components/confirm-dialog/confirm-dialog.component';
import { Camera } from 'src/app/entities/camera';
import { CamerasService } from 'src/app/services/cameras.service';
import { RmsEventsService } from 'src/app/services/rms-events.service';

@Component({
  selector: 'app-camera-page',
  templateUrl: './camera-page.component.html',
  styleUrls: ['./camera-page.component.css']
})
export class CameraPageComponent implements OnInit {

  constructor(private rmsEventsService : RmsEventsService, 
              private camerasService : CamerasService,
              private router : Router, 
              public dialog: MatDialog,
              private notification: MatSnackBar,
              private activatedRoute:ActivatedRoute) { }

  camera : Camera

  ngOnInit(): void {
    let cameraId = this.activatedRoute.snapshot.paramMap.get("camera_id");

    if (cameraId == null)
    {
      this.router.navigate(['/error-404'])
      return
    }

    this.getCamera(cameraId)

    this.rmsEventsService.cameraUpdatedEvent.subscribe({
      next: (updatedCameraId) => 
      {
        if (cameraId == updatedCameraId)
        {
          this.getCamera(cameraId)
        }
      }
    })
  }

  public getCamera(cameraId : string)
  {
    this.camerasService.getCamera(cameraId).subscribe({
      next : (device) => 
      {
        this.camera = device
      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

  getCameraStatus()
  {
    if (!this.camera.enabled)
    {
      return "Disabled"
    }
    else
    {
      return this.camera.state
    }
  }

  public enableCamera()
  {
    this.camerasService.enableCamera(this.camera.device_id).subscribe({
      next : (response) => this.getCamera(this.camera.device_id),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not enable camera device")
    })
  }

  public disableCamera()
  {
    this.camerasService.disableCamera(this.camera.device_id).subscribe({
      next : (response) => this.getCamera(this.camera.device_id),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not disable camera device")
    })
  }

  public deleteCamera()
  {
    let dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '360px',
      height: '140px',
      data: { message : "Are you sure you want to delete the device?" }
    });

    dialogRef.afterClosed().subscribe(result => {

      if (result)
      {
        this.camerasService.deleteCamera(this.camera.device_id).subscribe({
          next : (response) => this.router.navigate(['/cameras']),
          error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not delete camera device")
        })
      }
    });
  }

  private showNotification(message : string)
  {
    this.notification.open(message, "Close", { duration : 4000 })
  }

  public isCameraRecordingEnabled()
  {
    let recordService = this.camera.linked_services.find(linkedService => { return linkedService.service_id == "CAMERA_RECORDER"})
    return recordService != null;
  }

  public enableCameraRecording()
  {
    this.camerasService.enableCameraRecording(this.camera.device_id).subscribe({
      next : (response) => this.getCamera(this.camera.device_id),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not enable camera recording")
    })
  }

  public disableCameraRecording()
  {
    this.camerasService.disableCameraRecording(this.camera.device_id).subscribe({
      next : (response) => this.getCamera(this.camera.device_id),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not disable camera recording")
    })
  }

}
