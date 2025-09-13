export interface SP500PriceWithVolatility {
  date: string;       
  close: number;      
  volatility: number; 
  isLocalShadowPoint: boolean;
}