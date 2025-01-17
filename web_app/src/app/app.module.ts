/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http'
import { JwtInterceptor } from './security/jwt-interceptor'
import { getJwtToken } from './security/role-guard';
import { JwtModule } from '@auth0/angular-jwt';

import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms'

import {MatToolbarModule} from '@angular/material/toolbar';
import {MatButtonModule} from '@angular/material/button';
import {MatTableModule} from '@angular/material/table';
import {MatSortModule} from '@angular/material/sort';
import {MatChipsModule} from '@angular/material/chips';
import {MatSelectModule} from '@angular/material/select';
import {MatSnackBarModule} from '@angular/material/snack-bar';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatDividerModule} from '@angular/material/divider';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatSidenavModule} from '@angular/material/sidenav';
import {MatExpansionModule} from '@angular/material/expansion';
import {MatListModule} from '@angular/material/list';
import {MatDialogModule} from '@angular/material/dialog';
import {MatRadioModule} from '@angular/material/radio';
import {MatSlideToggleModule} from '@angular/material/slide-toggle';
import {MatCardModule} from '@angular/material/card';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatStepperModule} from '@angular/material/stepper';
import {MatTabsModule} from '@angular/material/tabs';
import {MatGridListModule} from '@angular/material/grid-list';
import {MatMenuModule} from '@angular/material/menu';
import {MatIconModule} from '@angular/material/icon';

import { RadarPageComponent } from './pages/radar-page/radar-page.component';
import { RadarsPageComponent } from './pages/radars-page/radars-page.component';
import { DeviceMappingPageComponent } from './pages/device-mapping-page/device-mapping-page.component';
import { RegisterRadarPageComponent } from './pages/register-radar-page/register-radar-page.component';
import { SetNetworkDialogComponent } from './components/set-network-dialog/set-network-dialog.component';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { ErrorPageNotFoundComponent } from './pages/error-page-not-found/error-page-not-found.component';
import { ErrorPageNoServiceComponent } from './pages/error-page-no-service/error-page-no-service.component';
import { SettingsPageComponent } from './pages/settings-page/settings-page.component';
import { TemplatesPageComponent } from './pages/templates-page/templates-page.component';
import { SetDeviceConfigDialogComponent } from './pages/radar-page/components/set-device-config-dialog/set-device-config-dialog.component';
import { TemplatePageComponent } from './pages/template-page/template-page.component';
import { RecordingsPageComponent } from './pages/recordings-page/recordings-page.component';
import { RadarInfoComponent } from './pages/radar-page/components/radar-info/radar-info.component';
import { RadarSettingsComponent } from './pages/radar-page/components/radar-settings/radar-settings.component';
import { EditRadarInfoDialogComponent } from './pages/radar-page/components/edit-radar-info-dialog/edit-radar-info-dialog.component';
import { DynamicWindow, DeviceViewerPageComponent } from './pages/device-viewer-page/device-viewer-page.component';
import { RadarViewWindowComponent } from './pages/device-viewer-page/components/radar-view-window/radar-view-window.component';
import { RadarDetailsInfoComponent } from './pages/device-viewer-page/components/radar-details-info/radar-details-info.component';
import { ConfigScriptDialogComponent } from './pages/radar-page/components/config-script-dialog/config-script-dialog.component';
import { CreateTemplateDialogComponent } from './pages/templates-page/components/create-template-dialog/create-template-dialog.component';
import { CameraViewWindowComponent } from './pages/device-viewer-page/components/camera-view-window/camera-view-window.component';
import { RenameDialogComponent } from './pages/recordings-page/components/rename-dialog/rename-dialog.component';
import { RecordingsListComponent } from './pages/recordings-page/components/recordings-list/recordings-list.component';
import { DeviceRecorderComponent } from './pages/recordings-page/components/device-recorder/device-recorder.component';
import { CamerasPageComponent } from './pages/cameras-page/cameras-page.component';
import { CameraPageComponent } from './pages/camera-page/camera-page.component';
import { RegisterCameraDialogComponent } from './pages/cameras-page/components/register-camera-dialog/register-camera-dialog.component';
import { GaitIdWindowComponent } from './pages/device-viewer-page/components/gait-id-window/gait-id-window.component';
import { FallDetectionWindowComponent } from './pages/device-viewer-page/components/fall-detection-window/fall-detection-window.component';
import { FanGesturesWindowComponent } from './pages/device-viewer-page/components/fan-gestures-window/fan-gestures-window.component';
import { RadarTrackerWindowComponent } from './pages/device-viewer-page/components/radar-tracker-window/radar-tracker-window.component';
import { VitalSignsWindowComponent } from './pages/device-viewer-page/components/vital-signs-window/vital-signs-window.component';
import { SchedulesListComponent } from './pages/recordings-page/components/schedules-list/schedules-list.component';
import { DaysOfWeekComponent } from './components/days-of-week/days-of-week.component';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RadarHeatmapWindowComponent } from './pages/device-viewer-page/components/radar-heatmap-window/radar-heatmap-window.component';
import { LoginPageComponent } from './pages/login-page/login-page.component';
import { RadarLogComponent } from './pages/radar-page/components/radar-log/radar-log.component';
import { SystemLogPageComponent } from './pages/system-log-page/system-log-page.component';

@NgModule({
  declarations: [
    AppComponent,
    ErrorPageNotFoundComponent,
    ErrorPageNoServiceComponent,
    SettingsPageComponent,
    RadarsPageComponent,
    RadarPageComponent,
    DeviceMappingPageComponent,
    RegisterRadarPageComponent,
    SetNetworkDialogComponent,
    EditRadarInfoDialogComponent,
    ConfirmDialogComponent,
    TemplatesPageComponent,
    SetDeviceConfigDialogComponent,
    TemplatePageComponent,
    RecordingsPageComponent,
    RadarInfoComponent,
    RadarSettingsComponent,
    DeviceViewerPageComponent,
    RadarViewWindowComponent,
    DynamicWindow,
    RadarDetailsInfoComponent,
    ConfigScriptDialogComponent,
    CreateTemplateDialogComponent,
    CameraViewWindowComponent,
    RenameDialogComponent,
    RecordingsListComponent,
    SchedulesListComponent,
    DeviceRecorderComponent,
    CamerasPageComponent,
    CameraPageComponent,
    RegisterCameraDialogComponent,
    GaitIdWindowComponent,
    FallDetectionWindowComponent,
    FanGesturesWindowComponent,
    RadarTrackerWindowComponent,
    VitalSignsWindowComponent,
    DaysOfWeekComponent,
    RadarHeatmapWindowComponent,
    LoginPageComponent,
    RadarLogComponent,
    SystemLogPageComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,

    MatToolbarModule,
    MatButtonModule,    
    MatTableModule,
    MatSortModule,
    MatChipsModule,
    MatSelectModule,
    MatButtonModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatInputModule,
    MatFormFieldModule,
    MatSidenavModule,
    MatExpansionModule,
    MatListModule,
    MatDialogModule,
    MatRadioModule,
    MatSlideToggleModule,
    MatCardModule,
    MatCheckboxModule,
    MatStepperModule,
    MatTabsModule,
    MatGridListModule,
    MatMenuModule,
    MatIconModule,
    MatTooltipModule,

    HttpClientModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    JwtModule.forRoot({ config: {tokenGetter: getJwtToken}})
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
