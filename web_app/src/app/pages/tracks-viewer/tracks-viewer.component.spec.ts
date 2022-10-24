import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TracksViewerComponent } from './tracks-viewer.component';

describe('TracksViewerComponent', () => {
  let component: TracksViewerComponent;
  let fixture: ComponentFixture<TracksViewerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TracksViewerComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TracksViewerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
