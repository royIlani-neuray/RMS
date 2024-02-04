/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegisterRadarPageComponent } from './register-radar-page.component';

describe('RegisterRadarPageComponent', () => {
  let component: RegisterRadarPageComponent;
  let fixture: ComponentFixture<RegisterRadarPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RegisterRadarPageComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RegisterRadarPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
