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
import { forkJoin } from 'rxjs';
import { SP500PriceWithVolatility } from '../Modules/SP500PriceWithVolatility';
import { FredService } from '../services/yieldCurve/fred.service';
import { GPSPrice } from '../Modules/GPSPrice';
import { toISOString } from '../Utils/DateHelper';
import { Wilshire5000Price } from '../Modules/Wilshire5000Price';
import { BuffetIndicator } from '../Modules/BuffetIndicator';

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
  date: string;
 
  // value of dropdown for yieldcurve or fred
  selectedDataset: string = 'yieldCurve';

  // #region states for SP500 Chart
  sp500FromDate:string;
  sp500ToDate:string;
  sp500CurveChart: any;
  // #endregion
  
  // #region states for Yield Curve Chart
  yieldCurveDate: string;
  volatilityWindowSize: number = 10; 
  volatilityThreshold: number = 0.0011;
  yieldCurveChart: any;
  // #endregion

  // #region states for FRED
  selectedFredIndicator: string = 'gdp';
  fredFromDate: string = '1947-1-1';
  fredToDate: string = '';
  fredCurveChart: any;
  selectedPeriod: 'week' | 'month' | 'year' | '10years' = 'month';
  selectedFrequency: 'daily' | 'weekly' | 'monthly' = 'daily';
  // #endregion
  


  constructor(private yieldCurveService: YieldCurveService, private sp500Service: SP500Service, private fredService: FredService)
  {
    const today = new Date();
    this.date = toISOString(today); 

    this.yieldCurveDate = this.date;

    this.sp500ToDate = this.date; 
    this.sp500FromDate = toISOString(new Date(today.getFullYear() - 1, today.getMonth(), today.getDate())); 

    this.fredToDate = this.date; 
    this.fredFromDate =toISOString(new Date(1947, 1, 1)); 
  }

  ngOnInit(): void {
    this.loadYieldCurveChart(this.country,this.date);
    this.loadSp500Chart();
  }

  // #region Chart Event Handlers
  onDatasetChange(event: Event): void {
  const target = event.target as HTMLSelectElement;
  this.selectedDataset = target.value;
 
  if (this.selectedDataset === 'yieldCurve') {
    this.loadYieldCurveChart(this.country,this.date);

    return;
  }

  if(this.selectedDataset==='fred') 
  {
    this.loadFredData();
    return;
  }
}
    onDateChange(event: Event):void {
    const target = event.target as HTMLInputElement;
    this.date = target.value;
    this.loadYieldCurveChart(this.country,this.date);
  }

onSp500FilterChange(source: 'from' | 'to' | 'volatility' | 'treshold' ,event: Event): void {
     const target = event.target as HTMLInputElement;
     const value = target.value;

  switch (source) {
    case 'from':
      this.sp500FromDate = value;
      break;
    case 'to':
      this.sp500ToDate = value;
      break;
    case 'volatility':
       this.volatilityWindowSize = Number(target.value);
    break
    case 'treshold':
       this.volatilityThreshold = Number(target.value);
    break
  }

  if (source === 'from' || source === 'to' || source === 'volatility' || source === 'treshold') {
    this.loadSp500Chart();
  }
}

onFredFilterChange(source: 'from' | 'to' | 'indicator', event: Event): void {

  const target = event.target as HTMLInputElement;
  const value = target.value;

  switch (source) {
    case 'from':
      this.fredFromDate = value;
      break;
    case 'to':
      this.fredToDate = value;
      break;
    case 'indicator':
      this.selectedFredIndicator = value;
      break;
  }

  if (source === 'from' || source === 'to' || source === 'indicator') {
    this.loadFredData();
  }
}

onYieldCurveDateChange(event: Event): void {
  const target = event.target as HTMLInputElement;
  this.date = target.value;          
  this.yieldCurveDate = target.value; 
  this.loadYieldCurveChart(this.country,this.date);
}

