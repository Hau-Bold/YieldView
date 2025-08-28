import { Component, OnInit, Input } from '@angular/core';
import { YieldCurveService } from '../services/yield-curve.service';
import { Chart, registerables } from 'chart.js';
import { YieldCurvePoint } from '../Modules/YieldCurvePoint';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { SP500Service } from '../services/sp500.service';
import { SP500Price } from '../Modules/SP500Price';
import { forkJoin } from 'rxjs';

Chart.register(...registerables);


@Component({
  selector: 'app-yield-curve-chart',
  templateUrl: './yield-curve-chart.component.html',
  styleUrls: ['./yield-curve-chart.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatDatepickerModule,
    MatInputModule,
    MatNativeDateModule,
  ],
  providers: []
})
export class YieldCurveChartComponent implements OnInit {

  @Input() country = 'US';
  maturityOrder:string[] = ["1M", "1_5M",  "2M", "3M", "4M", "6M", "1Y", "2Y", "3Y", "5Y", "7Y", "10Y", "20Y", "30Y"];
  date: string  ='2025-08-08';
  yieldCurveChart: any;

  sp500FromDate:string = '2025-01-01';
  sp500ToDate:string = '2025-08-08';
  sp500CurveChart: any;
  
  constructor(private yieldCurveService: YieldCurveService, private sp500Service: SP500Service)
   {
    const today = new Date();
    this.date =   today.toISOString().split('T')[0]; 

    this.sp500ToDate = this.date; 
    this.sp500FromDate = new Date(today.getFullYear() - 1, today.getMonth(), today.getDate()).toISOString().split('T')[0]; 
   }

  ngOnInit(): void {
    this.loadDataAndRenderChart(this.date);
     this.loadSp500Chart();
  }

   onDateChange(event: Event):void {
    const target = event.target as HTMLInputElement;
    this.date = target.value;
    this.loadDataAndRenderChart(this.date);
  }

  onSp500FromDateChange(event: Event): void {
  const target = event.target as HTMLInputElement;
  this.sp500FromDate = target.value;
  this.loadSp500Chart();
 }

  onSp500ToDateChange(event: Event): void {
  const target = event.target as HTMLInputElement;
  this.sp500ToDate = target.value;
  this.loadSp500Chart();
}

  loadDataAndRenderChart(date: string) {
    this.yieldCurveService.getYieldCurve(this.country, date).subscribe(data => {
      const sortedData = data.sort(
        (a, b) => this.maturityOrder.indexOf(a.maturity) - this.maturityOrder.indexOf(b.maturity)
      );
      this.createChart(sortedData);
    });
  }

  createChart(data: YieldCurvePoint[]) {
    const labels = data.map(d => d.maturity);
    const yields = data.map(d => d.yield);

    const ctx = document.getElementById('yieldChart') as HTMLCanvasElement;

    if (this.yieldCurveChart) {
      this.yieldCurveChart.destroy();
    }

    this.yieldCurveChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels,
        datasets: [{
          label: `Yield Curve for ${this.country}`,
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

// forkJoin({
//     sp500: this.sp500Service.getHistoricalPrices(this.sp500FromDate, this.sp500ToDate),
//     spreads: this.yieldService.getYieldCurveSpread(this.sp500FromDate, this.sp500ToDate, "US")
//   }).subscribe(({ sp500, spreads }) => {
//     const sp500Labels = sp500.map(d => d.date.split('T')[0]);

//     // SP500 Werte
//     const sp500Data = sp500.map(d => d.close);

//     // Spread Werte (Match nach Datum!)
//     const spreadMap = new Map(spreads.map(s => [s.date.split('T')[0], s.spread]));
//     const spreadData = sp500Labels.map(d => spreadMap.get(d) ?? null);

// TODO:
forkJoin({
  sp500:this.sp500Service.getPrices(this.sp500FromDate,this.sp500ToDate),
  spreads:this.yieldCurveService.getYieldCurveSpread(this.sp500FromDate,this.sp500ToDate, this.country)
})
.subscribe(({sp500,spreads}) => {


})



  this.sp500Service.getPrices(this.sp500FromDate,this.sp500ToDate).subscribe(data => {
    const filtered = data
      .filter(d =>
        new Date(d.date) >= new Date(this.sp500FromDate) &&
        new Date(d.date) <= new Date(this.sp500ToDate)
      )
      .sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());

    this.createSp500Chart(filtered);
  });
}

createSp500Chart(data: SP500Price[]) {
  const labels = data.map(d => d.date);
  const values = data.map(d => d.close);

  const ctx = document.getElementById('sp500Chart') as HTMLCanvasElement;

  if(this.sp500CurveChart)
   {
      this.sp500CurveChart.destroy();
   }

 this.sp500CurveChart = new Chart(ctx, {
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
      },
      onClick: (evt: any, elements: any[]) => {
  if (elements.length > 0) {
    const index = elements[0].index;
    const selectedDate = labels[index];

    const formattedDate = selectedDate.split('T')[0];

    this.date = formattedDate;

    const datePicker = document.getElementById('yieldCurveDatePicker') as HTMLInputElement;
    if (datePicker) {
      datePicker.value = formattedDate;
    }

    this.loadDataAndRenderChart(formattedDate);
  }
}

    }
  });
}
}
