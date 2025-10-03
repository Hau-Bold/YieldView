import { Injectable } from '@angular/core';
import { StockPrice } from '../../Modules/StockPrice';
import { Plateau } from '../../Modules/Plateau';



@Injectable({
  providedIn: 'root'
})
export class PlateauService {

  /**
   * Detect plateaus based on gaussianAveragedClose staying constant
   * for at least `minLength` consecutive days.
   */
  detectPlateaus(data: StockPrice[], minLength: number = 5): Plateau[] {
    const plateaus: Plateau[] = [];

    if (!data || data.length === 0) {
      return plateaus;
    }

    let startIdx = 0;
    let currentGaussian = data[0].gaussianAveragedClose;

    for (let i = 1; i <= data.length; i++) {
      const d = data[i];
      const g = d?.gaussianAveragedClose;

      // check if Gaussian changes OR we reached the end
      if (g !== currentGaussian || i === data.length) {
        const length = i - startIdx;
        if (length >= minLength) {
          //one plateau found
          plateaus.push({
            startDate: data[startIdx].date,
            endDate: data[i - 1].date,
            value: currentGaussian,
            length: length
          });
        }
        // reset
        startIdx = i;
        currentGaussian = g;
      }
    }

    return plateaus;
  }
}
