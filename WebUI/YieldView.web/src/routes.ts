import { Routes } from '@angular/router';
import { HomeComponent } from './app/home/home.component';
import { YieldCurveChartComponent } from './app/yield-curve-chart/yield-curve-chart.component';

export const routes: Routes = [
   { path: '', component: HomeComponent },
   { path: 'yieldChart', component: YieldCurveChartComponent },
];


