import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RadarDevice } from 'src/app/entities/radar-device';
import { DevicesService } from 'src/app/services/devices.service';
import { DevicePageDataService } from '../../device-page-data.service';

@Component({
  selector: 'app-radar-settings',
  templateUrl: './radar-settings.component.html',
  styleUrls: ['./radar-settings.component.css']
})
export class RadarSettingsComponent implements OnInit {

  constructor(private devicesService : DevicesService, 
              private devicePageData : DevicePageDataService,
              private router : Router, 
              private activatedRoute:ActivatedRoute) { }

  radarDevice : RadarDevice
  deviceId : string
  updateTimer : any

  ngOnInit(): void {
    this.radarDevice = this.devicePageData.radarDevice
    this.devicePageData.radarDeviceSubject.subscribe({
      next : (radarDevice) => {
        this.radarDevice = radarDevice
      }
    })
  }
}
