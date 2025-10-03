<template>
  <section class="page">
    <header class="page__header">
      <p class="page__eyebrow">Pacientes</p>
      <h1>Administración de pacientes</h1>
      <p class="page__description">
        Registra, actualiza y gestiona la información de los pacientes de la clínica. Mantén sus datos
        de contacto al día para ofrecer un servicio oportuno.
      </p>
    </header>

    <div class="layout">
      <div class="card">
        <header class="card__header">
          <h2>{{ isEditing ? 'Editar paciente' : 'Registrar nuevo paciente' }}</h2>
          <p class="card__description">
            Captura los datos básicos del paciente. Los campos marcados con * son obligatorios para un
            correcto seguimiento clínico.
          </p>
        </header>

        <form class="form" @submit.prevent="handleSubmit">
          <fieldset class="form__grid" :disabled="submitting">
            <label class="field">
              <span>Primer nombre *</span>
              <input v-model.trim="form.primerNombre" type="text" required />
            </label>

            <label class="field">
              <span>Segundo nombre</span>
              <input v-model.trim="form.segundoNombre" type="text" />
            </label>

            <label class="field">
              <span>Apellido paterno *</span>
              <input v-model.trim="form.apellidoPaterno" type="text" required />
            </label>

            <label class="field">
              <span>Apellido materno</span>
              <input v-model.trim="form.apellidoMaterno" type="text" />
            </label>

            <label class="field field--wide">
              <span>Teléfono de contacto</span>
              <input v-model.trim="form.telefono" type="tel" />
            </label>
          </fieldset>

          <div class="form__actions">
            <button class="button ghost" type="button" @click="resetForm" :disabled="submitting">
              Cancelar
            </button>
            <button class="button primary" type="submit" :disabled="submitting">
              <span v-if="submitting" class="loader" aria-hidden="true"></span>
              <span>{{ submitting ? 'Guardando...' : isEditing ? 'Actualizar paciente' : 'Guardar paciente' }}</span>
            </button>
          </div>
        </form>

        <p v-if="feedback" class="alert" :class="{ 'alert--success': feedbackSuccess }" role="alert">
          {{ feedback }}
        </p>
      </div>

      <div class="card">
        <header class="card__header">
          <h2>Listado de pacientes</h2>
          <p class="card__description">Selecciona un registro para editarlo o desactivarlo.</p>
        </header>

        <div v-if="loading" class="placeholder">Cargando pacientes...</div>

        <div v-else class="table-wrapper">
          <table class="table">
            <thead>
              <tr>
                <th scope="col">Nombre</th>
                <th scope="col">Teléfono</th>
                <th scope="col">Estado</th>
              <th scope="col">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="paciente in pacientes" :key="paciente.id">
              <td>{{ nombrePaciente(paciente) }}</td>
              <td>{{ paciente.telefono || 'Sin capturar' }}</td>
              <td>
                <span class="badge" :class="{ 'badge--inactive': !paciente.activo }">
                  {{ paciente.activo ? 'Activo' : 'Inactivo' }}
                </span>
              </td>
              <td class="actions">
                <button class="link" type="button" @click="startEdit(paciente)">Editar</button>
                <button
                  class="link link--danger"
                  type="button"
                  @click="handleDeactivate(paciente.id)"
                  :disabled="!paciente.activo || submitting"
                >
                  Desactivar
                </button>
              </td>
            </tr>
          </tbody>
          </table>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  ApiAuthError,
  createPaciente,
  deletePaciente,
  fetchPacientes,
  updatePaciente
} from '@/services/api';
import { useAuthStore } from '@/stores/auth';
import type { Paciente } from '@/types/Paciente';

interface PacienteForm {
  id: number | null;
  primerNombre: string;
  segundoNombre: string;
  apellidoPaterno: string;
  apellidoMaterno: string;
  telefono: string;
}

const router = useRouter();
const route = useRoute();
const { clearSession } = useAuthStore();

