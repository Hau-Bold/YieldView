import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { StockPrice } from '../../Modules/StockPrice';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class StockService {

  private baseUrl = 'https://localhost:7031/api/stock/';
  constructor(private http: HttpClient) {}

   getPrices(stock:string, from: string, to: string): Observable<StockPrice[]> 
   {
      return this.http.get<StockPrice[]>(`${this.baseUrl}${stock}?from=${from}&to=${to}`);
   }
}
