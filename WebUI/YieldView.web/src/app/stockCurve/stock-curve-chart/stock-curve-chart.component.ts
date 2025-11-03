import { Component, OnInit, Input } from '@angular/core';
import { Chart, registerables } from 'chart.js';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { StockService } from '../../services/stockCurve/stock.service';
import { PlateauService } from '../../services/stockCurve/plateau.service';
import { Plateau } from '../../Modules/Plateau';
import { StockPrice } from '../../Modules/StockPrice';

import 'chartjs-chart-financial';
import { CandlestickController, CandlestickElement } from 'chartjs-chart-financial';
import 'chartjs-adapter-date-fns'; 
import { groupCandlesByMonth, groupCandlesByYear, toISOString, getIsoWeekNumber } from '../../Utils/DateHelper';
import { Candle } from '../../Modules/Candle';

Chart.register(...registerables, CandlestickController, CandlestickElement);

@Component({
  selector: 'app-stock-curve-chart',
  templateUrl: './stock-curve-chart.component.html',
  styleUrls: ['./stock-curve-chart.component.css'],
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
})
export class StockCurveChartComponent implements OnInit {

  @Input() from: string;
  to: string;
  selectedStock: string;
  stockCurveChart: any;
  showAveragedMean = false; 
  showPlateaus = false;
  minPlateauLength = 5;
  showCandels = false;
  showVolumes = false;
  selectedPeriod: 'week' | 'month' | 'year' | '10years' = 'year';
  selectedFrequency: 'daily' | 'weekly' | 'monthly' = 'daily';

  constructor(private stockService: StockService, private plateauService: PlateauService) {
    const today = new Date();
    this.to = toISOString(today);
    this.from = toISOString(new Date(today.getFullYear() - 1, today.getMonth(), today.getDate()));
    this.selectedStock = 'bidu';
  }

  ngOnInit(): void {
    this.loadAndRenderStockCurveChart(this.selectedStock, this.from, this.to);
  }

  // #region Chart Event Handlers
   onMinPlateauLengthChange(event: Event): void {
      const target = event.target as HTMLInputElement;
      this.minPlateauLength = parseInt(target.value, 10) || 5;
      this.loadAndRenderStockCurveChart(this.selectedStock,this.from,this.to);
    }

   onDateFilterChange(source: 'from'| 'to',event: Event): void {
     const target = event.target as HTMLInputElement; 

    switch(source)
    {
      case 'from':
         this.from = target.value;
         break;
       case 'to':
         this.to = target.value;
         break;
    }

    this.loadAndRenderStockCurveChart(this.selectedStock,this.from,this.to);
} 


  onStockFilterChange(source: 'period' | 'frequency' | 'stock' , value: string): void {

    switch (source) {
      case 'period':
        this.handlePeriod(value);
        break;

      case 'frequency':
        this.selectedFrequency = value as 'daily' | 'weekly' | 'monthly';
        break;

      case 'stock':
        this.selectedStock = value;
        break;
    }

    this.loadAndRenderStockCurveChart(this.selectedStock, this.from, this.to);
  }


private handlePeriod(value: string): void {
  const now = new Date();
  const from = new Date();

  switch (value) {
    case '10years':
      from.setFullYear(now.getFullYear() - 10);
      break;
    case 'year':
      from.setFullYear(now.getFullYear() - 1);
      break;
    case 'month':
      from.setMonth(now.getMonth() - 1);
      break;
    case 'week':
      from.setDate(now.getDate() - 7);
      break;
  }

  this.from = from.toISOString().split('T')[0];
  this.to = now.toISOString().split('T')[0];

  this.selectedPeriod = value as 'week' | 'month' | 'year' | '10years';
}
// #endregion

 public toggleAveragedMean(): void {
    this.showAveragedMean = !this.showAveragedMean;
    if (this.stockCurveChart) {
      const averagedDataset = this.stockCurveChart.data.datasets.find((d: { label: string; }) => d.label === 'Averaged Mean');
      if (averagedDataset) averagedDataset.hidden = !this.showAveragedMean;
      this.stockCurveChart.update();
    }
  }

