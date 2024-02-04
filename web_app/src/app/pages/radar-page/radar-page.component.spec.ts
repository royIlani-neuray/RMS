/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RadarPageComponent } from './radar-page.component';

describe('RadarPageComponent', () => {
  let component: RadarPageComponent;
  let fixture: ComponentFixture<RadarPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RadarPageComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RadarPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
