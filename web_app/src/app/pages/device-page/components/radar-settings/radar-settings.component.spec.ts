import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RadarSettingsComponent } from './radar-settings.component';

describe('RadarSettingsComponent', () => {
  let component: RadarSettingsComponent;
  let fixture: ComponentFixture<RadarSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RadarSettingsComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RadarSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
