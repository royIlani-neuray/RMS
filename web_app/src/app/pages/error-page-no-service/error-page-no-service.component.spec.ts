import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ErrorPageNoServiceComponent } from './error-page-no-service.component';

describe('ErrorPageNoServiceComponent', () => {
  let component: ErrorPageNoServiceComponent;
  let fixture: ComponentFixture<ErrorPageNoServiceComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ErrorPageNoServiceComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ErrorPageNoServiceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
