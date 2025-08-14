import { Component, OnInit } from '@angular/core';
import { YieldCurveService } from '../services/yield-curve.service';
import { Chart, registerables } from 'chart.js';
import { YieldCurvePoint } from '../Modules/YieldCurvePoint';

Chart.register(...registerables);

const maturityOrder = ["1M", "3M", "6M", "1Y", "2Y", "3Y", "5Y", "7Y", "10Y", "20Y", "30Y"];

@Component({
  selector: 'app-yield-curve-chart',
  templateUrl: './yield-curve-chart.component.html',
  styleUrls: ['./yield-curve-chart.component.css'],
  standalone:false,
})
export class YieldCurveChartComponent implements OnInit {
  country = 'US';
  date = '2025-08-08';

  chart: any;

  constructor(private yieldCurveService: YieldCurveService) {}

  ngOnInit(): void {
    this.loadDataAndRenderChart();
  }

  loadDataAndRenderChart() {
    
    this.yieldCurveService.getYieldCurve(this.country, this.date).subscribe(
      data => {
      this.createChart(
        data.sort(
        (a, b) => maturityOrder.indexOf(a.maturity) - maturityOrder.indexOf(b.maturity)
      ));
      }
    )}

  createChart(data: YieldCurvePoint[]) {

     data.sort(
        (a, b) => maturityOrder.indexOf(a.maturity) - maturityOrder.indexOf(b.maturity)
      )

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
            title: {
              display: true,
              text: 'Yield (%)'
            }
          },
          x: {
            title: {
              display: true,
              text: 'Maturity'
            }
          }
        }
      }
    });
  }
}
