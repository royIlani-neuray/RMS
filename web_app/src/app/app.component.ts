/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component } from '@angular/core';
import { SettingsService } from './services/settings.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Radar Management Service';

  version : string

  constructor(private settingsService : SettingsService) { }

  ngOnInit(): void 
  {
    this.settingsService.getRMSVersion().subscribe({
      next : (result) => {
        this.version = result.version
      }
    })
  }
}
