export interface Usuario {
  id: number;
  correo: string;
  nombreCompleto: string;
  idMedico: number | null;
  activo: boolean;
  fechaCreacion: string;
}

