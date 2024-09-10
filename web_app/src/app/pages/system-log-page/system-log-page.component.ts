import { AfterViewChecked, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { RMSService } from 'src/app/services/rms.service';

@Component({
  selector: 'app-system-log-page',
  templateUrl: './system-log-page.component.html',
  styleUrls: ['./system-log-page.component.css']
})
export class SystemLogPageComponent implements OnInit {
  constructor(private rmsService : RMSService,
              private sanitizer: DomSanitizer
  ) { }
  @ViewChild('logContainer') logContainer!: ElementRef;
  
  logContent: any = '';

  ngOnInit(): void {

    this.rmsService.getSystemLog().subscribe({
      next : (response) => {
        this.logContent = this.formatLogContent(response)
        this.logContent = this.sanitizer.bypassSecurityTrustHtml(this.logContent) // must be used in order to render colors.
      },
      error : (err) => this.logContent = "Error: failed to get system log."
    })
  }

  ngAfterViewChecked() 
  {
    this.scrollToBottom();
  }

  scrollToBottom() 
  {
    const container = this.logContainer.nativeElement;
    container.scrollTop = container.scrollHeight;
  }

  formatLogContent(logContent: string): string {
    return logContent
      .split('\n')
      .map(line => line
        .replace(/\[ERR\]/g, '<span style="color: red; background-color: #333;">[ERR]</span>')
        .replace(/\[WRN\]/g, '<span style="color: yellow; background-color: #333;">[WRN]</span>')
      )
      .join('\n');
  }

}
