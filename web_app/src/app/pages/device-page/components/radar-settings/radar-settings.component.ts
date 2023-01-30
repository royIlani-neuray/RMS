import { Component, OnInit } from '@angular/core';
import { RadarDevice } from 'src/app/entities/radar-device';
import { DevicePageDataService } from '../../device-page-data.service';

@Component({
  selector: 'app-radar-settings',
  templateUrl: './radar-settings.component.html',
  styleUrls: ['./radar-settings.component.css']
})
export class RadarSettingsComponent implements OnInit {

  constructor(private devicePageData : DevicePageDataService) { }

  radarDevice : RadarDevice

  ngOnInit(): void {
    this.radarDevice = this.devicePageData.radarDevice
    this.devicePageData.radarDeviceSubject.subscribe({
      next : (radarDevice) => {
        this.radarDevice = radarDevice
      }
    })
  }
}
