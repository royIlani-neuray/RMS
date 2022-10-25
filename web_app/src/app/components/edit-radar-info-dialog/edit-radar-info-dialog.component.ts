import { ChangeDetectorRef, Component, Inject, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatInput } from '@angular/material/input';
import { MatSlideToggle } from '@angular/material/slide-toggle';
import { Router } from '@angular/router';
import { RadarDevice } from 'src/app/entities/radar-device';
import { DevicesService } from '../../services/devices.service';

export interface DialogData {
  radarDevice: RadarDevice
}

@Component({
  selector: 'app-edit-radar-info-dialog',
  templateUrl: './edit-radar-info-dialog.component.html',
  styleUrls: ['./edit-radar-info-dialog.component.css']
})
export class EditRadarInfoDialogComponent implements OnInit {

  @ViewChildren(MatInput) inputComponents: QueryList<MatInput>;
  
  radarDevice: RadarDevice

  radarNameFC: FormControl
  sendTracksReportsFC: FormControl
  
  constructor(public dialogRef: MatDialogRef<EditRadarInfoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData, private cd : ChangeDetectorRef,
    private router : Router,
    private devicesService : DevicesService) { }

  ngOnInit(): void 
  {
    this.radarDevice = this.data.radarDevice
    this.radarNameFC = new FormControl(this.radarDevice.name, [Validators.required])
    this.sendTracksReportsFC = new FormControl(this.radarDevice.send_tracks_report, [Validators.required])
  }

  private getNameInput() {
    return this.inputComponents.get(0)!
  }

  private getDescriptionInput() {
    return this.inputComponents.get(1)!
  }


  onSaveClicked() 
  {
    if (!this.radarNameFC.valid)
      return

    let name = this.getNameInput().value
    let description = this.getDescriptionInput().value
    let sendTracksReport = this.sendTracksReportsFC.value

    this.devicesService.updateRadarInfo(this.radarDevice.device_id, name, description, sendTracksReport).subscribe({
      next : (response) => this.dialogRef.close(true),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

  onCancelClicked() 
  {
    this.dialogRef.close(false);
  }

}