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
import {MatChipsModule} from '@angular/material/chips';
import {MatSelectModule} from '@angular/material/select';
import {MatSnackBarModule} from '@angular/material/snack-bar';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatDividerModule} from '@angular/material/divider';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatSidenavModule} from '@angular/material/sidenav';
import {MatDialogModule} from '@angular/material/dialog';
import {MatRadioModule} from '@angular/material/radio';
import {MatSlideToggleModule} from '@angular/material/slide-toggle';
import {MatCardModule} from '@angular/material/card';
import {MatCheckboxModule} from '@angular/material/checkbox';


import { DevicePageComponent } from './pages/device-page/device-page.component';
import { DevicesPageComponent } from './pages/devices-page/devices-page.component';
import { DeviceMappingPageComponent } from './pages/device-mapping-page/device-mapping-page.component';
import { NewDevicePageComponent } from './pages/new-device-page/new-device-page.component';
import { SetNetworkDialogComponent } from './components/set-network-dialog/set-network-dialog.component';
import { EditRadarInfoDialogComponent } from './components/edit-radar-info-dialog/edit-radar-info-dialog.component';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { TracksViewerComponent } from './pages/tracks-viewer/tracks-viewer.component';
import { ErrorPageNotFoundComponent } from './pages/error-page-not-found/error-page-not-found.component';
import { ErrorPageNoServiceComponent } from './pages/error-page-no-service/error-page-no-service.component';
import { SettingsPageComponent } from './pages/settings-page/settings-page.component';
import { TemplatesPageComponent } from './pages/templates-page/templates-page.component';
import { SetDeviceConfigDialogComponent } from './components/set-device-config-dialog/set-device-config-dialog.component';
import { TemplatePageComponent } from './pages/template-page/template-page.component';


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
    TracksViewerComponent,
    SetNetworkDialogComponent,
    EditRadarInfoDialogComponent,
    ConfirmDialogComponent,
    TemplatesPageComponent,
    SetDeviceConfigDialogComponent,
    TemplatePageComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,

    MatToolbarModule,
    MatButtonModule,    
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
    MatRadioModule,
    MatSlideToggleModule,
    MatCardModule,
    MatCheckboxModule,

    HttpClientModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
