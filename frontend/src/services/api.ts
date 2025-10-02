import axios from 'axios';
import type { Resultado } from '@/types/Resultado';

const http = axios.create({
  baseURL: '/api'
});

export async function showResultMessage(message: string): Promise<Resultado> {
  try {
    const response = await http.post<Resultado>('/demo', { message });
    return response.data;
  } catch (error) {
    return {
      value: false,
      message: error instanceof Error ? error.message : 'Error desconocido',
      data: null
    };
  }
}

export default http;
