import { TestBed } from '@angular/core/testing';

import { RequestHelperServiceTs } from './request-helper.service.ts';

describe('RequestHelperServiceTs', () => {
  let service: RequestHelperServiceTs;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RequestHelperServiceTs);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
