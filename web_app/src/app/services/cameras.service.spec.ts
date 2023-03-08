/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { TestBed } from '@angular/core/testing';

import { CamerasService } from './cameras.service';

describe('CamerasService', () => {
  let service: CamerasService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CamerasService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});