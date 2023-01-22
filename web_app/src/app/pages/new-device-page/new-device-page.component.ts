/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnInit, QueryList, ViewChildren } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatSlideToggle } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { map } from 'rxjs';
import { DeviceMapping } from 'src/app/entities/radar-device';
import { RadarTemplateBrief } from 'src/app/entities/radar-template';
import { TemplatesService } from 'src/app/services/templates.service';
import { DevicesService } from '../../services/devices.service';

@Component({
  selector: 'app-new-device-page',
  templateUrl: './new-device-page.component.html',
  styleUrls: ['./new-device-page.component.css']
})
export class NewDevicePageComponent implements OnInit {

  @ViewChildren(MatSlideToggle) slideToggleComponents: QueryList<MatSlideToggle>;

  deviceList: DeviceMapping[] = [];
  templatesList: RadarTemplateBrief[] = [];
  validTemplatesList: RadarTemplateBrief[] = [];

  
  radarNameFC = new FormControl('', [Validators.required])
  selectedDeviceFC = new FormControl('', [Validators.required])
  descriptionFC = new FormControl('', [Validators.maxLength(450)])
  selectedTemplateFC = new FormControl('', [Validators.required])
  heightInputFC = new FormControl('', [Validators.required])
  azimuthInputFC = new FormControl('', [Validators.required, Validators.min(-90), Validators.max(90)])
  elevationInputFC = new FormControl('', [Validators.required, Validators.min(-90), Validators.max(90)])
  calibrationInputFC = new FormControl('', [])

  constructor(private devicesService : DevicesService,
              private templatesService : TemplatesService, 
              private router : Router, 
              private notification: MatSnackBar) { }

  ngOnInit(): void 
  {
    this.getDeviceMapping()
    this.getTemplates()
  }

  public getTemplates()
  {
    this.templatesService.getRadarTemplates().subscribe({
      next : (response) =>
      {
        this.templatesList = response as RadarTemplateBrief[]
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  public getDeviceMapping()
  {
    this.devicesService.getDeviceMapping().subscribe({
      next : (response) => 
      {
        let mappedDevices = response as DeviceMapping[]

        // remove registerd devices from the list
        mappedDevices.forEach((mappedDevice, index) => 
        {
          if (mappedDevice.registered)
          {
            mappedDevices.splice(index, 1)
          }    
        })

        this.deviceList = mappedDevices
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  public onDeviceSelected(event : any)
  {
    let selectedDeviceId = event.value
    console.log(this.deviceList.length)
    let deviceInfo = this.deviceList.find((device) => { return device.device_id == selectedDeviceId })
    
    this.validTemplatesList = this.templatesList.filter((template) => {
      return (deviceInfo!.model.startsWith(template.model)) && (template.application == deviceInfo!.application)
    })
    
  }

  public onRegisterClicked()
  {
    if (!this.radarNameFC.valid || !this.selectedDeviceFC.valid || !this.selectedTemplateFC.valid ||
      !this.heightInputFC.valid || !this.azimuthInputFC.valid || !this.elevationInputFC.valid)
      return
    
    let name = this.radarNameFC.value!
    let description = this.descriptionFC.value!
    let deviceId = this.selectedDeviceFC.value!
    let templateId = this.selectedTemplateFC.value!
    let radarEnabled : boolean = this.slideToggleComponents.get(0)!.checked
    let sendTracksReport : boolean = this.slideToggleComponents.get(1)!.checked

    let height : number = +this.heightInputFC.value!
    let azimuthTilt : number = +this.azimuthInputFC.value!
    let elevationTilt : number = +this.elevationInputFC.value!

    let calibration = this.calibrationInputFC.value!

    this.devicesService.registerRadarDevice(deviceId, name, description, templateId, radarEnabled, sendTracksReport,
      height, azimuthTilt, elevationTilt, calibration).subscribe({
      next : (response) => this.router.navigate(['/device', deviceId]),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.notification.open("Error: could not register the device", "Close", { duration : 4000 })
    })
  }

}
