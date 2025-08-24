import { TestBed } from '@angular/core/testing';

import { Sp500Service } from './sp500.service';

describe('Sp500Service', () => {
  let service: Sp500Service;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Sp500Service);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
