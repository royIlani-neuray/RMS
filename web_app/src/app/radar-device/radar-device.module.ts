import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DevicesComponent } from './pages/devices/devices.component';
import { TracksViewerComponent } from './pages/tracks-viewer/tracks-viewer.component';
import { HttpClientModule} from '@angular/common/http'

import {MatTableModule} from '@angular/material/table';

@NgModule({
  declarations: [
    DevicesComponent,
    TracksViewerComponent
  ],
  imports: [
    CommonModule,
    MatTableModule,
    HttpClientModule
  ],
  exports: [
    DevicesComponent,
    TracksViewerComponent
  ]
})
export class RadarDeviceModule { }
