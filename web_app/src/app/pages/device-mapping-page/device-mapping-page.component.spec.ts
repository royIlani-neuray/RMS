/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeviceMappingPageComponent } from './device-mapping-page.component';

describe('DeviceMappingPageComponent', () => {
  let component: DeviceMappingPageComponent;
  let fixture: ComponentFixture<DeviceMappingPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DeviceMappingPageComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeviceMappingPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
