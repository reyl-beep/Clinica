import axios from 'axios';
import type { Resultado } from '@/types/Resultado';
import type { Usuario } from '@/types/Usuario';

const http = axios.create({
  baseURL: '/api'
});

export interface LoginPayload {
  correo: string;
  password: string;
}

export async function login(payload: LoginPayload): Promise<Resultado<Usuario>> {
  try {
    const response = await http.post<Resultado<Usuario>>('/auth/login', payload);
    return response.data;
  } catch (error) {
    return {
      value: false,
      message: error instanceof Error ? error.message : 'No fue posible conectar con el servidor.',
      data: null
    };
  }
}

export default http;
