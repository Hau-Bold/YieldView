import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SP500Price } from '../Modules/SP500Price';
import { SP500PriceWithVolatility } from '../Modules/SP500PriceWithVolatility';

@Injectable({
  providedIn: 'root'
})
export class SP500Service {
  private baseUrl = 'https://localhost:7031/api/sp500';

  constructor(private http: HttpClient) {}

  getPrices(from: string, to: string): Observable<SP500Price[]> {
  return this.http.get<SP500Price[]>(`${this.baseUrl}?from=${from}&to=${to}`);
}

getPricesWithVolatility(from: string, to: string, volatilityWindowSize: number, eps: number) {
  return this.http.get<SP500PriceWithVolatility[]>(
    `${this.baseUrl}/volatility?from=${from}&to=${to}&dataInterval=${volatilityWindowSize}&eps=${eps}`
  );
}

}