// #endregion

  loadYieldCurveChart(country:string,date: string) {
    this.yieldCurveService.getYieldCurve(country, date).subscribe(data => {
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
            borderColor: 'blue',
            fill: false,
            tension: 0,
            spanGaps: true,
            pointRadius: 0,      
            pointHoverRadius: 4, 
            pointBorderWidth: 0
        }
      ]
      },
      options: {
        responsive: true,
         interaction: {
          mode: 'index',
          intersect: false,
        },
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
forkJoin({
  sp500:this.sp500Service.getPricesWithVolatility(this.sp500FromDate,this.sp500ToDate, this.volatilityWindowSize),
  spreads:this.yieldCurveService.getYieldCurveSpread(this.sp500FromDate,this.sp500ToDate, this.country)
})
.subscribe(({sp500,spreads}) => {
  const sp500Labels: string[] = sp500.map(d => d.date.split('T')[0]);
  const sp500Prices: number[] = sp500.map(d => d.close);
  const sp500Vols: number[] = sp500.map(d => d.volatility);
  const sp500Data = sp500;

  const spreadMap = new Map(spreads.map(s => [s.date.split('T')[0], s.spread]));
  const spreadData: (number|null)[]  = sp500Labels.map(d => spreadMap.get(d) ?? null);

  this.createSp500Chart(sp500Labels, sp500Prices,sp500Vols, spreadData, sp500Data);

})
}

createSp500Chart(sp500Labels: string[], sp500Prices: number[], sp500Vols: number[], spreadData: (number | null)[],sp500Data: SP500PriceWithVolatility[]) {
 
  
  this.cleanupSP500Chart();
  
 const ctx = document.getElementById('sp500Chart') as HTMLCanvasElement;
 this.sp500CurveChart = new Chart(ctx, {
    type: 'line',
    data: {
      labels: sp500Labels,
      datasets: [
        {
        label: 'S&P 500 (Closing Price)',
        data: sp500Prices,
        borderColor: 'green',
        segment: {
        borderColor: ctx => {
                             const i = ctx.p0DataIndex; 
                             return sp500Vols[i] < this.volatilityThreshold ? 'green' : 'black';
                            }
                    },
        fill: false,
        tension: 2,
        spanGaps: true,
        pointRadius: 0,   
        pointHoverRadius: 4, 
        pointBorderWidth: 0, 
        yAxisID: 'y'
        },
        {
            label: 'Yield Spread (10Y - 6M)',
            data: spreadData,
            borderColor: 'red',
            spanGaps: true,
            pointRadius: 0,      
            pointHoverRadius: 4, 
            pointBorderWidth: 0,
            yAxisID: 'y1'
        }
    ]
    },
    options:
    {
      responsive: true,
      interaction: {
          mode: 'index',
          intersect: false,
        },
         plugins: {
        tooltip: {
          callbacks: {

            label: (context) => {
                 const idx = context.dataIndex;
                 const dsLabel = context.dataset.label;

                 if (dsLabel === 'S&P 500 (Closing Price)')
                 {
                    const price = context.formattedValue;
                    const vola = sp500Vols[idx];
                    return `Close: ${price}  |  Volatility: ${vola}`;
                 }

                 if (dsLabel === 'Yield Spread (10Y - 6M)') 
                 {
                  const spread = context.formattedValue;
                  return `Yield Spread: ${spread}`;
                 }

              return `${dsLabel}: ${context.formattedValue}`;
            },
          },
        },
      },
      scales: {
        y: {
          type: 'linear',
          display: true,
          position: 'left',
          title: { display: true, text: 'Price (USD)' }
        },
         y1: {
            type: 'linear',
            display: true,
            position: 'right',
            grid: { drawOnChartArea: false },
            title: { display: true, text: 'Yield Spread (%)' }
          },
        x: {
          title: { display: true, text: 'Date' },
          ticks: { maxTicksLimit: 10 } 
        }
      },
      onClick: (evt: any, elements: any[]) => 
        {
               if (elements.length > 0)
               {
                 const index = elements[0].index;
                 const selectedDate = sp500Labels[index];

                 const formattedDate = selectedDate.split('T')[0];

                 if (this.selectedDataset !== 'yieldCurve')
                 {
                  this.selectedDataset = 'yieldCurve';
                 }
               
                 this.yieldCurveDate = selectedDate;
               
                 this.date = formattedDate;
               
                 const datePicker = document.getElementById('yieldCurveDatePicker') as HTMLInputElement;
                 if (datePicker) {
                   datePicker.value = formattedDate;
                 }
               
                 this.loadYieldCurveChart(this.country,formattedDate);
               }
          }
    }
  });
}

