/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RadarViewWindowComponent } from './radar-view-window.component';

describe('RadarViewWindowComponent', () => {
  let component: RadarViewWindowComponent;
  let fixture: ComponentFixture<RadarViewWindowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RadarViewWindowComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RadarViewWindowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
