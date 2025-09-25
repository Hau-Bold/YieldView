export interface  StockPrice
{
 date: string;
 open: number;
 high: number,
 low: number,
 close: number,
 volume: number;
 averagedClose : number;
 gaussianAveragedClose: number;
 plateauIndex : number;
} 