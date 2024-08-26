/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component } from '@angular/core';
import { SettingsService } from './services/settings.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Radar Management Service';

  version : string
  isLoginPage: boolean = false;

  constructor(private settingsService : SettingsService, private router : Router) 
  { 
      // Listen to route changes
      this.router.events.subscribe((event) => {
        // Check if the current route is 'login'
        this.isLoginPage = this.router.url === '/login';
      });
  }

  ngOnInit(): void 
  {
    this.settingsService.getRMSVersion().subscribe({
      next : (result) => {
        this.version = result.version
      }
    })
  }
}
