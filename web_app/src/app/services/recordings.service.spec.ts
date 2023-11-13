/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { TestBed } from '@angular/core/testing';

import { RecordingsService } from './recordings.service';

describe('RecordingsService', () => {
  let service: RecordingsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RecordingsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
