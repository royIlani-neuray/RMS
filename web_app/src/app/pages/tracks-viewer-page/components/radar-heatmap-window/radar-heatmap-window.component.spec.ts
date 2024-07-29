import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RadarHeatmapWindowComponent } from './radar-heatmap-window.component';

describe('RadarHeatmapWindowComponent', () => {
  let component: RadarHeatmapWindowComponent;
  let fixture: ComponentFixture<RadarHeatmapWindowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RadarHeatmapWindowComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RadarHeatmapWindowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
