import { AfterViewChecked, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { RadarsService } from 'src/app/services/radars.service';
import { RadarPageDataService } from '../../radar-page-data.service';
import { Radar } from 'src/app/entities/radar';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'app-radar-log',
  templateUrl: './radar-log.component.html',
  styleUrls: ['./radar-log.component.css']
})
export class RadarLogComponent implements OnInit, AfterViewChecked  {

  constructor(private radarsService : RadarsService,
              private radarPageData : RadarPageDataService,
              private sanitizer: DomSanitizer
  ) { }
  @ViewChild('logContainer') logContainer!: ElementRef;
  
  radar : Radar
  logContent: any = '';

  ngOnInit(): void {
    this.radar = this.radarPageData.radar

    this.radarsService.getRadarLog(this.radar.device_id).subscribe({
      next : (response) => {
        this.logContent = this.formatLogContent(response)
        this.logContent = this.sanitizer.bypassSecurityTrustHtml(this.logContent) // must be used in order to render colors.
      },
      error : (err) => this.logContent = "Error: failed to get radar log."
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
