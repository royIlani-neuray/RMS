import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DevicePageComponent } from './pages/device-page/device-page.component';
import { DevicesPageComponent } from './pages/devices-page/devices-page.component';
import { DeviceMappingPageComponent } from './pages/device-mapping-page/device-mapping-page.component';
import { NewDevicePageComponent } from './pages/new-device-page/new-device-page.component';
import { SettingsPageComponent } from './pages/settings-page/settings-page.component';
import { TemplatesPageComponent } from './pages/templates-page/templates-page.component';

import { ErrorPageNotFoundComponent} from './pages/error-page-not-found/error-page-not-found.component';
import { ErrorPageNoServiceComponent } from './pages/error-page-no-service/error-page-no-service.component';
import { TracksViewerComponent } from './pages/tracks-viewer/tracks-viewer.component';

const routes: Routes = [
  { path: '', component: DevicesPageComponent },

  { path: 'device/:device_id', component: DevicePageComponent },
  { path: 'devices', component: DevicesPageComponent },
  { path: 'device-mapping', component: DeviceMappingPageComponent },
  { path: 'new-device', component: NewDevicePageComponent },

  { path: 'templates', component: TemplatesPageComponent},

  { path: 'tracks-viewer', component: TracksViewerComponent},
  { path: 'settings', component: SettingsPageComponent },
  { path: 'no-service', component: ErrorPageNoServiceComponent },
  
  //Wild Card Route for 404 request
  { path: '**', pathMatch: 'full', component: ErrorPageNotFoundComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
