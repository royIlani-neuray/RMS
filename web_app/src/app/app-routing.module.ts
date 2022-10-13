import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DevicePageComponent } from './radar-device/pages/device-page/device-page.component';
import { DevicesPageComponent } from './radar-device/pages/devices-page/devices-page.component';

import { HomepageComponent } from './homepage/pages/homepage/homepage.component';
import { ErrorPageNotFoundComponent} from './homepage/pages/error-page-not-found/error-page-not-found.component';
import { ErrorPageNoServiceComponent } from './homepage/pages/error-page-no-service/error-page-no-service.component';
import { TracksViewerComponent } from './radar-device/pages/tracks-viewer/tracks-viewer.component';

const routes: Routes = [
  { path: '', component: HomepageComponent },
  { path: 'device/:device_id', component: DevicePageComponent },
  { path: 'devices', component: DevicesPageComponent },
  { path: 'tracks-viewer', component: TracksViewerComponent},
  { path: 'no-service', component: ErrorPageNoServiceComponent },
  
  //Wild Card Route for 404 request
  { path: '**', pathMatch: 'full', component: ErrorPageNotFoundComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
