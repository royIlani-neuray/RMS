import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CamerasPageComponent } from './cameras-page.component';

describe('CamerasPageComponent', () => {
  let component: CamerasPageComponent;
  let fixture: ComponentFixture<CamerasPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CamerasPageComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CamerasPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
