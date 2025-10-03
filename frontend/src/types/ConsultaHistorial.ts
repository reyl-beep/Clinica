export interface ConsultaHistorial {
  id: number;
  idMedico: number;
  idPaciente: number;
  nombreMedico: string;
  nombrePaciente: string;
  sintomas: string | null;
  recomendaciones: string | null;
  diagnostico: string | null;
  fechaConsulta: string;
}
