/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
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

import { HttpClientModule } from '@angular/common/http'
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

import { DevicePageComponent } from './pages/device-page/device-page.component';
import { DevicesPageComponent } from './pages/devices-page/devices-page.component';
import { DeviceMappingPageComponent } from './pages/device-mapping-page/device-mapping-page.component';
import { NewDevicePageComponent } from './pages/new-device-page/new-device-page.component';
import { SetNetworkDialogComponent } from './components/set-network-dialog/set-network-dialog.component';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { ErrorPageNotFoundComponent } from './pages/error-page-not-found/error-page-not-found.component';
import { ErrorPageNoServiceComponent } from './pages/error-page-no-service/error-page-no-service.component';
import { SettingsPageComponent } from './pages/settings-page/settings-page.component';
import { TemplatesPageComponent } from './pages/templates-page/templates-page.component';
import { SetDeviceConfigDialogComponent } from './pages/device-page/components/set-device-config-dialog/set-device-config-dialog.component';
import { TemplatePageComponent } from './pages/template-page/template-page.component';
import { RecordingsPageComponent } from './pages/recordings-page/recordings-page.component';
import { DeviceInfoComponent } from './pages/device-page/components/device-info/device-info.component';
import { RadarSettingsComponent } from './pages/device-page/components/radar-settings/radar-settings.component';
import { EditRadarInfoDialogComponent } from './pages/device-page/components/edit-radar-info-dialog/edit-radar-info-dialog.component';
import { DynamicWindow, TracksViewerPageComponent } from './pages/tracks-viewer-page/tracks-viewer-page.component';
import { RadarViewWindowComponent } from './pages/tracks-viewer-page/components/radar-view-window/radar-view-window.component';
import { RadarDetailsInfoComponent } from './pages/tracks-viewer-page/components/radar-details-info/radar-details-info.component';

@NgModule({
  declarations: [
    AppComponent,
    ErrorPageNotFoundComponent,
    ErrorPageNoServiceComponent,
    SettingsPageComponent,
    DevicesPageComponent,
    DevicePageComponent,
    DeviceMappingPageComponent,
    NewDevicePageComponent,
    SetNetworkDialogComponent,
    EditRadarInfoDialogComponent,
    ConfirmDialogComponent,
    TemplatesPageComponent,
    SetDeviceConfigDialogComponent,
    TemplatePageComponent,
    RecordingsPageComponent,
    DeviceInfoComponent,
    RadarSettingsComponent,
    TracksViewerPageComponent,
    RadarViewWindowComponent,
    DynamicWindow,
    RadarDetailsInfoComponent
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

    HttpClientModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
