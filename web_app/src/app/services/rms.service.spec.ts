import { TestBed } from '@angular/core/testing';

import { RMSService } from './rms.service';

describe('RmsWebserviceService', () => {
  let service: RMSService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RMSService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
