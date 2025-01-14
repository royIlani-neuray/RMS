/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { Radar } from 'src/app/entities/radar';
import { BoundaryBoxParams, SensorPositionParams } from 'src/app/entities/radar-settings';
import { RadarTemplate, RadarTemplateBrief } from 'src/app/entities/radar-template';
import { RadarsService } from 'src/app/services/radars.service';
import { TemplatesService } from 'src/app/services/templates.service';

export interface DialogData {
  radar: Radar
}

@Component({
  selector: 'app-set-device-config-dialog',
  templateUrl: './set-device-config-dialog.component.html',
  styleUrls: ['./set-device-config-dialog.component.css']
})
export class SetDeviceConfigDialogComponent implements OnInit {

  radar: Radar
  templatesList: RadarTemplateBrief[] = [];
  validTemplatesList: RadarTemplateBrief[] = [];
  
  templateFormGroup = this.formBuilder.group({
    selectedTemplateFC: ['', Validators.required],
  });

  radarPositionFormGroup = this.formBuilder.group({
    heightInputFC: ['', Validators.required],
    azimuthInputFC: ['', [Validators.required, Validators.min(-90), Validators.max(90)] ],
    elevationInputFC: ['', [Validators.required, Validators.min(-90), Validators.max(90)] ],
  });

  boundaryBoxFormGroup = this.formBuilder.group({
    xMin: ['', [Validators.required, Validators.min(-300), Validators.max(300)] ],
    xMax: ['', [Validators.required, Validators.min(-300), Validators.max(300)] ],
    yMin: ['', [Validators.required, Validators.min(-300), Validators.max(300)] ],
    yMax: ['', [Validators.required, Validators.min(-300), Validators.max(300)] ],
    zMin: ['', [Validators.required, Validators.min(-300), Validators.max(300)] ],
    zMax: ['', [Validators.required, Validators.min(-300), Validators.max(300)] ],
  });

  staticBoundaryBoxFormGroup = this.formBuilder.group({
    xMin: ['', [Validators.required, Validators.min(-300), Validators.max(300)] ],
    xMax: ['', [Validators.required, Validators.min(-300), Validators.max(300)] ],
    yMin: ['', [Validators.required, Validators.min(-300), Validators.max(300)] ],
    yMax: ['', [Validators.required, Validators.min(-300), Validators.max(300)] ],
    zMin: ['', [Validators.required, Validators.min(-300), Validators.max(300)] ],
    zMax: ['', [Validators.required, Validators.min(-300), Validators.max(300)] ],
  });

  calibrationFormGroup = this.formBuilder.group({
    calibrationInputFC: ['', []]
  });

  constructor(public dialogRef: MatDialogRef<SetDeviceConfigDialogComponent>,
              @Inject(MAT_DIALOG_DATA) public data: DialogData, private cd : ChangeDetectorRef,
              private formBuilder: FormBuilder,
              private router : Router,
              private radarsService : RadarsService,
              private templatesService : TemplatesService) { }