loadFredData() {
  // TODO: why is the chart always redered?
  console.log(`Loading FRED data for ${this.selectedFredIndicator} from ${this.fredFromDate} to ${this.fredToDate}`);

   if (this.fredCurveChart) {
    this.fredCurveChart.destroy();
  }

  if(this.selectedFredIndicator==='gdp')
  {
    this.fredService.getGDPPrices(this.fredFromDate, this.fredToDate)
      .subscribe(data => {
        console.log('FRED GDP data received:', data);
        this.renderGDPChart(data);
      });
      return;
   }
   else if(this.selectedFredIndicator==='wilshire5000')
   {
 this.fredService.getW5000Prices(this.fredFromDate, this.fredToDate)
      .subscribe(data => {
        this.renderW5000Chart(data);
      });
   }
  else if(this.selectedFredIndicator==='buffett')
   {
     this.fredService.getBuffetIndicatorPrices(this.fredFromDate, this.fredToDate)
      .subscribe(data => {
        this.renderBuffetIndicatorChart(data);
      });
   }
}

renderGDPChart(data: GPSPrice[]) {
  const labels = data.map(d => d.date.split('T')[0]);
  const values = data.map(d => d.value);

  const ctx = document.getElementById('fredChart') as HTMLCanvasElement;

  this.fredCurveChart = new Chart(ctx, {
    type: 'line',
    data: {
      labels,
      datasets: [{
        label: `${this.selectedFredIndicator.toUpperCase()}`,
        data: values,
        borderColor: 'orange',
        fill: false,
        tension: 0.2,
        pointRadius: 2,
      }]
    },
    options: {
      responsive: true,
      interaction: { mode: 'index', intersect: false },
      scales: {
        y: { title: { display: true, text: 'Value' } },
         x: {
          type: 'time',
          time: { unit: 'week', tooltipFormat: 'yyyy-MM-dd' },
          ticks: { source: 'auto', maxTicksLimit: 10 },
          title: { display: true, text: 'Date' }
        }
      }
    }
  });
}

renderW5000Chart(data: Wilshire5000Price[]) {
  const labels = data.map(d => d.date.split('T')[0]);
  const values = data.map(d => d.value);

  const ctx = document.getElementById('fredChart') as HTMLCanvasElement;
 
  if (this.fredCurveChart) {
    this.fredCurveChart.destroy();
  }

  this.fredCurveChart = new Chart(ctx, {
    type: 'line',
    data: {
      labels,
      datasets: [{
        label: `${this.selectedFredIndicator.toUpperCase()}`,
        data: values,
        borderColor: '#8A2BE2',
        fill: false,
        tension: 0.2,
        pointRadius: 2,
      }]
    },
    options: {
      responsive: true,
      interaction: { mode: 'index', intersect: false },
      scales: {
        y: { title: { display: true, text: 'Value' } },
         x: {
          type: 'time',
          time: { unit: 'week', tooltipFormat: 'yyyy-MM-dd' },
          ticks: { source: 'auto', maxTicksLimit: 10 },
          title: { display: true, text: 'Date' }
        }
      }
    }
  });
}

renderBuffetIndicatorChart(data: BuffetIndicator[]) {
  const labels = data.map(d => d.date.split('T')[0]);
  const values = data.map(d => d.value);

  const ctx = document.getElementById('fredChart') as HTMLCanvasElement;

  if (this.fredCurveChart) {
    this.fredCurveChart.destroy();
  }

  this.fredCurveChart = new Chart(ctx, {
    type: 'line',
    data: {
      labels,
      datasets: [{
        label: `${this.selectedFredIndicator.toUpperCase()}`,
        data: values,
        borderColor: '#FFD700',
        fill: false,
        tension: 0.2,
        pointRadius: 2,
      }]
    },
    options: {
      responsive: true,
      interaction: { mode: 'index', intersect: false },
      scales: {
        y: { title: { display: true, text: 'Value' } },
         x: {
          type: 'time',
          time: { unit: 'week', tooltipFormat: 'yyyy-MM-dd' },
          ticks: { source: 'auto', maxTicksLimit: 10 },
          title: { display: true, text: 'Date' }
        }
      }
    }
  });
}

  private cleanupSP500Chart() {
    if (this.sp500CurveChart) {
      this.sp500CurveChart.destroy();
    }
  }
}
