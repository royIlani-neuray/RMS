import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RadarDevice } from 'src/app/entities/radar-device';
import { DevicesService } from 'src/app/services/devices.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../../components/confirm-dialog/confirm-dialog.component';
import { SetDeviceConfigDialogComponent } from 'src/app/pages/device-page/components/set-device-config-dialog/set-device-config-dialog.component';
import { EditRadarInfoDialogComponent } from 'src/app/pages/device-page/components/edit-radar-info-dialog/edit-radar-info-dialog.component';
import { DevicePageDataService } from '../../device-page-data.service';

@Component({
  selector: 'app-device-info',
  templateUrl: './device-info.component.html',
  styleUrls: ['./device-info.component.css'],
})
export class DeviceInfoComponent implements OnInit {

  constructor(private devicesService : DevicesService, 
              private devicePageData : DevicePageDataService,
              private router : Router, 
              private notification: MatSnackBar,
              public dialog: MatDialog) { }

  radarDevice : RadarDevice

  ngOnInit(): void {

    this.radarDevice = this.devicePageData.radarDevice
    this.devicePageData.radarDeviceSubject.subscribe({
      next : (radarDevice) => {
        this.radarDevice = radarDevice
      }
    })

  }

  public enableRadarDevice()
  {
    this.devicesService.enableRadarDevice(this.radarDevice.device_id).subscribe({
      next : (response) => this.devicePageData.getDevice(this.radarDevice.device_id),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not enable radar device")
    })
  }

  public disableRadarDevice()
  {
    this.devicesService.disableRadarDevice(this.radarDevice.device_id).subscribe({
      next : (response) => this.devicePageData.getDevice(this.radarDevice.device_id),
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
        this.devicesService.deleteRadarDevice(this.radarDevice.device_id).subscribe({
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
        this.devicePageData.getDevice(this.radarDevice.device_id)
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
        this.devicePageData.getDevice(this.radarDevice.device_id)
      }
    });    
  }

  public setTracksReports(sendReports : boolean)
  {
    this.devicesService.setTracksReports(this.radarDevice.device_id, sendReports).subscribe({
      next : (response) => this.devicePageData.getDevice(this.radarDevice.device_id),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not enable radar device")
    })    
  }

  private showNotification(message : string)
  {
    this.notification.open(message, "Close", { duration : 4000 })
  }

  public isRadarRecordingEnabled()
  {
    let recordService = this.radarDevice.linked_services.find(linkedService => { return linkedService.service_id == "RADAR_RECORDER"})
    return recordService != null;
  }

  public isRadarCalibrated()
  {
    return (this.radarDevice.radar_settings.radar_calibration != "0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0")
  }

  public enableRadarRecording()
  {
    this.devicesService.enableRadarRecording(this.radarDevice.device_id).subscribe({
      next : (response) => this.devicePageData.getDevice(this.radarDevice.device_id),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not enable radar recording")
    })
  }

  public disableRadarRecording()
  {
    this.devicesService.disableRadarRecording(this.radarDevice.device_id).subscribe({
      next : (response) => this.devicePageData.getDevice(this.radarDevice.device_id),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: could not disable radar recording")
    })
  }

}
