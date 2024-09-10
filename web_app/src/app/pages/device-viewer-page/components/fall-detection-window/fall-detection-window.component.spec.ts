import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FallDetectionWindowComponent } from './fall-detection-window.component';

describe('FallDetectionWindowComponent', () => {
  let component: FallDetectionWindowComponent;
  let fixture: ComponentFixture<FallDetectionWindowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FallDetectionWindowComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FallDetectionWindowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
