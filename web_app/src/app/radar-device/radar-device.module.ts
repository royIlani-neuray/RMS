import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DevicesComponent } from './pages/devices/devices.component';
import { TracksViewerComponent } from './pages/tracks-viewer/tracks-viewer.component';



@NgModule({
  declarations: [
    DevicesComponent,
    TracksViewerComponent
  ],
  imports: [
    CommonModule
  ],
  exports: [
    DevicesComponent,
    TracksViewerComponent
  ]
})
export class RadarDeviceModule { }
