export interface Medico {
  id: number;
  primerNombre: string;
  segundoNombre: string | null;
  apellidoPaterno: string;
  apellidoMaterno: string | null;
  cedula: string;
  telefono: string | null;
  especialidad: string | null;
  email: string | null;
  activo: boolean;
  fechaCreacion: string;
}
