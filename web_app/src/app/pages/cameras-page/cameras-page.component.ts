import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { CameraBrief } from 'src/app/entities/camera';
import { CamerasService } from 'src/app/services/cameras.service';
import { RmsEventsService } from 'src/app/services/rms-events.service';
import { RegisterCameraDialogComponent } from './components/register-camera-dialog/register-camera-dialog.component';

@Component({
  selector: 'app-cameras-page',
  templateUrl: './cameras-page.component.html',
  styleUrls: ['./cameras-page.component.css']
})
export class CamerasPageComponent implements OnInit {

  camerasListLoaded = new Subject<boolean>();
  dataSource = new MatTableDataSource<CameraBrief>()
  displayedColumns: string[] = ['name', 'state', 'enabled', 'device_id', 'description'];

  constructor(private camerasService : CamerasService, 
              private rmsEventsService : RmsEventsService,
              public dialog: MatDialog,
              private router : Router) { }

  ngOnInit(): void 
  {
    this.camerasListLoaded.next(false);
    
    this.getCamerasList()

    this.rmsEventsService.cameraUpdatedEvent.subscribe({
      next: (cameraId) => 
      {
        this.getCamerasList()
      }
    })

    this.rmsEventsService.cameraAddedEvent.subscribe({
      next: (cameraId) => 
      {
        this.getCamerasList()
      }
    })

    this.rmsEventsService.cameraDeletedEvent.subscribe({
      next: (cameraId) => 
      {
        this.getCamerasList()
      }
    })

  }

  public getCamerasList()
  {
    this.camerasService.getCameras().subscribe({
      next : (cameras) => 
      {
        this.dataSource.data = cameras
        this.camerasListLoaded.next(true);
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  public registerCameraClicked()
  {
    let dialogRef = this.dialog.open(RegisterCameraDialogComponent, {
      width: '850px',
      height: '720px',
    });

    dialogRef.afterClosed().subscribe(result => {
      /*
      if (result)
      {
        this.router.navigate(['/no-service']
      }
      */
    });
  }
}