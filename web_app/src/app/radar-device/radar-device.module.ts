import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TracksViewerComponent } from './pages/tracks-viewer/tracks-viewer.component';
import { HttpClientModule } from '@angular/common/http'
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms'

import {MatTableModule} from '@angular/material/table';
import {MatChipsModule} from '@angular/material/chips';
import {MatSelectModule} from '@angular/material/select';
import {MatButtonModule} from '@angular/material/button';
import {MatSnackBarModule} from '@angular/material/snack-bar';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatDividerModule} from '@angular/material/divider';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatSidenavModule} from '@angular/material/sidenav';
import {MatDialogModule} from '@angular/material/dialog';
import {MatRadioModule} from '@angular/material/radio';

import { DevicePageComponent } from './pages/device-page/device-page.component';
import { DevicesPageComponent } from './pages/devices-page/devices-page.component';
import { DeviceMappingPageComponent } from './pages/device-mapping-page/device-mapping-page.component';
import { NewDevicePageComponent } from './pages/new-device-page/new-device-page.component';
import { SetNetworkDialogComponent } from './components/set-network-dialog/set-network-dialog.component';

@NgModule({
  declarations: [
    DevicesPageComponent,
    TracksViewerComponent,
    DevicePageComponent,
    DeviceMappingPageComponent,
    NewDevicePageComponent,
    SetNetworkDialogComponent
  ],
  imports: [
    CommonModule,
    HttpClientModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,

    MatTableModule,
    MatChipsModule,
    MatSelectModule,
    MatButtonModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatInputModule,
    MatFormFieldModule,
    MatSidenavModule,
    MatDialogModule,
    MatRadioModule
  ],
  exports: [
    DevicePageComponent,
    DevicesPageComponent,
    DeviceMappingPageComponent,
    NewDevicePageComponent,
    TracksViewerComponent
  ]
})
export class RadarDeviceModule { }
