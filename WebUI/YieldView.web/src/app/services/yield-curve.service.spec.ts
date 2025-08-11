import { TestBed } from '@angular/core/testing';

import { YieldCurveService } from './yield-curve.service';

describe('YieldCurveService', () => {
  let service: YieldCurveService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(YieldCurveService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
