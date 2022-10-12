import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { RadarDeviceBrief } from 'src/app/entities/radar-device';
import { RadarDeviceModule } from '../../radar-device.module';
import { DevicesService } from '../../services/devices.service';

@Component({
  selector: 'app-devices-page',
  templateUrl: './devices-page.component.html',
  styleUrls: ['./devices-page.component.css']
})
export class DevicesPageComponent implements OnInit {

  deviceList: RadarDeviceBrief[] = [];
  dataSource = new MatTableDataSource<RadarDeviceBrief>(this.deviceList)
  displayedColumns: string[] = ['name', 'state', 'enabled', 'device_id', 'description'];

  constructor(private devicesService:DevicesService) { }

  ngOnInit(): void {
    this.getDeviceList()
  }

  public getDeviceList()
  {
    this.devicesService.getRadarDevices().subscribe(response => this.dataSource.data = response as RadarDeviceBrief[])
  }

}
