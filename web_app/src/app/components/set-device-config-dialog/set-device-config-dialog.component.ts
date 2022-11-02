import { ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { RadarDevice } from 'src/app/entities/radar-device';
import { RadarTemplateBrief } from 'src/app/entities/radar-template';
import { DevicesService } from 'src/app/services/devices.service';
import { TemplatesService } from 'src/app/services/templates.service';

export interface DialogData {
  radarDevice: RadarDevice
}

@Component({
  selector: 'app-set-device-config-dialog',
  templateUrl: './set-device-config-dialog.component.html',
  styleUrls: ['./set-device-config-dialog.component.css']
})
export class SetDeviceConfigDialogComponent implements OnInit {

  radarDevice: RadarDevice
  templatesList: RadarTemplateBrief[] = [];
  validTemplatesList: RadarTemplateBrief[] = [];
  
  selectedTemplateFC = new FormControl('', [Validators.required])
  
  constructor(public dialogRef: MatDialogRef<SetDeviceConfigDialogComponent>,
              @Inject(MAT_DIALOG_DATA) public data: DialogData, private cd : ChangeDetectorRef,
              private router : Router,
              private devicesService : DevicesService,
              private templatesService : TemplatesService) { }

  ngOnInit(): void 
  {
    this.radarDevice = this.data.radarDevice

    this.getTemplates()
  }

  public getTemplates()
  {
    this.templatesService.getRadarTemplates().subscribe({
      next : (response) =>
      {
        this.templatesList = response as RadarTemplateBrief[]

        if (this.radarDevice.device_mapping == null)
          return

        this.validTemplatesList = this.templatesList.filter((template) => {
          return (template.model == this.radarDevice.device_mapping.model) && (template.application == this.radarDevice.device_mapping.application)
        })

      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  public onSaveClicked()
  {
    if (!this.selectedTemplateFC.valid)
      return

    let templateId = this.selectedTemplateFC.value!

    this.devicesService.setDeviceConfiguration(this.radarDevice.device_id, templateId).subscribe({
      next : (response) => this.dialogRef.close(true),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

  public onCancelClicked()
  {
    this.dialogRef.close(false);
  }

}
