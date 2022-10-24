import { Component, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
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

  deviceList: DeviceMapping[] = [];
  templatesList: RadarTemplateBrief[] = [];
  
  radarNameFC = new FormControl('', [Validators.required])
  selectedDeviceFC = new FormControl('', [Validators.required])
  descriptionFC = new FormControl('', [Validators.maxLength(450)])
  selectedTemplateFC = new FormControl('', [Validators.required])

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

  public onAddClicked()
  {
    if (!this.radarNameFC.valid || !this.selectedDeviceFC.valid || !this.selectedTemplateFC.valid)
      return
    
    let name = this.radarNameFC.value!
    let description = this.descriptionFC.value!
    let device_id = this.selectedDeviceFC.value!
    let template_id = this.selectedTemplateFC.value!

    this.devicesService.registerRadarDevice(device_id, name, description, template_id).subscribe({
      next : (response) => this.router.navigate(['/device', device_id]),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.notification.open("Error: could not register the device", "Close", { duration : 4000 })
    })
  }

}
