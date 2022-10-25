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
  
  radarNameFC = new FormControl('', [Validators.required])
  selectedDeviceFC = new FormControl('', [Validators.required])
  descriptionFC = new FormControl('', [Validators.maxLength(450)])
  selectedTemplateFC = new FormControl('', [Validators.required])
  heightInputFC = new FormControl('', [Validators.required])
  azimuthInputFC = new FormControl('', [Validators.required, Validators.min(-360), Validators.max(360)])
  elevationInputFC = new FormControl('', [Validators.required, Validators.min(-360), Validators.max(360)])

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

    this.devicesService.registerRadarDevice(deviceId, name, description, templateId, radarEnabled, sendTracksReport,
      height, azimuthTilt, elevationTilt).subscribe({
      next : (response) => this.router.navigate(['/device', deviceId]),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.notification.open("Error: could not register the device", "Close", { duration : 4000 })
    })
  }

}
