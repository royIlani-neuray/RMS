/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RadarDetailsInfoComponent } from './radar-details-info.component';

describe('RadarDetailsInfoComponent', () => {
  let component: RadarDetailsInfoComponent;
  let fixture: ComponentFixture<RadarDetailsInfoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RadarDetailsInfoComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RadarDetailsInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
