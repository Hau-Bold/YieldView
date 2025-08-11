import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { YieldCurvePoint } from '../Modules/YieldCurvePoint';


@Injectable({
  providedIn: 'root'
})
export class YieldCurveService {
  private baseUrl = 'https://localhost:7031/api/yieldcurve';

  constructor(private http: HttpClient) {}

  getYieldCurve(country: string, date: string): Observable<YieldCurvePoint[]> {
    return this.http.get<YieldCurvePoint[]>(`${this.baseUrl}/${country}/${date}`);
  }
}