const pacientes = ref<Paciente[]>([]);
const loading = ref(false);
const submitting = ref(false);
const feedback = ref('');
const feedbackSuccess = ref(false);

const form = reactive<PacienteForm>({
  id: null,
  primerNombre: '',
  segundoNombre: '',
  apellidoPaterno: '',
  apellidoMaterno: '',
  telefono: ''
});

const isEditing = computed(() => form.id !== null);

const nombrePaciente = (paciente: Paciente) =>
  [
    paciente.primerNombre,
    paciente.segundoNombre ?? '',
    paciente.apellidoPaterno,
    paciente.apellidoMaterno ?? ''
  ]
    .filter(Boolean)
    .join(' ');

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

function resetForm() {
  form.id = null;
  form.primerNombre = '';
  form.segundoNombre = '';
  form.apellidoPaterno = '';
  form.apellidoMaterno = '';
  form.telefono = '';
  feedback.value = '';
}

function populateForm(paciente: Paciente) {
  form.id = paciente.id;
  form.primerNombre = paciente.primerNombre;
  form.segundoNombre = paciente.segundoNombre ?? '';
  form.apellidoPaterno = paciente.apellidoPaterno;
  form.apellidoMaterno = paciente.apellidoMaterno ?? '';
  form.telefono = paciente.telefono ?? '';
}

async function loadPacientes() {
  loading.value = true;
  feedback.value = '';

  try {
    const resultado = await fetchPacientes();

    if (resultado.value && resultado.data) {
      pacientes.value = resultado.data;
    } else {
      feedback.value = resultado.message;
      feedbackSuccess.value = false;
    }
  } catch (error) {
    if (await handleAuthError(error)) {
      return;
    }

    feedback.value =
      error instanceof Error
        ? error.message
        : 'No fue posible cargar la lista de pacientes. Inténtalo más tarde.';
    feedbackSuccess.value = false;
  } finally {
    loading.value = false;
  }
}

function buildPayload() {
  return {
    primerNombre: form.primerNombre,
    segundoNombre: form.segundoNombre || null,
    apellidoPaterno: form.apellidoPaterno,
    apellidoMaterno: form.apellidoMaterno || null,
    telefono: form.telefono || null
  };
}

async function handleSubmit() {
  submitting.value = true;
  feedback.value = '';

  try {
    const payload = buildPayload();
    const resultado =
      isEditing.value && form.id !== null
        ? await updatePaciente(form.id, payload)
        : await createPaciente(payload);

    feedback.value = resultado.message;
    feedbackSuccess.value = resultado.value;

    if (resultado.value) {
      await loadPacientes();
      resetForm();
    }
  } catch (error) {
    if (await handleAuthError(error)) {
      return;
    }

    feedback.value =
      error instanceof Error
        ? error.message
        : 'No fue posible guardar la información del paciente.';
    feedbackSuccess.value = false;
  } finally {
    submitting.value = false;
  }
}

function startEdit(paciente: Paciente) {
  populateForm(paciente);
  feedback.value = '';
}

async function handleDeactivate(id: number) {
  submitting.value = true;

  try {
    const resultado = await deletePaciente(id);
    feedback.value = resultado.message;
    feedbackSuccess.value = resultado.value;

    if (resultado.value) {
      await loadPacientes();
    }
  } catch (error) {
    if (await handleAuthError(error)) {
      return;
    }

    feedback.value =
      error instanceof Error
        ? error.message
        : 'No fue posible actualizar el estado del paciente.';
    feedbackSuccess.value = false;
  } finally {
    submitting.value = false;
  }
}

