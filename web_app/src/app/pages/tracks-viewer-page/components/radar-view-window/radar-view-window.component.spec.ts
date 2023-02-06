import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RadarViewWindowComponent } from './radar-view-window.component';

describe('RadarViewWindowComponent', () => {
  let component: RadarViewWindowComponent;
  let fixture: ComponentFixture<RadarViewWindowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RadarViewWindowComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RadarViewWindowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
