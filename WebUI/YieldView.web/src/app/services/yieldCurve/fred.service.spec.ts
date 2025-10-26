import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { FredService } from './fred.service';
import { GPSPrice } from '../../Modules/GPSPrice';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { Wilshire5000Price } from '../../Modules/Wilshire5000Price';
import { BuffetIndicator } from '../../Modules/BuffetIndicator';

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

describe('getGDPPrices', () => {
  it('should call the correct URL with given parameters and return expected data', () => {
    const mockData: GPSPrice[] = [
      { date: '2025-01-01', value: 123.45 },
      { date: '2025-02-01', value: 234.56 }
    ];

    const from = '2025-01-01';
    const to = '2025-02-01';

    service.getGDPPrices(from, to).subscribe((data) => {
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

    service.getGDPPrices(from, to).subscribe((data) => {
      expect(data.length).toBe(0);
    });

    const req = httpMock.expectOne(
      `https://localhost:7031/api/fred/gdp?from=${from}&to=${to}`
    );

    req.flush([]); 
  });
});

describe('getW5000Prices', () => {
  it('should call the correct URL with given parameters and return expected data', () => {
    const mockData: Wilshire5000Price[] = [
      { date: '2025-01-01', value: 123.45 ,open: 120.00, high: 125.00, low: 119.00},
      { date: '2025-02-01', value: 234.56,open: 121.00, high: 250.00, low: 120.00 }
    ];

    const from = '2025-01-01';
    const to = '2025-02-01';

    service.getW5000Prices(from, to).subscribe((data) => {
      expect(data).toEqual(mockData);
    });

    const req = httpMock.expectOne(
      `https://localhost:7031/api/fred/w5000?from=${from}&to=${to}`
    );

    expect(req.request.method).toBe('GET');
    req.flush(mockData); 
  });

  it('should handle empty responses gracefully', () => {
    const from = '2025-01-01';
    const to = '2025-02-01';

    service.getW5000Prices(from, to).subscribe((data) => {
      expect(data.length).toBe(0);
    });

    const req = httpMock.expectOne(
      `https://localhost:7031/api/fred/w5000?from=${from}&to=${to}`
    );

    req.flush([]); 
  });
});

describe('getBuffetIndicatorPrices', () => {
  it('should call the correct URL with given parameters and return expected data', () => {
    const mockData: BuffetIndicator[] = [
      { date: '2025-01-01', value: 123.45},
      { date: '2025-02-01', value: 234.56}
    ];

    const from = '2025-01-01';
    const to = '2025-02-01';

    service.getBuffetIndicatorPrices(from, to).subscribe((data) => {
      expect(data).toEqual(mockData);
    });

    const req = httpMock.expectOne(
      `https://localhost:7031/api/fred/buffett-indicator?from=${from}&to=${to}`
    );

    expect(req.request.method).toBe('GET');
    req.flush(mockData); 
  });

  it('should handle empty responses gracefully', () => {
    const from = '2025-01-01';
    const to = '2025-02-01';

    service.getBuffetIndicatorPrices(from, to).subscribe((data) => {
      expect(data.length).toBe(0);
    });

    const req = httpMock.expectOne(
      `https://localhost:7031/api/fred/buffett-indicator?from=${from}&to=${to}`
    );

    req.flush([]); 
  });
});
});
