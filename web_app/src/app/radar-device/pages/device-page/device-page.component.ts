import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RadarDevice } from 'src/app/entities/radar-device';
import { DevicesService } from '../../services/devices.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-device-page',
  templateUrl: './device-page.component.html',
  styleUrls: ['./device-page.component.css']
})
export class DevicePageComponent implements OnInit {

  constructor(private devicesService : DevicesService, 
              private router : Router, 
              private activatedRoute:ActivatedRoute,
              private notification: MatSnackBar) { }

  radarDevice : RadarDevice
  deviceId : string

  ngOnInit(): void {
    let deviceId = this.activatedRoute.snapshot.paramMap.get("device_id");

    if (deviceId == null)
    {
      this.router.navigate(['/error-404'])
      return
    }

    this.deviceId = deviceId

    setInterval(() => 
    {
      this.getDevice(this.deviceId)
    }, 3000)
    
  }

  public getDevice(deviceId : string)
  {
    this.devicesService.getRadarDevice(deviceId).subscribe({
      next : (response) => this.radarDevice = response as RadarDevice,
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

  private showNotification(message : string)
  {
    this.notification.open(message, "Close", { duration : 4000 })
  }

}
