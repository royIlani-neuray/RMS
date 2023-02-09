/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateTemplateDialogComponent } from './create-template-dialog.component';

describe('CreateTemplateDialogComponent', () => {
  let component: CreateTemplateDialogComponent;
  let fixture: ComponentFixture<CreateTemplateDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CreateTemplateDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreateTemplateDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
