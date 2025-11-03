import { Candle } from "../Modules/Candle";

export function toISOString(date: Date ): string{
    return date.toISOString().split('T')[0];
}

export function groupCandlesByYear(candles: Candle[]): Record<number, Candle[]> {
  const grouped: Record<number, Candle[]> = {};

  candles.forEach(candle => {
    const year = candle.x.getFullYear();
    if (!grouped[year])
        {
             grouped[year] = [];
        }
    grouped[year].push(candle);
  });


  return grouped;
}

export function groupCandlesByMonth(groupedCandlesByYear: Record<number, Candle[]>): Record<number,Record<number, Candle[]>> {
  const grouped: Record<number,Record<number, Candle[]>> = {};

  for (const [yearStr, candles] of Object.entries(groupedCandlesByYear)) {
    const year = parseInt(yearStr, 10);
    grouped[year] = {};

    candles.forEach(candle => {
      const month = candle.x.getMonth() + 1; 

      if (!grouped[year][month]) {
        grouped[year][month] = [];
      }
      grouped[year][month].push(candle);
    });
  }

  return grouped;
}

export function getIsoWeekNumber(date: Date): number {
  const tmp = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
  const dayNum = tmp.getUTCDay() || 7;
  tmp.setUTCDate(tmp.getUTCDate() + 4 - dayNum);
  const yearStart = new Date(Date.UTC(tmp.getUTCFullYear(), 0, 1));
  const weekNo = Math.ceil((((tmp.getTime() - yearStart.getTime()) / 86400000) + 1) / 7);
  return weekNo;
}