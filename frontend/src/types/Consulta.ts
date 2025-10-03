export interface Consulta {
  id: number;
  idMedico: number;
  idPaciente: number;
  sintomas: string | null;
  recomendaciones: string | null;
  diagnostico: string | null;
  fechaConsulta: string;
}
