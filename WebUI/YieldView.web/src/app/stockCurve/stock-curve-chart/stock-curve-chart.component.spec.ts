import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StockCurveChartComponent } from './stock-curve-chart.component';

describe('StockCurveChartComponent', () => {
  let component: StockCurveChartComponent;
  let fixture: ComponentFixture<StockCurveChartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [StockCurveChartComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StockCurveChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
