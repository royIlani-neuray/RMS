import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RadarDetailsInfoComponent } from './radar-details-info.component';

describe('RadarDetailsInfoComponent', () => {
  let component: RadarDetailsInfoComponent;
  let fixture: ComponentFixture<RadarDetailsInfoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RadarDetailsInfoComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RadarDetailsInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
