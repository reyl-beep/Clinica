<template>
  <section class="page">
    <header class="page__header">
      <p class="page__eyebrow">Médicos</p>
      <h1>Gestión de médicos</h1>
      <p class="page__description">
        Crea, actualiza o desactiva a los médicos registrados en la clínica. Mantén la información
        profesional al día para agilizar la asignación de consultas.
      </p>
    </header>

    <div class="layout">
      <div class="card">
        <header class="card__header">
          <h2>{{ isEditing ? 'Editar médico' : 'Registrar nuevo médico' }}</h2>
          <p class="card__description">
            Captura los datos básicos del profesional de salud. Todos los campos marcados con * son
            obligatorios.
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

            <label class="field">
              <span>Cédula profesional *</span>
              <input v-model.trim="form.cedula" type="text" required />
            </label>

            <label class="field">
              <span>Teléfono de contacto</span>
              <input v-model.trim="form.telefono" type="tel" />
            </label>

            <label class="field field--wide">
              <span>Especialidad</span>
              <input v-model.trim="form.especialidad" type="text" />
            </label>

            <label class="field field--wide">
              <span>Correo electrónico</span>
              <input v-model.trim="form.email" type="email" />
            </label>
          </fieldset>

          <div class="form__actions">
            <button class="button ghost" type="button" @click="resetForm" :disabled="submitting">
              Cancelar
            </button>
            <button class="button primary" type="submit" :disabled="submitting">
              <span v-if="submitting" class="loader" aria-hidden="true"></span>
              <span>{{ submitting ? 'Guardando...' : isEditing ? 'Actualizar médico' : 'Guardar médico' }}</span>
            </button>
          </div>
        </form>

        <p v-if="feedback" class="alert" :class="{ 'alert--success': feedbackSuccess }" role="alert">
          {{ feedback }}
        </p>
      </div>

      <div class="card">
        <header class="card__header">
          <h2>Listado de médicos</h2>
          <p class="card__description">Selecciona un registro para editar o desactivar el perfil.</p>
        </header>

        <div v-if="loading" class="placeholder">Cargando médicos...</div>

        <table v-else class="table">
          <thead>
            <tr>
              <th scope="col">Nombre</th>
              <th scope="col">Especialidad</th>
              <th scope="col">Cédula</th>
              <th scope="col">Estado</th>
              <th scope="col">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="medico in medicos" :key="medico.id">
              <td>{{ nombreMedico(medico) }}</td>
              <td>{{ medico.especialidad || 'Sin especialidad' }}</td>
              <td>{{ medico.cedula }}</td>
              <td>
                <span class="badge" :class="{ 'badge--inactive': !medico.activo }">
                  {{ medico.activo ? 'Activo' : 'Inactivo' }}
                </span>
              </td>
              <td class="actions">
                <button class="link" type="button" @click="startEdit(medico)">Editar</button>
                <button
                  class="link link--danger"
                  type="button"
                  @click="handleDeactivate(medico.id)"
                  :disabled="!medico.activo || submitting"
                >
                  Desactivar
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ApiAuthError, createMedico, deleteMedico, fetchMedicos, updateMedico } from '@/services/api';
import { useAuthStore } from '@/stores/auth';
import type { Medico } from '@/types/Medico';

interface MedicoForm {
  id: number | null;
  primerNombre: string;
  segundoNombre: string;
  apellidoPaterno: string;
  apellidoMaterno: string;
  cedula: string;
  telefono: string;
  especialidad: string;
  email: string;
}

const router = useRouter();
const route = useRoute();
const { clearSession } = useAuthStore();

const medicos = ref<Medico[]>([]);
const loading = ref(false);
const submitting = ref(false);
const feedback = ref('');
const feedbackSuccess = ref(false);

const form = reactive<MedicoForm>({
  id: null,
  primerNombre: '',
  segundoNombre: '',
  apellidoPaterno: '',
  apellidoMaterno: '',
  cedula: '',
  telefono: '',
  especialidad: '',
  email: ''
});

const isEditing = computed(() => form.id !== null);

const nombreMedico = (medico: Medico) =>
  [
    medico.primerNombre,
    medico.segundoNombre ?? '',
    medico.apellidoPaterno,
    medico.apellidoMaterno ?? ''
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
  form.cedula = '';
  form.telefono = '';
  form.especialidad = '';
  form.email = '';
  feedback.value = '';
}

function populateForm(medico: Medico) {
  form.id = medico.id;
  form.primerNombre = medico.primerNombre;
  form.segundoNombre = medico.segundoNombre ?? '';
  form.apellidoPaterno = medico.apellidoPaterno;
  form.apellidoMaterno = medico.apellidoMaterno ?? '';
  form.cedula = medico.cedula;
  form.telefono = medico.telefono ?? '';
  form.especialidad = medico.especialidad ?? '';
  form.email = medico.email ?? '';
}

async function loadMedicos() {
  loading.value = true;
  feedback.value = '';

  try {
    const resultado = await fetchMedicos();

    if (resultado.value && resultado.data) {
      medicos.value = resultado.data;
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
        : 'No fue posible cargar la lista de médicos. Inténtalo más tarde.';
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
    cedula: form.cedula,
    telefono: form.telefono || null,
    especialidad: form.especialidad || null,
    email: form.email || null
  };
}

async function handleSubmit() {
  submitting.value = true;
  feedback.value = '';

  try {
    const payload = buildPayload();
    const resultado =
      isEditing.value && form.id !== null
        ? await updateMedico(form.id, payload)
        : await createMedico(payload);

    feedback.value = resultado.message;
    feedbackSuccess.value = resultado.value;

    if (resultado.value) {
      await loadMedicos();
      resetForm();
    }
  } catch (error) {
    if (await handleAuthError(error)) {
      return;
    }

    feedback.value =
      error instanceof Error
        ? error.message
        : 'No fue posible guardar la información del médico.';
    feedbackSuccess.value = false;
  } finally {
    submitting.value = false;
  }
}

function startEdit(medico: Medico) {
  populateForm(medico);
  feedback.value = '';
}

async function handleDeactivate(id: number) {
  submitting.value = true;

  try {
    const resultado = await deleteMedico(id);
    feedback.value = resultado.message;
    feedbackSuccess.value = resultado.value;

    if (resultado.value) {
      await loadMedicos();
    }
  } catch (error) {
    if (await handleAuthError(error)) {
      return;
    }

    feedback.value =
      error instanceof Error
        ? error.message
        : 'No fue posible actualizar el estado del médico.';
    feedbackSuccess.value = false;
  } finally {
    submitting.value = false;
  }
}

onMounted(() => {
  loadMedicos();
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
  gap: 1rem;
  border: none;
  margin: 0;
  padding: 0;
}

@media (min-width: 640px) {
  .form__grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .field--wide {
    grid-column: span 2;
  }
}

.field {
  display: grid;
  gap: 0.5rem;
}

.field span {
  font-weight: 600;
}

input[type='text'],
input[type='email'],
input[type='tel'] {
  border-radius: 16px;
  border: 1px solid var(--border-subtle);
  padding: 0.85rem 1rem;
  font-size: 1rem;
  background: var(--surface-subtle);
  color: var(--text-primary);
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

input:focus {
  outline: none;
  border-color: var(--brand-primary);
  box-shadow: 0 0 0 4px var(--focus-ring);
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
