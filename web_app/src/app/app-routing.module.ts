/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DevicePageComponent } from './pages/device-page/device-page.component';
import { DevicesPageComponent } from './pages/devices-page/devices-page.component';
import { DeviceMappingPageComponent } from './pages/device-mapping-page/device-mapping-page.component';
import { NewDevicePageComponent } from './pages/new-device-page/new-device-page.component';
import { SettingsPageComponent } from './pages/settings-page/settings-page.component';
import { TemplatesPageComponent } from './pages/templates-page/templates-page.component';
import { TemplatePageComponent } from './pages/template-page/template-page.component';
import { RecordingsPageComponent } from './pages/recordings-page/recordings-page.component';
import { TracksViewerPageComponent } from './pages/tracks-viewer-page/tracks-viewer-page.component';

import { ErrorPageNotFoundComponent} from './pages/error-page-not-found/error-page-not-found.component';
import { ErrorPageNoServiceComponent } from './pages/error-page-no-service/error-page-no-service.component';

import { DeviceInfoComponent } from './pages/device-page/components/device-info/device-info.component';
import { RadarSettingsComponent } from './pages/device-page/components/radar-settings/radar-settings.component';

const routes: Routes = [
  { path: '', component: DevicesPageComponent },

  { 
    path: 'device/:device_id', 
    title: 'Devices - RMS | neuRay Labs', 
    component: DevicePageComponent,
    children: [
      {
        path: '',
        redirectTo: 'device-info',
        pathMatch: 'full'
      },
      {
        path: 'device-info',
        component: DeviceInfoComponent
      },
      {
        path: 'radar-settings',
        component: RadarSettingsComponent
      }
    ] 
  },

  { path: 'devices', title: 'Devices - RMS | neuRay Labs', component: DevicesPageComponent },
  { path: 'device-mapping', title: 'Device Mapping - RMS | neuRay Labs', component: DeviceMappingPageComponent },
  { path: 'new-device', title: 'Register Device - RMS | neuRay Labs', component: NewDevicePageComponent },

  { path: 'templates', title: 'Templates - RMS | neuRay Labs', component: TemplatesPageComponent},
  { path: 'template/:template_id', title: 'Templates - RMS | neuRay Labs', component: TemplatePageComponent },

  { path: 'recordings', title: 'Recordings - RMS | neuRay Labs', component: RecordingsPageComponent},
  { path: 'tracks-viewer', title: 'Tracks Viewer - RMS | neuRay Labs', component: TracksViewerPageComponent},
  { path: 'settings', title: 'Settings - RMS | neuRay Labs', component: SettingsPageComponent },
  { path: 'no-service', component: ErrorPageNoServiceComponent },
  
  //Wild Card Route for 404 request
  { path: '**', pathMatch: 'full', title: '404 - RMS | neuRay Labs', component: ErrorPageNotFoundComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
