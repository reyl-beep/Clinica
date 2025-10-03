<template>
  <section class="page">
    <header class="page__header">
      <p class="page__eyebrow">Consultas</p>
      <h1>Historial de consultas</h1>
      <p class="page__description">
        Filtra y revisa las consultas realizadas en la clínica. Puedes acotar por médico, paciente y
        rangos de fechas para encontrar la información que necesitas.
      </p>
    </header>

    <div class="card">
      <form class="filters" @submit.prevent="handleFilter">
        <fieldset class="filters__grid" :disabled="loading">
          <label class="field">
            <span>Médico</span>
            <select v-model="filters.idMedico">
              <option value="">Todos los médicos</option>
              <option v-for="medico in medicos" :key="medico.id" :value="medico.id">
                {{ nombreMedico(medico) }}
              </option>
            </select>
          </label>

          <label class="field">
            <span>Paciente</span>
            <select v-model="filters.idPaciente">
              <option value="">Todos los pacientes</option>
              <option v-for="paciente in pacientes" :key="paciente.id" :value="paciente.id">
                {{ nombrePaciente(paciente) }}
              </option>
            </select>
          </label>

          <label class="field">
            <span>Fecha inicial</span>
            <input v-model="filters.fechaInicio" type="date" />
          </label>

          <label class="field">
            <span>Fecha final</span>
            <input v-model="filters.fechaFin" type="date" />
          </label>
        </fieldset>

        <div class="filters__actions">
          <button class="button ghost" type="button" @click="resetFilters" :disabled="loading">
            Limpiar filtros
          </button>
          <button class="button primary" type="submit" :disabled="loading">
            <span v-if="loading" class="loader" aria-hidden="true"></span>
            <span>{{ loading ? 'Consultando...' : 'Buscar' }}</span>
          </button>
        </div>
      </form>

      <p v-if="feedback" class="feedback" :class="{ 'feedback--success': feedbackSuccess }" role="status">
        {{ feedback }}
      </p>

      <div v-if="loading" class="placeholder" aria-live="polite">
        Cargando historial de consultas...
      </div>

      <div v-else>
        <table v-if="historial.length" class="table">
          <thead>
            <tr>
              <th scope="col">Fecha</th>
              <th scope="col">Médico</th>
              <th scope="col">Paciente</th>
              <th scope="col">Diagnóstico</th>
              <th scope="col">Recomendaciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="consulta in historial" :key="consulta.id">
              <td>{{ formatFecha(consulta.fechaConsulta) }}</td>
              <td>{{ consulta.nombreMedico }}</td>
              <td>{{ consulta.nombrePaciente }}</td>
              <td>{{ consulta.diagnostico || 'Sin diagnóstico' }}</td>
              <td>{{ consulta.recomendaciones || 'Sin recomendaciones' }}</td>
            </tr>
          </tbody>
        </table>

        <p v-else class="empty">No se encontraron consultas con los filtros seleccionados.</p>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  ApiAuthError,
  fetchConsultasHistorial,
  fetchMedicos,
  fetchPacientes
} from '@/services/api';
import { useAuthStore } from '@/stores/auth';
import type { ConsultaHistorial } from '@/types/ConsultaHistorial';
import type { Medico } from '@/types/Medico';
import type { Paciente } from '@/types/Paciente';

interface Filters {
  idMedico: string;
  idPaciente: string;
  fechaInicio: string;
  fechaFin: string;
}

const router = useRouter();
const route = useRoute();
const { clearSession } = useAuthStore();

const filters = reactive<Filters>({
  idMedico: '',
  idPaciente: '',
  fechaInicio: '',
  fechaFin: ''
});

const medicos = ref<Medico[]>([]);
const pacientes = ref<Paciente[]>([]);
const historial = ref<ConsultaHistorial[]>([]);
const loading = ref(false);
const feedback = ref('');
const feedbackSuccess = ref(false);

const nombreMedico = (medico: Medico) =>
  [
    medico.primerNombre,
    medico.segundoNombre ?? '',
    medico.apellidoPaterno,
    medico.apellidoMaterno ?? ''
  ]
    .filter(Boolean)
    .join(' ');

const nombrePaciente = (paciente: Paciente) =>
  [
    paciente.primerNombre,
    paciente.segundoNombre ?? '',
    paciente.apellidoPaterno,
    paciente.apellidoMaterno ?? ''
  ]
    .filter(Boolean)
    .join(' ');

function resetFilters() {
  filters.idMedico = '';
  filters.idPaciente = '';
  filters.fechaInicio = '';
  filters.fechaFin = '';
  handleFilter();
}

function formatFecha(value: string) {
  const date = new Date(value);
  return date.toLocaleString('es-MX', {
    year: 'numeric',
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  });
}

async function handleAuthError(error: unknown): Promise<boolean> {
  if (error instanceof ApiAuthError) {
    feedback.value = error.message;
    feedbackSuccess.value = false;
    clearSession();
    await router.push({ name: 'login', query: { redirect: route.fullPath } });
    return true;
  }

  return false;
}

