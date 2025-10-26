import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { GPSPrice } from '../../Modules/GPSPrice';
import { Observable } from 'rxjs';
import { Wilshire5000Price } from '../../Modules/Wilshire5000Price';
import { BuffetIndicator } from '../../Modules/BuffetIndicator';

@Injectable({
  providedIn: 'root'
})
export class FredService {

   private baseUrl = 'https://localhost:7031/api/fred';
  
    constructor(private http: HttpClient) {}
  
    getGDPPrices(from: string, to: string): Observable<GPSPrice[]> {

    return this.http.get<GPSPrice[]>(`${this.baseUrl}/gdp?from=${from}&to=${to}`);
  }

   getW5000Prices(from: string, to: string): Observable<Wilshire5000Price[]> {

    return this.http.get<Wilshire5000Price[]>(`${this.baseUrl}/w5000?from=${from}&to=${to}`);
  }

  getBuffetIndicatorPrices(from: string, to: string): Observable<BuffetIndicator[]> {

    return this.http.get<BuffetIndicator[]>(`${this.baseUrl}/buffett-indicator?from=${from}&to=${to}`);
  }
}
