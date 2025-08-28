import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { YieldCurvePoint } from '../Modules/YieldCurvePoint';
import { YieldSpread } from '../Modules/YieldSpread';


@Injectable({
  providedIn: 'root'
})
export class YieldCurveService {
  private baseUrl = 'https://localhost:7031/api/yieldcurve';

  constructor(private http: HttpClient) {}

  getYieldCurve(country: string, date: string): Observable<YieldCurvePoint[]> {
    return this.http.get<YieldCurvePoint[]>(`${this.baseUrl}/${country}/${date}`);
  }

  getYieldCurveSpread(from: string, to: string,country: string): Observable<YieldSpread[]> {
    return this.http.get<YieldSpread[]>(`${this.baseUrl}?from=${from}&to=${to}&country=${country}`);
  }
}

