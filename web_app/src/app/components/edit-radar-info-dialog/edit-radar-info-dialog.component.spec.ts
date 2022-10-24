import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditRadarInfoDialogComponent } from './edit-radar-info-dialog.component';

describe('EditRadarInfoDialogComponent', () => {
  let component: EditRadarInfoDialogComponent;
  let fixture: ComponentFixture<EditRadarInfoDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EditRadarInfoDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditRadarInfoDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
