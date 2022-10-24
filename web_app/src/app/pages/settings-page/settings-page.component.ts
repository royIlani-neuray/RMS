import { Component, OnInit, QueryList, ViewChildren } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatInput } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { RMSSettings, SettingsService } from '../../services/settings.service';

@Component({
  selector: 'app-settings-page',
  templateUrl: './settings-page.component.html',
  styleUrls: ['./settings-page.component.css']
})
export class SettingsPageComponent implements OnInit {

  @ViewChildren(MatInput) inputComponents: QueryList<MatInput>;
  
  reportsIntervalFC = new FormControl('', [Validators.min(0), Validators.max(10000)])
  
  URL_REGEXP = /^[A-Za-z][A-Za-z\d.+-]*:\/*(?:\w+(?::\w+)?@)?[^\s/]+(?::\d+)?(?:\/[\w#!:.?+=&%@\-/]*)?$/;
  reportsUrlFC = new FormControl('', [Validators.pattern(this.URL_REGEXP)])
  
  constructor(private settingsService:SettingsService, private router : Router, private notification: MatSnackBar) { }

  private getReportsUrlInput() {
    return this.inputComponents.get(0)!
  }

  private getReportsIntervalInput() {
    return this.inputComponents.get(1)!
  }

  ngOnInit(): void 
  {
    this.settingsService.getSettings().subscribe({
      next : (result) => {
        let settings = result as RMSSettings
        this.getReportsUrlInput().value = settings.reports_url
        this.getReportsIntervalInput().value = settings.reports_interval
      },
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  onSaveClicked()
  {
    if (!this.reportsIntervalFC.valid || !this.reportsUrlFC.valid)
      return

    let reportsInterval: number = +this.getReportsIntervalInput().value;
    let reportsUrl = this.getReportsUrlInput().value

    this.settingsService.updateSettings(reportsInterval, reportsUrl).subscribe({
      next: () => this.notification.open("Settings saved!", "Close", { duration : 2500, horizontalPosition : 'right', verticalPosition : 'top' }),
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.showNotification("Error: save settings failed!")
    })
  }

  private showNotification(message : string)
  {
    this.notification.open(message, "Close", { duration : 4000 })
  }
}
