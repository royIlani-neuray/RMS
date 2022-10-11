import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import {MatToolbarModule} from '@angular/material/toolbar';
import {MatButtonModule} from '@angular/material/button';


import { HomepageModule } from './homepage/homepage.module';
import { RadarDeviceModule } from './radar-device/radar-device.module';


@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    MatToolbarModule,
    MatButtonModule,

    HomepageModule,
    RadarDeviceModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
