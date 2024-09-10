import { AfterViewChecked, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { RadarsService } from 'src/app/services/radars.service';
import { RadarPageDataService } from '../../radar-page-data.service';
import { Radar } from 'src/app/entities/radar';

@Component({
  selector: 'app-radar-log',
  templateUrl: './radar-log.component.html',
  styleUrls: ['./radar-log.component.css']
})
export class RadarLogComponent implements OnInit, AfterViewChecked  {

  constructor(private radarsService : RadarsService,
              private radarPageData : RadarPageDataService
  ) { }
  @ViewChild('logContainer') logContainer!: ElementRef;
  
  radar : Radar
  logContent: string = '';

  ngOnInit(): void {
    this.radar = this.radarPageData.radar

    this.radarsService.getRadarLog(this.radar.device_id).subscribe({
      next : (response) => this.logContent = response,
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

}