async function loadCatalogs() {
  try {
    const [medicosResultado, pacientesResultado] = await Promise.all([
      fetchMedicos(),
      fetchPacientes()
    ]);

    if (medicosResultado.value && medicosResultado.data) {
      medicos.value = medicosResultado.data.filter(medico => medico.activo);
    }

    if (pacientesResultado.value && pacientesResultado.data) {
      pacientes.value = pacientesResultado.data.filter(paciente => paciente.activo);
    }
  } catch (error) {
    if (await handleAuthError(error)) {
      return;
    }

    feedback.value =
      error instanceof Error
        ? error.message
        : 'No fue posible cargar los catálogos de médicos y pacientes.';
    feedbackSuccess.value = false;
  }
}

async function handleFilter() {
  loading.value = true;
  feedback.value = '';

  try {
    const resultado = await fetchConsultasHistorial({
      idMedico: filters.idMedico ? Number(filters.idMedico) : undefined,
      idPaciente: filters.idPaciente ? Number(filters.idPaciente) : undefined,
      fechaInicio: filters.fechaInicio || undefined,
      fechaFin: filters.fechaFin || undefined
    });

    if (resultado.value && resultado.data) {
      historial.value = resultado.data;
      feedback.value = resultado.message;
      feedbackSuccess.value = true;
    } else {
      historial.value = [];
      feedback.value = resultado.message;
      feedbackSuccess.value = false;
    }
  } catch (error) {
    if (await handleAuthError(error)) {
      return;
    }

    historial.value = [];
    feedback.value =
      error instanceof Error
        ? error.message
        : 'No fue posible consultar el historial. Intenta nuevamente.';
    feedbackSuccess.value = false;
  } finally {
    loading.value = false;
  }
}

onMounted(async () => {
  await loadCatalogs();
  await handleFilter();
});
</script>

<style scoped>
.page {
  display: grid;
  gap: 2rem;
}

.page__header {
  display: grid;
  gap: 0.75rem;
  max-width: 720px;
}

.page__eyebrow {
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.28em;
  font-weight: 600;
  color: var(--brand-primary);
  margin: 0;
}

.page__description {
  margin: 0;
  color: var(--text-muted);
}

.card {
  background: var(--surface-raised);
  border-radius: 28px;
  padding: clamp(1.5rem, 4vw, 2.5rem);
  box-shadow: var(--shadow-xl);
  display: grid;
  gap: 1.5rem;
}

.filters {
  display: grid;
  gap: 1rem;
}

.filters__grid {
  display: grid;
  gap: 1rem;
  border: none;
  margin: 0;
  padding: 0;
}

@media (min-width: 960px) {
  .filters__grid {
    grid-template-columns: repeat(4, minmax(0, 1fr));
  }
}

.field {
  display: grid;
  gap: 0.5rem;
}

.field span {
  font-weight: 600;
}

select,
input[type='date'] {
  border-radius: 16px;
  border: 1px solid var(--border-subtle);
  padding: 0.85rem 1rem;
  font-size: 1rem;
  background: var(--surface-subtle);
  color: var(--text-primary);
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

select:focus,
input[type='date']:focus {
  outline: none;
  border-color: var(--brand-primary);
  box-shadow: 0 0 0 4px var(--focus-ring);
}

.filters__actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
}

.button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.6rem;
  border-radius: 999px;
  padding: 0.85rem 1.4rem;
  font-weight: 600;
  cursor: pointer;
}

.button.primary {
  background: linear-gradient(135deg, var(--brand-primary), var(--brand-secondary));
  color: #fff;
  border: none;
  box-shadow: var(--shadow-md);
}

.button.ghost {
  background: transparent;
  border: 1px solid var(--border-subtle);
  color: var(--text-primary);
}

.button:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.loader {
  width: 1rem;
  height: 1rem;
  border-radius: 50%;
  border: 2px solid rgba(255, 255, 255, 0.4);
  border-top-color: #fff;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.feedback {
  margin: 0;
  padding: 0.9rem 1.2rem;
  border-radius: 18px;
  background: var(--error-bg);
  border: 1px solid var(--error-border);
  color: var(--error-text);
  font-weight: 600;
}

.feedback--success {
  background: rgba(34, 197, 94, 0.14);
  border-color: rgba(34, 197, 94, 0.4);
  color: var(--success-text);
}

.placeholder {
  padding: 1.5rem;
  border-radius: 18px;
  background: var(--surface-subtle);
  border: 1px dashed var(--border-subtle);
  text-align: center;
  color: var(--text-muted);
}

.table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.95rem;
}

.table th,
.table td {
  padding: 0.85rem 1rem;
  text-align: left;
  border-bottom: 1px solid var(--border-subtle);
}

.table tbody tr:hover {
  background: rgba(37, 99, 235, 0.06);
}

.empty {
  margin: 0;
  padding: 1.5rem;
  border-radius: 18px;
  border: 1px dashed var(--border-subtle);
  color: var(--text-muted);
  text-align: center;
}
</style>
