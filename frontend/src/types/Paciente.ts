export interface Paciente {
  id: number;
  primerNombre: string;
  segundoNombre: string | null;
  apellidoPaterno: string;
  apellidoMaterno: string | null;
  telefono: string | null;
  activo: boolean;
  fechaCreacion: string;
}
