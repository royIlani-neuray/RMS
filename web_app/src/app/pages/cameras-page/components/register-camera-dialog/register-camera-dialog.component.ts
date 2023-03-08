import { Component, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { CamerasService } from 'src/app/services/cameras.service';

@Component({
  selector: 'app-register-camera-dialog',
  templateUrl: './register-camera-dialog.component.html',
  styleUrls: ['./register-camera-dialog.component.css']
})
export class RegisterCameraDialogComponent implements OnInit {

  cameraNameFC: FormControl
  rtspUrlFC: FormControl

  testConnectionStatus : string = "Works!"
  
  constructor(public dialogRef: MatDialogRef<RegisterCameraDialogComponent>,
    private camerasService : CamerasService) { }

  ngOnInit(): void 
  {
    this.cameraNameFC = new FormControl("", [Validators.required])
    this.rtspUrlFC = new FormControl("", [Validators.required])
  }

  public testConnectionClicked()
  {
    //this.camerasService.
  }

  public onRegisterClicked()
  {

  }

  public onCancelClicked()
  {
    this.dialogRef.close(false);
  }

}
