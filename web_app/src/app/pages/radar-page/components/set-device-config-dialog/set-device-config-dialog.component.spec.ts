/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SetDeviceConfigDialogComponent } from './set-device-config-dialog.component';

describe('SetDeviceConfigDialogComponent', () => {
  let component: SetDeviceConfigDialogComponent;
  let fixture: ComponentFixture<SetDeviceConfigDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SetDeviceConfigDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SetDeviceConfigDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
