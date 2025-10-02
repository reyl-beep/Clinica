export interface Resultado<T = unknown> {
  value: boolean;
  message: string;
  data: T | null;
}
