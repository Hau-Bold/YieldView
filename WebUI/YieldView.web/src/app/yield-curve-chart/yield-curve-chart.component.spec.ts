import { ComponentFixture, TestBed } from '@angular/core/testing';

import { YieldCurveChartComponent } from './yield-curve-chart.component';

describe('YieldCurveChartComponent', () => {
  let component: YieldCurveChartComponent;
  let fixture: ComponentFixture<YieldCurveChartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [YieldCurveChartComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(YieldCurveChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
