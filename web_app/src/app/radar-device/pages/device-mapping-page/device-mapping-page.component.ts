import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { DeviceMapping } from 'src/app/entities/radar-device';
import { DevicesService } from '../../services/devices.service';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-device-mapping-page',
  templateUrl: './device-mapping-page.component.html',
  styleUrls: ['./device-mapping-page.component.css']
})
export class DeviceMappingPageComponent implements OnInit {

  deviceListLoaded = new Subject<boolean>();
  deviceList: DeviceMapping[] = [];
  dataSource = new MatTableDataSource<DeviceMapping>(this.deviceList)
  displayedColumns: string[] = ['device_id', 'ip', 'subnet', 'gateway', 'model', 'application', 'static_ip'];

  constructor(private devicesService : DevicesService, private router : Router, private notification: MatSnackBar) { }

  ngOnInit(): void 
  {
    this.deviceListLoaded.next(false);
    
    setInterval(() => 
    {
      this.getDeviceMapping()
    }, 3000)
  }

  public getDeviceMapping()
  {
    this.devicesService.getDeviceMapping().subscribe({
      next : (response) => 
      {
        this.deviceListLoaded.next(true);
        this.dataSource.data = response as DeviceMapping[]
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  public triggerDeviceMapping()
  {
    this.devicesService.triggerDeviceMapping().subscribe({
      next : (response) => this.notification.open("Device mapping triggered!", "Close", { duration : 2500, horizontalPosition : 'right', verticalPosition : 'top' }),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.notification.open("Error: could not trigger device mappping.", "Close", { duration : 4000 })
    })
  }
}
