import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { GPSPrice } from '../../Modules/GPSPrice';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FredService {

   private baseUrl = 'https://localhost:7031/api/fred';
  
    constructor(private http: HttpClient) {}
  
    getPrices(from: string, to: string): Observable<GPSPrice[]> {

    return this.http.get<GPSPrice[]>(`${this.baseUrl}/gdp?from=${from}&to=${to}`);
  }
}
