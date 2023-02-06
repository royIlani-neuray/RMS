import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TracksViewerPageComponent } from './tracks-viewer-page.component';

describe('TracksViewerPageComponent', () => {
  let component: TracksViewerPageComponent;
  let fixture: ComponentFixture<TracksViewerPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TracksViewerPageComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TracksViewerPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
