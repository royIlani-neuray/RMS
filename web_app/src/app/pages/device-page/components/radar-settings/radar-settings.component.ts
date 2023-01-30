import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RadarDevice } from 'src/app/entities/radar-device';
import { DevicesService } from 'src/app/services/devices.service';

@Component({
  selector: 'app-radar-settings',
  templateUrl: './radar-settings.component.html',
  styleUrls: ['./radar-settings.component.css']
})
export class RadarSettingsComponent implements OnInit {

  constructor(private devicesService : DevicesService, 
    private router : Router, 
    private activatedRoute:ActivatedRoute) { }

  radarDevice : RadarDevice
  deviceId : string
  updateTimer : any

  ngOnInit(): void {
    let deviceId = this.activatedRoute.parent!.snapshot.paramMap.get("device_id");

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

}
