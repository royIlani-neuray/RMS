/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { RadarPageComponent } from './pages/radar-page/radar-page.component';
import { RadarsPageComponent } from './pages/radars-page/radars-page.component';
import { DeviceMappingPageComponent } from './pages/device-mapping-page/device-mapping-page.component';
import { RegisterRadarPageComponent } from './pages/register-radar-page/register-radar-page.component';
import { SettingsPageComponent } from './pages/settings-page/settings-page.component';
import { TemplatesPageComponent } from './pages/templates-page/templates-page.component';
import { TemplatePageComponent } from './pages/template-page/template-page.component';
import { RecordingsPageComponent } from './pages/recordings-page/recordings-page.component';
import { TracksViewerPageComponent } from './pages/tracks-viewer-page/tracks-viewer-page.component';

import { ErrorPageNotFoundComponent} from './pages/error-page-not-found/error-page-not-found.component';
import { ErrorPageNoServiceComponent } from './pages/error-page-no-service/error-page-no-service.component';

import { RadarInfoComponent } from './pages/radar-page/components/radar-info/radar-info.component';
import { RadarSettingsComponent } from './pages/radar-page/components/radar-settings/radar-settings.component';
import { RecordingsListComponent } from './pages/recordings-page/components/recordings-list/recordings-list.component';
import { DeviceRecorderComponent } from './pages/recordings-page/components/device-recorder/device-recorder.component';
import { CamerasPageComponent } from './pages/cameras-page/cameras-page.component';
import { CameraPageComponent } from './pages/camera-page/camera-page.component';

const routes: Routes = [
  { path: '', component: RadarsPageComponent },

  { 
    path: 'radar/:radar_id', 
    title: 'Devices - RMS | neuRay Labs', 
    component: RadarPageComponent,
    children: [
      {
        path: '',
        redirectTo: 'radar-info',
        pathMatch: 'full'
      },
      {
        path: 'radar-info',
        component: RadarInfoComponent
      },
      {
        path: 'radar-settings',
        component: RadarSettingsComponent
      }
    ] 
  },

  { path: 'radars', title: 'Radars - RMS | neuRay Labs', component: RadarsPageComponent },

  { path: 'cameras', title: 'Cameras - RMS | neuRay Labs', component: CamerasPageComponent },
  { path: 'camera/:camera_id', title: 'Cameras - RMS | neuRay Labs', component: CameraPageComponent },

  { path: 'device-mapping', title: 'Device Mapping - RMS | neuRay Labs', component: DeviceMappingPageComponent },
  { path: 'register-radar', title: 'Register Radar Device - RMS | neuRay Labs', component: RegisterRadarPageComponent },

  { path: 'templates', title: 'Templates - RMS | neuRay Labs', component: TemplatesPageComponent},
  { path: 'template/:template_id', title: 'Templates - RMS | neuRay Labs', component: TemplatePageComponent },

  { 
    path: 'recordings', 
    title: 'Recordings - RMS | neuRay Labs', 
    component: RecordingsPageComponent,
    children: [
      {
        path: '',
        redirectTo: 'recordings-list',
        pathMatch: 'full'
      },
      {
        path: 'recordings-list',
        component: RecordingsListComponent
      },
      {
        path: 'device-recorder',
        component: DeviceRecorderComponent
      }
    ]
  },
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
