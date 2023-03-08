/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RadarsPageComponent } from './radars-page.component';

describe('RadarsPageComponent', () => {
  let component: RadarsPageComponent;
  let fixture: ComponentFixture<RadarsPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RadarsPageComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RadarsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});