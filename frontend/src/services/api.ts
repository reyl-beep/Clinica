import axios, { type AxiosResponse } from 'axios';
import { useAuthStore } from '@/stores/auth';
import type { Resultado } from '@/types/Resultado';
import type { AuthSession } from '@/types/AuthSession';
import type { Usuario } from '@/types/Usuario';
import type { Medico } from '@/types/Medico';
import type { Paciente } from '@/types/Paciente';
import type { Consulta } from '@/types/Consulta';
import type { ConsultaHistorial } from '@/types/ConsultaHistorial';

export class ApiAuthError extends Error {
  constructor(public status: number, message: string) {
    super(message);
    this.name = 'ApiAuthError';
  }
}

const http = axios.create({
  baseURL: '/api'
});

http.interceptors.request.use(config => {
  const { token } = useAuthStore();
  const authToken = token.value;

  if (authToken) {
    config.headers = {
      ...(config.headers ?? {}),
      Authorization: `Bearer ${authToken}`
    };
  }

  return config;
});

http.interceptors.response.use(
  response => response,
  error => {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status ?? 0;
      if (status === 401 || status === 403) {
        const data = error.response?.data as Resultado<unknown> | undefined;
        const fallbackMessage =
          status === 403
            ? 'No tienes permisos para realizar esta acción.'
            : 'Tu sesión ha expirado. Inicia sesión nuevamente.';
        const message = data?.message || fallbackMessage;
        return Promise.reject(new ApiAuthError(status, message));
      }
    }

    return Promise.reject(error);
  }
);

type RequestAction<T> = () => Promise<AxiosResponse<Resultado<T>>>;

function buildErrorResult<T>(error: unknown): Resultado<T> {
  const defaultMessage = 'No fue posible conectar con el servidor.';

  if (axios.isAxiosError(error)) {
    const data = error.response?.data as Resultado<T> | undefined;
    if (data) {
      return {
        value: data.value ?? false,
        message: data.message ?? defaultMessage,
        data: (data.data as T | null) ?? null
      };
    }

    return {
      value: false,
      message: error.message || defaultMessage,
      data: null
    };
  }

  return {
    value: false,
    message: error instanceof Error ? error.message : defaultMessage,
    data: null
  };
}

async function safeRequest<T>(action: RequestAction<T>): Promise<Resultado<T>> {
  try {
    const response = await action();
    return response.data;
  } catch (error) {
    if (error instanceof ApiAuthError) {
      throw error;
    }
    return buildErrorResult<T>(error);
  }
}

export interface LoginPayload {
  correo: string;
  password: string;
}

export async function login(payload: LoginPayload): Promise<Resultado<AuthSession>> {
  try {
    const response = await http.post<Resultado<AuthSession>>('/auth/login', payload);
    return response.data;
  } catch (error) {
    if (error instanceof ApiAuthError) {
      return {
        value: false,
        message: error.message,
        data: null
      };
    }

    return buildErrorResult<AuthSession>(error);
  }
}

export interface MedicoPayload {
  primerNombre: string;
  segundoNombre?: string | null;
  apellidoPaterno: string;
  apellidoMaterno?: string | null;
  cedula: string;
  telefono?: string | null;
  especialidad?: string | null;
  email?: string | null;
}

export function fetchMedicos(): Promise<Resultado<Medico[]>> {
  return safeRequest(() => http.get<Resultado<Medico[]>>('/medicos'));
}

export function fetchMedico(id: number): Promise<Resultado<Medico | null>> {
  return safeRequest(() => http.get<Resultado<Medico | null>>(`/medicos/${id}`));
}

export function createMedico(payload: MedicoPayload): Promise<Resultado<{ id: number } | null>> {
  return safeRequest(() => http.post<Resultado<{ id: number } | null>>('/medicos', payload));
}

export function updateMedico(id: number, payload: MedicoPayload): Promise<Resultado<{ id: number } | null>> {
  return safeRequest(() => http.put<Resultado<{ id: number } | null>>(`/medicos/${id}`, payload));
}