  ngOnInit(): void 
  {
    this.radar = this.data.radar

    this.radarPositionFormGroup.controls.heightInputFC.setValue(this.radar.radar_settings.sensor_position.height.toString())
    this.radarPositionFormGroup.controls.azimuthInputFC.setValue(this.radar.radar_settings.sensor_position.azimuth_tilt.toString())
    this.radarPositionFormGroup.controls.elevationInputFC.setValue(this.radar.radar_settings.sensor_position.elevation_tilt.toString())

    this.boundaryBoxFormGroup.controls.xMin.setValue(this.radar.radar_settings.boundary_box.x_min.toString())
    this.boundaryBoxFormGroup.controls.yMin.setValue(this.radar.radar_settings.boundary_box.y_min.toString())
    this.boundaryBoxFormGroup.controls.zMin.setValue(this.radar.radar_settings.boundary_box.z_min.toString())
    this.boundaryBoxFormGroup.controls.xMax.setValue(this.radar.radar_settings.boundary_box.x_max.toString())
    this.boundaryBoxFormGroup.controls.yMax.setValue(this.radar.radar_settings.boundary_box.y_max.toString())
    this.boundaryBoxFormGroup.controls.zMax.setValue(this.radar.radar_settings.boundary_box.z_max.toString())

    this.staticBoundaryBoxFormGroup.controls.xMin.setValue(this.radar.radar_settings.static_boundary_box.x_min.toString())
    this.staticBoundaryBoxFormGroup.controls.yMin.setValue(this.radar.radar_settings.static_boundary_box.y_min.toString())
    this.staticBoundaryBoxFormGroup.controls.zMin.setValue(this.radar.radar_settings.static_boundary_box.z_min.toString())
    this.staticBoundaryBoxFormGroup.controls.xMax.setValue(this.radar.radar_settings.static_boundary_box.x_max.toString())
    this.staticBoundaryBoxFormGroup.controls.yMax.setValue(this.radar.radar_settings.static_boundary_box.y_max.toString())
    this.staticBoundaryBoxFormGroup.controls.zMax.setValue(this.radar.radar_settings.static_boundary_box.z_max.toString())

    this.calibrationFormGroup.controls.calibrationInputFC.setValue(this.radar.radar_settings.radar_calibration)

    this.getTemplates()
  }

  public getTemplates()
  {
    this.templatesService.getRadarTemplates().subscribe({
      next : (response) =>
      {
        this.templatesList = response as RadarTemplateBrief[]

        if (this.radar.device_mapping == null)
          return

        this.validTemplatesList = this.templatesList.filter((template) => {
          return (this.radar.device_mapping.model.startsWith(template.model)) && (template.application == this.radar.device_mapping.application)
        })

      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  public onSaveClicked()
  {
    if (!this.templateFormGroup.valid || !this.radarPositionFormGroup.valid || 
        !this.boundaryBoxFormGroup.valid || !this.staticBoundaryBoxFormGroup.valid)
       return

    let templateId = this.templateFormGroup.controls.selectedTemplateFC.value!

    let sensorPosition : SensorPositionParams = {
      height : +this.radarPositionFormGroup.controls.heightInputFC.value!,
      azimuth_tilt : +this.radarPositionFormGroup.controls.azimuthInputFC.value!,
      elevation_tilt : +this.radarPositionFormGroup.controls.elevationInputFC.value!
    }

    let boundaryBox : BoundaryBoxParams = {
      x_min: +this.boundaryBoxFormGroup.controls.xMin.value!,
      y_min: +this.boundaryBoxFormGroup.controls.yMin.value!,
      z_min: +this.boundaryBoxFormGroup.controls.zMin.value!,
      x_max: +this.boundaryBoxFormGroup.controls.xMax.value!,
      y_max: +this.boundaryBoxFormGroup.controls.yMax.value!,
      z_max: +this.boundaryBoxFormGroup.controls.zMax.value!,
    }

    let staticBoundaryBox : BoundaryBoxParams = {
      x_min: +this.staticBoundaryBoxFormGroup.controls.xMin.value!,
      y_min: +this.staticBoundaryBoxFormGroup.controls.yMin.value!,
      z_min: +this.staticBoundaryBoxFormGroup.controls.zMin.value!,
      x_max: +this.staticBoundaryBoxFormGroup.controls.xMax.value!,
      y_max: +this.staticBoundaryBoxFormGroup.controls.yMax.value!,
      z_max: +this.staticBoundaryBoxFormGroup.controls.zMax.value!,
    }

    let calibration = this.calibrationFormGroup.controls.calibrationInputFC.value!

    this.radarsService.setRadarConfiguration(this.radar.device_id, templateId, sensorPosition, boundaryBox, staticBoundaryBox, calibration).subscribe({
      next : (response) => this.dialogRef.close(true),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

  public onTemplateSelected(event : any)
  {
    let templateId = event.value

    this.templatesService.getRadarTemplate(templateId).subscribe({
      next : (response) =>
      {
        let template = response as RadarTemplate

        // TODO: set validation for Y max according to the max range...
      },
      error : (err) => this.router.navigate(['/no-service'])
    })

  }

  public onCancelClicked()
  {
    this.dialogRef.close(false);
  }

}
