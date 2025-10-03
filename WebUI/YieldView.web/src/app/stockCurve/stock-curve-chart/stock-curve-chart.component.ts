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

  constructor(private stockService: StockService, private plateauService: PlateauService) {
    const today = new Date();
    this.to = today.toISOString().split('T')[0];
    this.from = new Date(today.getFullYear() - 1, today.getMonth(), today.getDate()).toISOString().split('T')[0];
    this.selectedStock = 'bidu';
  }

  ngOnInit(): void {
    this.loadAndRenderStockCurveChart(this.selectedStock, this.from, this.to);
  }

  // #region Chart Event Handlers
   onDateFromChange(event: Event): void {
     const target = event.target as HTMLInputElement;
        this.from = target.value; 
        this.loadAndRenderStockCurveChart(this.selectedStock,this.from,this.to);
     } 
     
     onDateToChange(event: Event): void { 
      const target = event.target as HTMLInputElement; 
      this.to = target.value; console.log(`To date changed to ${this.to}`);
      this.loadAndRenderStockCurveChart(this.selectedStock,this.from,this.to); 
    }

    onMinPlateauLengthChange(event: Event): void {
      const target = event.target as HTMLInputElement;
      this.minPlateauLength = parseInt(target.value, 10) || 5;
      this.loadAndRenderStockCurveChart(this.selectedStock,this.from,this.to);
    }

    onStockChange(event: Event):void {
       const target = event.target as HTMLInputElement;
        this.selectedStock = target.value;

        console.log(`Stock changed to ${this.selectedStock}`);

         this.loadAndRenderStockCurveChart(this.selectedStock,this.from,this.to); 
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

    const candles = data.map(d => ( {
      x: new Date(d.date),
      o: d.open,
      h: d.high,
      l: d.low,
      c: d.close } ));

    this.createStockChart(prices, volumes, averagedData, plateauDatasets, candles);
  });
}

private createStockChart(
  prices: { x: Date; y: number }[],
  volumes: { x: Date; y: number }[],
  averagedData: { x: Date; y: number }[],
  plateauDatasets: any[],
  candles: {x:Date,o:number,h:number,l:number,c:number}[]
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

 
}