import { AfterViewInit, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { DeviceMapping } from 'src/app/entities/radar-device';
import { DevicesService } from '../../services/devices.service';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { SetNetworkDialogComponent } from '../../components/set-network-dialog/set-network-dialog.component';

@Component({
  selector: 'app-device-mapping-page',
  templateUrl: './device-mapping-page.component.html',
  styleUrls: ['./device-mapping-page.component.css']
})
export class DeviceMappingPageComponent implements OnInit {

  deviceListLoaded = new Subject<boolean>();
  deviceList: DeviceMapping[] = [];
  dataSource = new MatTableDataSource<DeviceMapping>()
  displayedColumns: string[] = ['device_id', 'registered', 'ip', 'subnet', 'gateway', 'model', 'application', 'static_ip', 'set_network'];
  updateTimer : any

  constructor(private devicesService : DevicesService, private router : Router, 
              private notification: MatSnackBar, public dialog: MatDialog) { }

  ngOnInit(): void 
  {
    this.deviceListLoaded.next(false);
    
    this.getDeviceMapping()

    // trigger periodic update
    this.updateTimer = setInterval(() => 
    {
      this.getDeviceMapping()
    }, 3000)
  }

  ngOnDestroy() 
  {
    if (this.updateTimer) 
    {
      clearInterval(this.updateTimer);
    }
  }

  public getDeviceMapping()
  {
    this.devicesService.getDeviceMapping().subscribe({
      next : (response) => 
      {
        this.deviceList = response as DeviceMapping[]
        this.dataSource.data = this.deviceList
        this.deviceListLoaded.next(true);
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

  public setNetworkClicked(deviceMapping : DeviceMapping)
  {
    let dialogRef = this.dialog.open(SetNetworkDialogComponent, {
      width: '750px',
      height: '630px',
      data: { targetDevice: deviceMapping }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result == null)
        return

      let resultData = result as DeviceMapping
      this.devicesService.setNetwork(deviceMapping.device_id, resultData.ip, resultData.subnet, resultData.gateway, resultData.static_ip).subscribe({
        next : (resp) => this.notification.open("Set-Network request sent!", "Close", { duration : 2500, horizontalPosition : 'right', verticalPosition : 'top' }),
        error: (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.notification.open("Error: Set-Network failed", "Close", { duration : 4000 })
      })
    });
  }
}
