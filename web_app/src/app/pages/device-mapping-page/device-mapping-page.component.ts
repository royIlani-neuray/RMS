/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { AfterViewInit, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { DeviceMapping } from 'src/app/entities/radar';
import { RadarsService } from '../../services/radars.service';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { SetNetworkDialogComponent } from '../../components/set-network-dialog/set-network-dialog.component';
import { RmsEventsService } from 'src/app/services/rms-events.service';

@Component({
  selector: 'app-device-mapping-page',
  templateUrl: './device-mapping-page.component.html',
  styleUrls: ['./device-mapping-page.component.css']
})
export class DeviceMappingPageComponent implements OnInit {

  deviceListLoaded = new Subject<boolean>();
  deviceList: DeviceMapping[] = [];
  dataSource = new MatTableDataSource<DeviceMapping>()
  displayedColumns: string[] = ['device_id', 'registered', 'ip', 'subnet', 'gateway', 'model', 'application', 'fw_version', 'static_ip', 'set_network'];

  constructor(private rmsEventsService : RmsEventsService, 
              private radarsService : RadarsService, 
              private router : Router, 
              private notification: MatSnackBar, 
              public dialog: MatDialog) { }

  ngOnInit(): void 
  {
    this.deviceListLoaded.next(false);
    
    this.getDeviceMapping()

    this.rmsEventsService.deviceMappingUpdatedEvent.subscribe({
      next: () => 
      {
        this.getDeviceMapping()
      }
    })

  }

  public getDeviceMapping()
  {
    this.radarsService.getDeviceMapping().subscribe({
      next : (mappedDevices) => 
      {
        this.deviceList = mappedDevices
        this.dataSource.data = this.deviceList
        this.deviceListLoaded.next(true);
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  public triggerDeviceMapping()
  {
    this.radarsService.triggerDeviceMapping().subscribe({
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
      this.radarsService.setNetwork(deviceMapping.device_id, resultData.ip, resultData.subnet, resultData.gateway, resultData.static_ip).subscribe({
        next : (resp) => this.notification.open("Set-Network request sent!", "Close", { duration : 2500, horizontalPosition : 'right', verticalPosition : 'top' }),
        error: (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.notification.open("Error: Set-Network failed", "Close", { duration : 4000 })
      })
    });
  }
}
