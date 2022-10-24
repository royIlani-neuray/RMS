import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SetNetworkDialogComponent } from './set-network-dialog.component';

describe('SetNetworkDialogComponent', () => {
  let component: SetNetworkDialogComponent;
  let fixture: ComponentFixture<SetNetworkDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SetNetworkDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SetNetworkDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