  togglePlateaus(): void {
    this.showPlateaus = !this.showPlateaus;

    if(this.showPlateaus === false)
    {
      this.minPlateauLength=5;
    }

    this.loadAndRenderStockCurveChart(this.selectedStock, this.from, this.to);
  }

  toggleCandels() : void {
    this.showCandels = !this.showCandels;
    this.loadAndRenderStockCurveChart(this.selectedStock, this.from, this.to);
  }

  toggleVolumes() : void {
    this.showVolumes = !this.showVolumes;
    this.loadAndRenderStockCurveChart(this.selectedStock, this.from, this.to);
  }

  get averagedMeanButtonLabel() { return this.showAveragedMean ? "Hide Averaged Mean" : "Show Averaged Mean"; }
  get detectPlateausButtonLabel() { return this.showPlateaus ? "Hide Plateaus" : "Detect Plateaus"; }
  get candelsButtonLabel(){return this.showCandels ? "Hide Candels" : "Show Candels";}
  get volumesButtonLabel(){return this.showVolumes ? "Hide Volumes" : "Show Volumes";}

  loadAndRenderStockCurveChart(stock: string, from: string, to: string) {
  this.stockService.getPrices(stock, from, to).subscribe((data: StockPrice[]) => {

    const prices = data.map(d => ({ x: new Date(d.date), y: d.close }));
    const volumes = data.map(d => ({ x: new Date(d.date), y: d.volume }));
    const averagedData = data.map(d => ({ x: new Date(d.date), y: d.averagedClose }));

    let plateauDatasets: any[] = [];
    if (this.showPlateaus) {
      const plateaus: Plateau[] = this.plateauService.detectPlateaus(data, this.minPlateauLength);
      plateauDatasets = this.createPlateauDatasets( plateaus);
    }

    const candles = this.aggregateFrequency(data, this.selectedFrequency);
   
    this.createStockChart(prices, volumes, averagedData, plateauDatasets, candles);
  });
}

private createStockChart(
  prices: { x: Date; y: number }[],
  volumes: { x: Date; y: number }[],
  averagedData: { x: Date; y: number }[],
  plateauDatasets: any[],
  candles:Candle[]
) {
  if (this.stockCurveChart) {
    this.stockCurveChart.destroy();
  }

  const ctx = document.getElementById('stockCurveChart') as HTMLCanvasElement;

  this.stockCurveChart = new Chart(ctx, {
    data: {
      datasets: [
        { label: `Candles ${this.selectedStock}`,  data: candles,  type: 'candlestick',  borderColor: 'black',yAxisID: 'y',parsing: false,  backgroundColor: 'transparent',hidden: !this.showCandels,barPercentage: 0.4, categoryPercentage: 0.1, },
        { label: `Stock Curve for ${this.selectedStock}`, data: prices, type: 'line', borderColor: 'blue', fill: false, tension: 0, pointRadius: 0, spanGaps: true },
        { label: 'Averaged Mean', data: averagedData, type: 'line', borderColor: 'rgba(255, 99, 132, 1)', fill: false, tension: 0, pointRadius: 0, spanGaps: true, hidden: !this.showAveragedMean },
        ...plateauDatasets,
        { label: 'Volume', data: volumes, type: 'bar', backgroundColor: 'rgba(0,0,0,0.2)', borderColor: 'rgba(0,0,0,0.3)', yAxisID: 'y1', hidden: !this.showVolumes, barPercentage: 0.02,  categoryPercentage: 0.6  }
      ]
    },
    options: {
      responsive: true,
      interaction: { mode: 'index', intersect: false },
      scales: {
        y: { title: { display: true, text: 'Close' } },
        y1: { title: { display: true, text: 'Volume' }, type: 'linear', position: 'right', grid: { drawOnChartArea: false } },
        x: {
          type: 'time',
          time: { unit: 'week', tooltipFormat: 'yyyy-MM-dd' },
          ticks: { source: 'auto', maxTicksLimit: 10 },
          title: { display: true, text: 'Date' }
        }
      },
      plugins: {
            tooltip: {
      callbacks: {

labelColor: (context) => {
          if (context.dataset.type === 'candlestick') {
            const candle = context.raw as { o: number; c: number };
            const up = candle.c >= candle.o;
            return { borderColor: up ? 'green' : 'red', backgroundColor: up ? 'green' : 'red' };
          }
          return {
            borderColor: context.dataset.borderColor as string,
            backgroundColor: context.dataset.borderColor as string
          };
        }
      }
    },
         legend: { display: true    }
  }
}
});
}

private createPlateauDatasets(plateaus: Plateau[]) {
  return plateaus.map(p => ({
    label: `val: ${p.value}; Len: ${p.length}`,
    data: [
      { x: p.startDate, y: p.value },
      { x: p.endDate, y: p.value }
    ],
    type: 'line',
    borderColor: 'green',
    borderWidth: 2,
    fill: false,
    pointRadius: 0,
    spanGaps: false,
    yAxisID: 'y'
  }));
}


private aggregateFrequency(data: StockPrice[], selectedFrequency: string): Candle[] {

   const candles: Candle[] = data.map(d => ({
    x: new Date(d.date),
    o: d.open,
    h: d.high,
    l: d.low,
    c: d.close
  }));

 switch (selectedFrequency) {
  case 'daily':
     return candles

  case 'weekly':
    return this.handleWeeklyFrequency(candles);

  case 'monthly':
   return this.handleMonthlyFrequency(candles);

  default:
    throw new Error(`Unsupported frequency: ${selectedFrequency}`);
 }
}

private handleWeeklyFrequency(candles: Candle[]): Candle[] {
  const groupedByYear = groupCandlesByYear(candles);
  const groupedByMonth = groupCandlesByMonth(groupedByYear);

  const result: Candle[] = [];

  for (const [year, months] of Object.entries(groupedByMonth)) {
    for (const [month, monthCandles] of Object.entries(months)) {
      if (monthCandles.length === 0)
        {
           continue;
        }

      const sorted = monthCandles.sort((a, b) => a.x.getTime() - b.x.getTime());

      const groupedWeeks: Record<number, Candle[]> = {};

      for (const c of sorted) {
        const week = getIsoWeekNumber(c.x);
        if (!groupedWeeks[week]) groupedWeeks[week] = [];
        groupedWeeks[week].push(c);
      }

      for (const [week, weekCandles] of Object.entries(groupedWeeks)) {
        if (weekCandles.length === 0) continue;

        const sortedWeek = weekCandles.sort((a, b) => a.x.getTime() - b.x.getTime());
        result.push({
          x: sortedWeek[sortedWeek.length - 1].x, 
          o: sortedWeek[0].o,                     
          h: Math.max(...sortedWeek.map(c => c.h)),
          l: Math.min(...sortedWeek.map(c => c.l)),
          c: sortedWeek[sortedWeek.length - 1].c 
        });
      }
    }
  }

  return result;
}


private handleMonthlyFrequency(candles: Candle[]): Candle[] {
  const groupedByYear = groupCandlesByYear(candles);
  const groupedByMonth = groupCandlesByMonth(groupedByYear);

  const result: Candle[] = [];

  for (const [year, months] of Object.entries(groupedByMonth)) {
    for (const [month, monthCandles] of Object.entries(months)) {
      if (monthCandles.length === 0)
        {
           continue;
        }

      const sorted = monthCandles.sort((a, b) => a.x.getTime() - b.x.getTime());

      result.push({
        x: sorted[sorted.length - 1].x, 
        o: sorted[0].o,                 
        h: Math.max(...sorted.map(c => c.h)),
        l: Math.min(...sorted.map(c => c.l)),
        c: sorted[sorted.length - 1].c 
      });
    }
  }

  return result;
}
}

