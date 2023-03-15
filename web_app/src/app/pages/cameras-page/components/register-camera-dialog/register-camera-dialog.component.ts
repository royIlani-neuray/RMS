import { Component, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { CamerasService } from 'src/app/services/cameras.service';

@Component({
  selector: 'app-register-camera-dialog',
  templateUrl: './register-camera-dialog.component.html',
  styleUrls: ['./register-camera-dialog.component.css']
})
export class RegisterCameraDialogComponent implements OnInit {

  cameraNameFC: FormControl
  rtspUrlFC: FormControl
  descriptionFC: FormControl
  frameRateFC: FormControl
  resolutionXFC: FormControl
  resolutionYFC: FormControl
  fovXFC: FormControl
  fovYFC: FormControl

  testConnectionStatus : string = ""
  
  constructor(public dialogRef: MatDialogRef<RegisterCameraDialogComponent>,
    private camerasService : CamerasService,
    private router : Router) { }

  ngOnInit(): void 
  {
    this.cameraNameFC = new FormControl("", [Validators.required])
    this.rtspUrlFC = new FormControl("", [Validators.required])
    this.descriptionFC = new FormControl("", [])
    this.frameRateFC = new FormControl(0, [Validators.required])
    this.resolutionXFC = new FormControl(0, [Validators.required])
    this.resolutionYFC = new FormControl(0, [Validators.required])
    this.fovXFC = new FormControl(0, [Validators.required])
    this.fovYFC = new FormControl(0, [Validators.required])
  }

  public testConnectionClicked()
  {
    let rtspUrl = this.rtspUrlFC.value
    this.camerasService.testConnection(rtspUrl).subscribe({
      next: (response) => this.testConnectionStatus = response.status_string,
      error: (err) => this.testConnectionStatus = "Error: test connection failed."
    })
  }

  public onRegisterClicked()
  {
    if (!this.cameraNameFC.valid || !this.rtspUrlFC.valid ||!this.frameRateFC.valid 
        || !this.resolutionXFC.valid || !this.resolutionYFC.valid || !this.fovXFC.valid || !this.fovYFC.valid)
      return
      
    let name = this.cameraNameFC.value
    let rtspUrl = this.rtspUrlFC.value
    let description = this.descriptionFC.value
    let frameRate = this.frameRateFC.value
    let resolutionX = this.resolutionXFC.value
    let resolutionY = this.resolutionYFC.value
    let fovX = this.fovXFC.value
    let fovY = this.fovYFC.value

    this.camerasService.registerCamera(name, description, rtspUrl, frameRate, resolutionX, resolutionY, fovX, fovY).subscribe({
      next: (response) => {
        this.router.navigate(['/camera', response.camera_id])
        this.dialogRef.close(true)
      },
      error: (err) => this.dialogRef.close(false)
    })

  }

  public onCancelClicked()
  {
    this.dialogRef.close(false);
  }

}
