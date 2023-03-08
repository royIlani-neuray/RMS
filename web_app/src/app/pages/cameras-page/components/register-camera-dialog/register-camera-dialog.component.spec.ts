import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegisterCameraDialogComponent } from './register-camera-dialog.component';

describe('RegisterCameraDialogComponent', () => {
  let component: RegisterCameraDialogComponent;
  let fixture: ComponentFixture<RegisterCameraDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RegisterCameraDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RegisterCameraDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
