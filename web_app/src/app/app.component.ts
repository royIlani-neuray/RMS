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
