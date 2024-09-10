import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RadarLogComponent } from './radar-log.component';

describe('RadarLogComponent', () => {
  let component: RadarLogComponent;
  let fixture: ComponentFixture<RadarLogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RadarLogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RadarLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