export function deleteMedico(id: number): Promise<Resultado<{ id: number } | null>> {
  return safeRequest(() => http.delete<Resultado<{ id: number } | null>>(`/medicos/${id}`));
}

export interface UsuarioPayload {
  correo: string;
  password: string;
  nombreCompleto: string;
  idMedico: number | null;
}

export function fetchUsuarios(): Promise<Resultado<Usuario[]>> {
  return safeRequest(() => http.get<Resultado<Usuario[]>>('/usuarios'));
}

export function fetchUsuario(id: number): Promise<Resultado<Usuario | null>> {
  return safeRequest(() => http.get<Resultado<Usuario | null>>(`/usuarios/${id}`));
}

export function createUsuario(payload: UsuarioPayload): Promise<Resultado<{ id: number } | null>> {
  return safeRequest(() => http.post<Resultado<{ id: number } | null>>('/usuarios', payload));
}

export function updateUsuario(id: number, payload: UsuarioPayload): Promise<Resultado<{ id: number } | null>> {
  return safeRequest(() => http.put<Resultado<{ id: number } | null>>(`/usuarios/${id}`, payload));
}

export function deleteUsuario(id: number): Promise<Resultado<{ id: number } | null>> {
  return safeRequest(() => http.delete<Resultado<{ id: number } | null>>(`/usuarios/${id}`));
}

export function fetchPacientes(): Promise<Resultado<Paciente[]>> {
  return safeRequest(() => http.get<Resultado<Paciente[]>>('/pacientes'));
}

export function fetchPaciente(id: number): Promise<Resultado<Paciente | null>> {
  return safeRequest(() => http.get<Resultado<Paciente | null>>(`/pacientes/${id}`));
}

export interface PacientePayload {
  primerNombre: string;
  segundoNombre?: string | null;
  apellidoPaterno: string;
  apellidoMaterno?: string | null;
  telefono?: string | null;
}

export function createPaciente(
  payload: PacientePayload
): Promise<Resultado<{ id: number } | null>> {
  return safeRequest(() => http.post<Resultado<{ id: number } | null>>('/pacientes', payload));
}

export function updatePaciente(
  id: number,
  payload: PacientePayload
): Promise<Resultado<{ id: number } | null>> {
  return safeRequest(() => http.put<Resultado<{ id: number } | null>>(`/pacientes/${id}`, payload));
}

export function deletePaciente(id: number): Promise<Resultado<{ id: number } | null>> {
  return safeRequest(() => http.delete<Resultado<{ id: number } | null>>(`/pacientes/${id}`));
}

export interface ConsultaPayload {
  idMedico: number;
  idPaciente: number;
  sintomas?: string | null;
  recomendaciones?: string | null;
  diagnostico?: string | null;
  fechaConsulta?: string | null;
}

export function createConsulta(payload: ConsultaPayload): Promise<Resultado<{ id: number } | null>> {
  return safeRequest(() => http.post<Resultado<{ id: number } | null>>('/consultas', payload));
}

export function fetchConsultas(): Promise<Resultado<Consulta[]>> {
  return safeRequest(() => http.get<Resultado<Consulta[]>>('/consultas'));
}

export interface ConsultaHistorialFilters {
  idMedico?: number;
  idPaciente?: number;
  fechaInicio?: string;
  fechaFin?: string;
}

export function fetchConsultasHistorial(
  filters: ConsultaHistorialFilters = {}
): Promise<Resultado<ConsultaHistorial[]>> {
  const params: Record<string, unknown> = {};

  if (typeof filters.idMedico === 'number') {
    params.idMedico = filters.idMedico;
  }

  if (typeof filters.idPaciente === 'number') {
    params.idPaciente = filters.idPaciente;
  }

  if (filters.fechaInicio) {
    params.fechaInicio = filters.fechaInicio;
  }

  if (filters.fechaFin) {
    params.fechaFin = filters.fechaFin;
  }

  return safeRequest(() =>
    http.get<Resultado<ConsultaHistorial[]>>('/consultas/historial', {
      params
    })
  );
}

export default http;
