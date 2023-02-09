import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfigScriptDialogComponent } from './config-script-dialog.component';

describe('ConfigScriptDialogComponent', () => {
  let component: ConfigScriptDialogComponent;
  let fixture: ComponentFixture<ConfigScriptDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ConfigScriptDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConfigScriptDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
