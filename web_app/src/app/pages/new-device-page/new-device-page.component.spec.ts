import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewDevicePageComponent } from './new-device-page.component';

describe('NewDevicePageComponent', () => {
  let component: NewDevicePageComponent;
  let fixture: ComponentFixture<NewDevicePageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NewDevicePageComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NewDevicePageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
