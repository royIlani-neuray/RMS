import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FanGesturesWindowComponent } from './fan-gestures-window.component';

describe('FanGesturesWindowComponent', () => {
  let component: FanGesturesWindowComponent;
  let fixture: ComponentFixture<FanGesturesWindowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FanGesturesWindowComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FanGesturesWindowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
