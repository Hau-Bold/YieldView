import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { StockService } from './stock.service';
import { StockPrice } from '../../Modules/StockPrice';
import { provideHttpClient, withFetch } from '@angular/common/http';

describe('StockService', () => {
  let sut: StockService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        StockService,
       provideHttpClient(withFetch()),   
      provideHttpClientTesting()        
      ]
    });

    sut = TestBed.inject(StockService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should call the correct URL with query params', () => {
    // ARRANGE
    const mockStock = 'AAPL';
    const from = '2025-01-01';
    const to = '2025-01-10';
    const mockResponse: StockPrice[] = [
      { date: '2025-01-01', open:23, high:20,low:10,  close: 150, volume:10, averagedClose:20.1,gaussianAveragedClose:19.5, plateauIndex:0},
      { date: '2025-01-02', open:23, high:20,low:10,  close: 153, volume:10, averagedClose:20.1,gaussianAveragedClose:19.5, plateauIndex:0}
    ];

    // Act
    sut.getPrices(mockStock, from, to).subscribe((prices) => {
      expect(prices.length).toBe(2);
      expect(prices).toEqual(mockResponse);
    });

    // Assert
    const request = httpMock.expectOne(
      `https://localhost:7031/api/stock/${mockStock}?from=${from}&to=${to}`
    );

    expect(request.request.method).toBe('GET');
    request.flush(mockResponse); 
  });

  it('should handle empty response', () => {
    // Arrange
    const mockStock = 'TSLA';
    const from = '2025-01-01';
    const to = '2025-01-05';

    // Act
    sut.getPrices(mockStock, from, to).subscribe((prices) => {
      expect(prices).toEqual([]);
    });

    // Assert
    const req = httpMock.expectOne(
      `https://localhost:7031/api/stock/${mockStock}?from=${from}&to=${to}`
    );

    req.flush([]);
  });
});
