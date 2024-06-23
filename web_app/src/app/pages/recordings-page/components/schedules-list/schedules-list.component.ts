/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { DeviceEmulatorService } from 'src/app/services/device-emulator.service';
import { Router } from '@angular/router';
import { Observable, Subject, combineLatest, forkJoin, map, merge } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from 'src/app/components/confirm-dialog/confirm-dialog.component';
import { MatSort } from '@angular/material/sort';
import { RecordingSchedule } from 'src/app/entities/recording-schedule';
import { RecordingSchedulesService } from 'src/app/services/recording-schedules.service';
import { RenameDialogComponent } from '../rename-dialog/rename-dialog.component';
import { RadarsService } from 'src/app/services/radars.service';
import { CamerasService } from 'src/app/services/cameras.service';
import { RmsEventsService } from 'src/app/services/rms-events.service';
import { RadarBrief } from 'src/app/entities/radar';
import { CameraBrief } from 'src/app/entities/camera';
import { animate, state, style, transition, trigger } from '@angular/animations';


@Component({
  selector: 'app-schedules-list',
  templateUrl: './schedules-list.component.html',
  styleUrls: ['./schedules-list.component.css'],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({height: '0px', minHeight: '0'})),
      state('expanded', style({height: '*'})),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ],
})
export class SchedulesListComponent implements OnInit {

  @ViewChild(MatSort) sort: MatSort;
  
  radars: RadarBrief[] = [];
  cameras: CameraBrief[] = [];
  schedulesListLoaded = new Subject<boolean>();
  radarsListLoaded = new Subject<boolean>();
  camerasListLoaded = new Subject<boolean>();
  dataSource = new MatTableDataSource<RecordingSchedule>()
  displayedColumns: string[] = ['name', 'enabled', 'upload_s3', 'times', 'num_of_devices', 'actions'];
  displayedColumnsWithExpand: string[] = [...this.displayedColumns, 'expand'];
  expandedElement: RecordingSchedule | null;

  constructor(private recordingSchedulesService : RecordingSchedulesService,
              private router : Router, private notification: MatSnackBar,
              private radarsService : RadarsService,
              private camerasService : CamerasService,
              public dialog: MatDialog) { }

  public ngOnInit(): void 
  {
    this.schedulesListLoaded.next(false);
    this.radarsListLoaded.next(false);
    this.camerasListLoaded.next(false);
    this.getSchedulesList();
    this.getRadarsList()
    this.getCamerasList()
  }

  public getRadarsList()
  {
    this.radarsService.getRadars().subscribe({
      next : (radars) => 
      {
        this.radars = radars;
        this.radarsListLoaded.next(true);
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  public getCamerasList()
  {
    this.camerasService.getCameras().subscribe({
      next : (cameras) => 
      {
        this.cameras = cameras;
        this.camerasListLoaded.next(true);
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  public getSchedulesList()
  {
    this.recordingSchedulesService.getSchedules().subscribe({
      next : (schedules) => 
      {
        this.dataSource.data = schedules;
        this.schedulesListLoaded.next(true);
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  public radarNameById(radarId: string)
  {
    return this.radars.find((radar) => radar.device_id == radarId)?.name ?? "Unknown";
  }

  public cameraNameById(cameraId: string)
  {
    return this.cameras.find((camera) => camera.device_id == cameraId)?.name ?? "Unknown";
  }
  
  public deleteSchedule(schedule: RecordingSchedule)
  {
    let dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '360px',
      height: '140px',
      data: { message : "Delete the given schedule?" }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result)
      {
        this.recordingSchedulesService.deleteSchedule(schedule.id).subscribe({
          next : (response) => this.getSchedulesList(),
          error : (err) => this.showNotification("Error: could not delete schedule")
        })
      }
    });
  }

  public renameSchedule(schedule: RecordingSchedule)
  {
    let dialogRef = this.dialog.open(RenameDialogComponent, {
      width: '550px',
      height: '240px',
      data: {
        name: schedule.name
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.recordingSchedulesService.updateSchedule(schedule.id, result).subscribe({
          next : (response) => this.getSchedulesList(),
          error : (err) => this.showNotification("Error: could not rename schedule")
        });
      }
    });
  }

  public toggleEnable(schedule: RecordingSchedule)
  {
    this.recordingSchedulesService.updateSchedule(schedule.id, undefined, !schedule.enabled).subscribe({
      next : (response) => this.getSchedulesList(),
      error : (err) => this.showNotification("Error: could not update schedule")
    });
  }

  public toggleCloud(schedule: RecordingSchedule)
  {
    this.recordingSchedulesService.updateSchedule(schedule.id, undefined, undefined, !schedule.upload_s3).subscribe({
      next : (response) => this.getSchedulesList(),
      error : (err) => this.showNotification("Error: could not update schedule")
    });
  }

  private showNotification(message : string)
  {
    this.notification.open(message, "Close", { duration : 4000 })
  }
}
