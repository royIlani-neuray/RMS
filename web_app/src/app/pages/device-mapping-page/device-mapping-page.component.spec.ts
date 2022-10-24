import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeviceMappingPageComponent } from './device-mapping-page.component';

describe('DeviceMappingPageComponent', () => {
  let component: DeviceMappingPageComponent;
  let fixture: ComponentFixture<DeviceMappingPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DeviceMappingPageComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeviceMappingPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
