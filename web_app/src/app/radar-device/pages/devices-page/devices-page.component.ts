import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { RadarDeviceBrief } from 'src/app/entities/radar-device';
import { DevicesService } from '../../services/devices.service';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-devices-page',
  templateUrl: './devices-page.component.html',
  styleUrls: ['./devices-page.component.css']
})
export class DevicesPageComponent implements OnInit {

  deviceListLoaded = new Subject<boolean>();
  deviceList: RadarDeviceBrief[] = [];
  dataSource = new MatTableDataSource<RadarDeviceBrief>(this.deviceList)
  displayedColumns: string[] = ['name', 'state', 'enabled', 'device_id', 'description'];

  constructor(private devicesService : DevicesService, private router : Router) { }

  ngOnInit(): void 
  {
    this.deviceListLoaded.next(false);
    
    setInterval(() => 
    {
      this.getDeviceList()
    }, 3000)
  }

  public getDeviceList()
  {
    this.devicesService.getRadarDevices().subscribe({
      next : (response) => 
      {
        this.deviceListLoaded.next(true);
        this.dataSource.data = response as RadarDeviceBrief[]
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

}
