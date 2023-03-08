import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeviceRecorderComponent } from './device-recorder.component';

describe('DeviceRecorderComponent', () => {
  let component: DeviceRecorderComponent;
  let fixture: ComponentFixture<DeviceRecorderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DeviceRecorderComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeviceRecorderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
