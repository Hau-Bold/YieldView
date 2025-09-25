import { Component, OnInit, Input } from '@angular/core';
import { Chart, registerables } from 'chart.js';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { StockService } from '../../services/stockCurve/stock.service';
import { PlateauService } from '../../services/stockCurve/plateau.service';
import { Plateau } from '../../Modules/Plateau';
import { StockPrice } from '../../Modules/StockPrice';

Chart.register(...registerables);

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

    onStockChange(event: Event):void {
       const target = event.target as HTMLInputElement;
        this.selectedStock = target.value;
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
      const labels = data.map(d => d.date);
      const prices = data.map(d => d.close);
      const volumes = data.map(d => d.volume);
      const averagedData = data.map(d => d.averagedClose);

      let plateauDatasets: any[] = [];
      if (this.showPlateaus) {
        const plateaus: Plateau[] = this.plateauService.detectPlateaus(data, 5);
        plateauDatasets = this.createPlateauDatasets(labels, plateaus);
      }

      this.createStockChart(labels, prices,volumes, averagedData, plateauDatasets);
    });
  }

  private createPlateauDatasets(labels: string[], plateaus: Plateau[]) {
    return plateaus.map(p => ({
      label: `val: ${p.value}; Len: ${p.length}`,
      data: labels.map(date => (date >= p.startDate && date <= p.endDate ? p.value : null)),
      borderColor: 'green',
      borderWidth: 2,
      borderDash: [5, 5],
      fill: false,
      pointRadius: 0,
      spanGaps: true,
      datalabels: { display: false, align: 'start', anchor: 'start' }
    }));
  }

  private createStockChart(labels: string[], prices: number[],volumes:number[], averagedData: number[], plateauDatasets: any[]) {

    if (this.stockCurveChart) 
    {
        this.stockCurveChart.destroy();
    }

    const ctx = document.getElementById('stockCurveChart') as HTMLCanvasElement;

    this.stockCurveChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels,
        datasets: [
          { label: `Stock Curve for ${this.selectedStock}`, data: prices, borderColor: 'blue', fill: false, tension: 0, pointRadius: 0, spanGaps: true },
          { label: 'Averaged Mean', data: averagedData, borderColor: 'rgba(255, 99, 132, 1)', fill: false,  tension: 0, pointRadius: 0, spanGaps: true, hidden: !this.showAveragedMean },
          ...plateauDatasets,
          { label: 'Volume', data: volumes, type: 'bar', backgroundColor: 'rgba(0, 0, 0, 0.2)', borderColor: 'rgba(0, 0, 0, 0.3)',  yAxisID: 'y1', hidden: !this.showVolumes       }
        ]
      },
      options: {
        responsive: true,
        interaction: { mode: 'index', intersect: false },
        scales: {
          y: { title: { display: true, text: 'Close' } },
          y1: {title: { display: true, text: 'Volume' }, type: 'linear', position: 'right', grid: { drawOnChartArea: false } },
          x: { title: { display: true, text: 'Date' }, ticks: { maxTicksLimit: 10 } }
        },
        plugins: {
          legend: { display: true }
        }
      }
    });
  }
}
