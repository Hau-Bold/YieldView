import { Component, OnInit, Input } from '@angular/core';
import { Chart, registerables } from 'chart.js';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-stock-curve-chart',
  templateUrl: './stock-curve-chart.component.html',
  styleUrls: ['./stock-curve-chart.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
  ],
  providers: []
})
export class StockCurveChartComponent implements OnInit {

  @Input()
  from:string;
  to:string;
  selectedStock:string='Bidu'
  date: string ;

  constructor()
  {
    const today = new Date();
    this.date =   today.toISOString().split('T')[0]; 

    this.to = this.date; 
    this.from = new Date(today.getFullYear() - 1, today.getMonth(), today.getDate()).toISOString().split('T')[0]; 
  }
  ngOnInit(): void {}

  detectPlateaus():void{}

  onDateFromChange(event: Event): void
  {
    const target = event.target as HTMLInputElement;
    this.date = target.value;
  }

  onDateToChange(event: Event): void
  {
    const target = event.target as HTMLInputElement;
    this.date = target.value;
  }

}
