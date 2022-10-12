import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TracksViewerComponent } from './pages/tracks-viewer/tracks-viewer.component';
import { HttpClientModule} from '@angular/common/http'

import {MatTableModule} from '@angular/material/table';
import {MatChipsModule} from '@angular/material/chips';

import { DevicePageComponent } from './pages/device-page/device-page.component';
import { DevicesPageComponent } from './pages/devices-page/devices-page.component';

@NgModule({
  declarations: [
    DevicesPageComponent,
    TracksViewerComponent,
    DevicePageComponent
  ],
  imports: [
    CommonModule,
    HttpClientModule,

    MatTableModule,
    MatChipsModule
  ],
  exports: [
    DevicePageComponent,
    DevicesPageComponent,
    TracksViewerComponent
  ]
})
export class RadarDeviceModule { }
