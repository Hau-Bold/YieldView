import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import { YieldCurveService } from '../services/yield-curve.service';
import { Chart, registerables } from 'chart.js';
import { YieldCurvePoint } from '../Modules/YieldCurvePoint';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SP500Service } from '../services/sp500.service';
import { SP500Price } from '../Modules/SP500Price';

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

  constructor(private yieldCurveService: YieldCurveService, private sp500Service: SP500Service) {}

  ngOnInit(): void {
    this.loadDataAndRenderChart();
     this.loadSp500Chart();
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

  loadSp500Chart() {
  this.sp500Service.getPrices().subscribe(data => {
    const sorted = data.sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());
    this.createSp500Chart(sorted);
  });
}

createSp500Chart(data: SP500Price[]) {
  const labels = data.map(d => d.date);
  const values = data.map(d => d.close);

  const ctx = document.getElementById('sp500Chart') as HTMLCanvasElement;

  new Chart(ctx, {
    type: 'line',
    data: {
      labels,
      datasets: [{
        label: 'S&P 500 (Closing Price)',
        data: values,
        borderColor: 'green',
        fill: false,
        tension: 0.1
      }]
    },
    options: {
      responsive: true,
      scales: {
        y: {
          title: { display: true, text: 'Price (USD)' }
        },
        x: {
          title: { display: true, text: 'Date' },
          ticks: { maxTicksLimit: 10 } 
        }
      }
    }
  });
}


}
