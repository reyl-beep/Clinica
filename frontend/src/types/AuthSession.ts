import type { Usuario } from './Usuario';

export interface AuthSession {
  token: string;
  usuario: Usuario;
}
