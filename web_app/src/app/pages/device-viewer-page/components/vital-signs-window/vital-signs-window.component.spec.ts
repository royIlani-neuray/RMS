import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VitalSignsWindowComponent } from './vital-signs-window.component';

describe('VitalSignsWindowComponent', () => {
  let component: VitalSignsWindowComponent;
  let fixture: ComponentFixture<VitalSignsWindowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ VitalSignsWindowComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VitalSignsWindowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
