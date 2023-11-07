import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RadarTrackerWindowComponent } from './radar-tracker-window.component';

describe('RadarTrackerWindowComponent', () => {
  let component: RadarTrackerWindowComponent;
  let fixture: ComponentFixture<RadarTrackerWindowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RadarTrackerWindowComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RadarTrackerWindowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
