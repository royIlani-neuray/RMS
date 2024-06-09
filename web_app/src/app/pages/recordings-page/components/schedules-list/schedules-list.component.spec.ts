/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SchedulesListComponent } from './schedules-list.component';

describe('SchedulesListComponent', () => {
  let component: SchedulesListComponent;
  let fixture: ComponentFixture<SchedulesListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SchedulesListComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SchedulesListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
