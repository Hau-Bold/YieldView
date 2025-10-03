import { TestBed } from '@angular/core/testing';

import { PlateauService } from './plateau.service';

describe('PlateauService', () => {
  let service: PlateauService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PlateauService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
