import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RadarDevice } from 'src/app/entities/radar-device';
import { DevicesService } from '../../services/devices.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { EditRadarInfoDialogComponent } from '../../components/edit-radar-info-dialog/edit-radar-info-dialog.component';
import { ConfirmDialogComponent } from '../../components/confirm-dialog/confirm-dialog.component';
import { SetDeviceConfigDialogComponent } from 'src/app/components/set-device-config-dialog/set-device-config-dialog.component';

@Component({
  selector: 'app-device-page',
  templateUrl: './device-page.component.html',
  styleUrls: ['./device-page.component.css']
})
export class DevicePageComponent implements OnInit {

  constructor(private devicesService : DevicesService, 
              private router : Router, 
              private activatedRoute:ActivatedRoute,
              private notification: MatSnackBar,
              public dialog: MatDialog) { }

  radarDevice : RadarDevice
  deviceId : string
  updateTimer : any

  ngOnInit(): void {
    let deviceId = this.activatedRoute.snapshot.paramMap.get("device_id");

    if (deviceId == null)
    {
      this.router.navigate(['/error-404'])
      return
    }

    this.deviceId = deviceId

    this.getDevice(this.deviceId)

    this.updateTimer = setInterval(() => 
    {
      this.getDevice(this.deviceId)
    }, 3000)
    
  }

  ngOnDestroy() 
  {
    if (this.updateTimer) 
    {
      clearInterval(this.updateTimer);
    }
  }
  
  public getDevice(deviceId : string)
  {
    this.devicesService.getRadarDevice(deviceId).subscribe({
      next : (device) => this.radarDevice = device,
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

  public enableRadarDevice()
  {
    this.devicesService.enableRadarDevice(this.deviceId).subscribe({
      next : (response) => this.getDevice(this.deviceId),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not enable radar device")
    })
  }

  public disableRadarDevice()
  {
    this.devicesService.disableRadarDevice(this.deviceId).subscribe({
      next : (response) => this.getDevice(this.deviceId),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not disable radar device")
    })
  }

  public deleteRadarDevice()
  {
    let dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '360px',
      height: '140px',
      data: { message : "Are you sure you want to delete the device?" }
    });

    dialogRef.afterClosed().subscribe(result => {

      if (result)
      {
        this.devicesService.deleteRadarDevice(this.deviceId).subscribe({
          next : (response) => this.router.navigate(['/devices']),
          error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not delete radar device")
        })
      }
    });
  }

  public editInfoClicked()
  {
    let dialogRef = this.dialog.open(EditRadarInfoDialogComponent, {
      width: '550px',
      height: '390px',
      data: { radarDevice: this.radarDevice }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result)
      {
        this.getDevice(this.deviceId)
      }
    });
  }

  public setDeviceConfiguration()
  {
    let dialogRef = this.dialog.open(SetDeviceConfigDialogComponent, {
      width: '850px',
      height: '690px',
      data: { radarDevice: this.radarDevice }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result)
      {
        this.notification.open("Radar configuration updated.", "Close", { duration : 2500, horizontalPosition : 'right', verticalPosition : 'top' }),
        this.getDevice(this.deviceId)
      }
    });    
  }

  public setTracksReports(sendReports : boolean)
  {
    this.devicesService.setTracksReports(this.deviceId, sendReports).subscribe({
      next : (response) => this.getDevice(this.deviceId),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not enable radar device")
    })    
  }

  private showNotification(message : string)
  {
    this.notification.open(message, "Close", { duration : 4000 })
  }

}
