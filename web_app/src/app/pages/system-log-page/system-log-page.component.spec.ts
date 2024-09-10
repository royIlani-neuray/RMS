import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SystemLogPageComponent } from './system-log-page.component';

describe('SystemLogPageComponent', () => {
  let component: SystemLogPageComponent;
  let fixture: ComponentFixture<SystemLogPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SystemLogPageComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SystemLogPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
