import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GaitIdWindowComponent } from './gait-id-window.component';

describe('GaitIdWindowComponent', () => {
  let component: GaitIdWindowComponent;
  let fixture: ComponentFixture<GaitIdWindowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GaitIdWindowComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GaitIdWindowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
