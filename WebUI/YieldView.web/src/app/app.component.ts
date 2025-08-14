// app.component.ts
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { YieldCurveChartComponent } from './yield-curve-chart/yield-curve-chart.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  standalone: true,
  imports: [
    FormsModule,               // Für [(ngModel)]
    YieldCurveChartComponent,  // Chart-Komponente korrekt einbinden
  ]
})
export class AppComponent {
  country = 'US';
  date = '2025-08-14';

  updateChart() {
  
    console.log('Chart aktualisieren', this.country, this.date);
  }
}
