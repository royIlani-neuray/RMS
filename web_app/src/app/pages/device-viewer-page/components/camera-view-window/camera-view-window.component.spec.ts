import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CameraViewWindowComponent } from './camera-view-window.component';

describe('CameraViewWindowComponent', () => {
  let component: CameraViewWindowComponent;
  let fixture: ComponentFixture<CameraViewWindowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CameraViewWindowComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CameraViewWindowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
