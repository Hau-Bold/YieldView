import { TestBed } from '@angular/core/testing';

import { SP500Service } from './sp500.service';

describe('Sp500Service', () => {
  let service: SP500Service;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SP500Service);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
