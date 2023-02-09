import { AfterViewInit, ChangeDetectorRef, Component, Inject, OnInit, ViewChild } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatInput } from '@angular/material/input';
import { RadarDevice } from 'src/app/entities/radar-device';
import { DevicesService } from 'src/app/services/devices.service';

export interface DialogData {
  radarDevice: RadarDevice
}

@Component({
  selector: 'app-config-script-dialog',
  templateUrl: './config-script-dialog.component.html',
  styleUrls: ['./config-script-dialog.component.css']
})
export class ConfigScriptDialogComponent implements OnInit , AfterViewInit{

  @ViewChild(MatInput) textInput: MatInput;

  radarDevice: RadarDevice

  constructor(public dialogRef: MatDialogRef<ConfigScriptDialogComponent>,
              @Inject(MAT_DIALOG_DATA) public data: DialogData,
              private devicesService : DevicesService,
              private cd : ChangeDetectorRef) 
              { }

  ngOnInit(): void 
  {
    this.radarDevice = this.data.radarDevice
  }

  ngAfterViewInit(): void 
  {
    let config = ""
    this.radarDevice.config_script.forEach((line) => { config += line + "\n" })
    this.textInput.value = config
    this.cd.detectChanges()
  }

  public onSaveClicked()
  {
    let configScript = this.textInput.value.split("\n")

    this.devicesService.setDeviceConfigScript(this.radarDevice.device_id, configScript).subscribe({
      next : (response) => this.dialogRef.close(true),
      error : (err) => this.dialogRef.close(false)
    })
  }

  public onCancelClicked()
  {
    this.dialogRef.close(false);
  }
}
