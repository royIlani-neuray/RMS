import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SetDeviceConfigDialogComponent } from './set-device-config-dialog.component';

describe('SetDeviceConfigDialogComponent', () => {
  let component: SetDeviceConfigDialogComponent;
  let fixture: ComponentFixture<SetDeviceConfigDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SetDeviceConfigDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SetDeviceConfigDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
