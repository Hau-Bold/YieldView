import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { FredService } from './fred.service';
import { GPSPrice } from '../../Modules/GPSPrice';
import { provideHttpClient, withFetch } from '@angular/common/http';

describe('FredService', () => {
  let service: FredService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
                 FredService,
                 provideHttpClient(withFetch()),   
                 provideHttpClientTesting()        
                 ]
    });
    service = TestBed.inject(FredService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); 
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call the correct URL with given parameters and return expected data', () => {
    const mockData: GPSPrice[] = [
      { date: '2025-01-01', value: 123.45 },
      { date: '2025-02-01', value: 234.56 }
    ];

    const from = '2025-01-01';
    const to = '2025-02-01';

    service.getPrices(from, to).subscribe((data) => {
      expect(data).toEqual(mockData);
    });

    const req = httpMock.expectOne(
      `https://localhost:7031/api/fred/gdp?from=${from}&to=${to}`
    );

    expect(req.request.method).toBe('GET');
    req.flush(mockData); 
  });

  it('should handle empty responses gracefully', () => {
    const from = '2025-01-01';
    const to = '2025-02-01';

    service.getPrices(from, to).subscribe((data) => {
      expect(data.length).toBe(0);
    });

    const req = httpMock.expectOne(
      `https://localhost:7031/api/fred/gdp?from=${from}&to=${to}`
    );

    req.flush([]); 
  });
});