onMounted(() => {
  loadPacientes();
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

.layout {
  display: grid;
  gap: 1.75rem;
}

@media (min-width: 960px) {
  .layout {
    grid-template-columns: minmax(0, 420px) minmax(0, 1fr);
    align-items: start;
  }
}

.card {
  background: var(--surface-raised);
  border-radius: 28px;
  padding: clamp(1.5rem, 4vw, 2.5rem);
  box-shadow: var(--shadow-xl);
  display: grid;
  gap: 1.5rem;
}

.card__header {
  display: grid;
  gap: 0.5rem;
}

.card__description {
  margin: 0;
  color: var(--text-muted);
}

.form {
  display: grid;
  gap: 1.5rem;
}

.form__grid {
  display: grid;
  gap: clamp(0.75rem, 2vw, 1.25rem);
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  border: none;
  margin: 0;
  padding: 0;
}

.field--wide {
  grid-column: 1 / -1;
}

.field {
  display: grid;
  gap: 0.45rem;
}

.field span {
  font-weight: 600;
  color: var(--text-muted);
  font-size: 0.95rem;
}

input[type='text'],
input[type='tel'] {
  border-radius: 18px;
  border: 1px solid var(--border-strong);
  padding: 0.95rem 1.1rem;
  font-size: 1rem;
  background: var(--surface-glass);
  color: var(--text-primary);
  box-shadow: var(--shadow-md);
  backdrop-filter: blur(14px);
  transition: border-color 0.2s ease, box-shadow 0.2s ease, transform 0.2s ease;
}

input::placeholder {
  color: var(--text-muted);
  opacity: 0.9;
}

input:focus {
  outline: none;
  border-color: var(--brand-primary);
  box-shadow: 0 0 0 4px var(--focus-ring);
  transform: translateY(-2px);
}

:global(:root[data-theme='dark']) input[type='text'],
:global(:root[data-theme='dark']) input[type='tel'] {
  background: rgba(15, 23, 42, 0.75);
  box-shadow: 0 18px 40px rgba(7, 37, 64, 0.45);
}

.form__actions {
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

.alert {
  margin: 0;
  padding: 1rem 1.25rem;
  border-radius: 18px;
  border: 1px solid var(--error-border);
  background: var(--error-bg);
  color: var(--error-text);
  font-weight: 600;
}

.alert--success {
  border-color: rgba(34, 197, 94, 0.4);
  background: rgba(34, 197, 94, 0.14);
  color: var(--success-text);
}

.placeholder {
  padding: 1.5rem;
  border-radius: 18px;
  border: 1px dashed var(--border-subtle);
  text-align: center;
  color: var(--text-muted);
}


.table-wrapper {
  border-radius: 22px;
  background: var(--surface-glass);
  border: 1px solid var(--border-strong);
  box-shadow: var(--shadow-lg);
  overflow: hidden;
  backdrop-filter: blur(14px);
}

.table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  font-size: 0.95rem;
}

.table thead th {
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-size: 0.78rem;
  color: var(--text-muted);
  background: rgba(37, 99, 235, 0.08);
  padding: 0.95rem 1.1rem;
  border-bottom: 1px solid var(--border-subtle);
}

.table tbody td {
  padding: 1rem 1.1rem;
  border-top: 1px solid var(--border-subtle);
}

.table tbody tr:nth-child(even) {
  background: rgba(148, 163, 184, 0.12);
}

.table tbody tr:hover {
  background: rgba(37, 99, 235, 0.12);
}

:global(:root[data-theme='dark']) .table thead th {
  background: rgba(37, 99, 235, 0.24);
  color: #e2e8f0;
}

:global(:root[data-theme='dark']) .table tbody tr:nth-child(even) {
  background: rgba(15, 23, 42, 0.55);
}

:global(:root[data-theme='dark']) .table tbody tr:hover {
  background: rgba(37, 99, 235, 0.3);
}

.actions {
  display: flex;
  gap: 0.5rem;
}

.link {
  border: none;
  background: none;
  color: var(--brand-primary);
  font-weight: 600;
  cursor: pointer;
}

.link--danger {
  color: #dc2626;
}

.link:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.2rem 0.75rem;
  border-radius: 999px;
  background: rgba(34, 197, 94, 0.14);
  color: var(--success-text);
  font-size: 0.85rem;
  font-weight: 600;
}

.badge--inactive {
  background: rgba(239, 68, 68, 0.18);
  color: var(--error-text);
}
</style>
