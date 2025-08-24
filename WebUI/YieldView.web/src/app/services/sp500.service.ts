import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SP500Price } from '../Modules/SP500Price';

@Injectable({
  providedIn: 'root'
})
export class SP500Service {
  private baseUrl = 'https://localhost:7031/api/sp500';

  constructor(private http: HttpClient) {}

  // getPrices(): Observable<SP500Price[]> {
  //   return this.http.get<SP500Price[]>(this.baseUrl);
  // }

  getPrices(from: string, to: string): Observable<SP500Price[]> {
  return this.http.get<SP500Price[]>(`${this.baseUrl}?from=${from}&to=${to}`);
}
}
