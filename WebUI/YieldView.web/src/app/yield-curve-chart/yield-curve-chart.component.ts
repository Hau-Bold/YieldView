import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import { YieldCurveService } from '../services/yield-curve.service';
import { Chart, registerables } from 'chart.js';
import { YieldCurvePoint } from '../Modules/YieldCurvePoint';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

Chart.register(...registerables);

const maturityOrder = ["1M", "1_5M",  "2M", "3M", "4M", "6M", "1Y", "2Y", "3Y", "5Y", "7Y", "10Y", "20Y", "30Y"];

@Component({
  selector: 'app-yield-curve-chart',
  templateUrl: './yield-curve-chart.component.html',
  styleUrls: ['./yield-curve-chart.component.css'],
  standalone: true,
  imports: [CommonModule, RouterModule],
})
export class YieldCurveChartComponent implements OnInit, OnChanges {
  @Input() country = 'US';
  @Input() date = '2025-08-08';

  chart: any;

  constructor(private yieldCurveService: YieldCurveService) {}

  ngOnInit(): void {
    this.loadDataAndRenderChart();
  }

 ngOnChanges(changes: SimpleChanges): void {
  console.log('Changes detected:', changes);
  if ((changes['country'] && !changes['country'].firstChange) ||
      (changes['date'] && !changes['date'].firstChange)) {
    this.loadDataAndRenderChart();
  }
}

  loadDataAndRenderChart() {
    this.yieldCurveService.getYieldCurve(this.country, this.date).subscribe(data => {
      const sortedData = data.sort(
        (a, b) => maturityOrder.indexOf(a.maturity) - maturityOrder.indexOf(b.maturity)
      );
      this.createChart(sortedData);
    });
  }

  createChart(data: YieldCurvePoint[]) {
    const labels = data.map(d => d.maturity);
    const yields = data.map(d => d.yield);

    const ctx = document.getElementById('yieldChart') as HTMLCanvasElement;

    if (this.chart) {
      this.chart.destroy();
    }

    this.chart = new Chart(ctx, {
      type: 'line',
      data: {
        labels,
        datasets: [{
          label: `Yield Curve for ${this.country} on ${this.date}`,
          data: yields,
          fill: false,
          borderColor: 'blue',
          tension: 0.1
        }]
      },
      options: {
        responsive: true,
        scales: {
          y: {
            title: { display: true, text: 'Yield (%)' }
          },
          x: {
            title: { display: true, text: 'Maturity' }
          }
        }
      }
    });
  }
}
