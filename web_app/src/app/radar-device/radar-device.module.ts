import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TracksViewerComponent } from './pages/tracks-viewer/tracks-viewer.component';
import { HttpClientModule} from '@angular/common/http'
import {RouterModule} from '@angular/router';

import {MatTableModule} from '@angular/material/table';
import {MatChipsModule} from '@angular/material/chips';
import {MatSelectModule} from '@angular/material/select';
import {MatButtonModule} from '@angular/material/button';
import {MatSnackBarModule} from '@angular/material/snack-bar';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';

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
    RouterModule,

    MatTableModule,
    MatChipsModule,
    MatSelectModule,
    MatButtonModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  exports: [
    DevicePageComponent,
    DevicesPageComponent,
    TracksViewerComponent
  ]
})
export class RadarDeviceModule { }
