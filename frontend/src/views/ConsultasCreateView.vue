<template>
  <section class="page">
    <header class="page__header">
      <p class="page__eyebrow">Consultas</p>
      <h1>Registrar una nueva consulta</h1>
      <p class="page__description">
        Completa el formulario con los datos del médico, paciente y hallazgos de la valoración para
        resguardar la atención proporcionada.
      </p>
    </header>

    <div class="card">
      <form class="form" @submit.prevent="handleSubmit">
        <fieldset class="form__grid" :disabled="submitting || loadingCatalogs">
          <label class="field">
            <span>Médico responsable</span>
            <select v-model="form.idMedico" required>
              <option value="" disabled>Selecciona un médico</option>
              <option v-for="medico in medicos" :key="medico.id" :value="medico.id">
                {{ nombreMedico(medico) }}
              </option>
            </select>
          </label>

          <label class="field">
            <span>Paciente</span>
            <select v-model="form.idPaciente" required>
              <option value="" disabled>Selecciona un paciente</option>
              <option v-for="paciente in pacientes" :key="paciente.id" :value="paciente.id">
                {{ nombrePaciente(paciente) }}
              </option>
            </select>
          </label>

          <label class="field field--wide">
            <span>Descripción de síntomas</span>
            <textarea
              v-model.trim="form.sintomas"
              rows="3"
              placeholder="Describe los síntomas o motivos de consulta"
            ></textarea>
          </label>

          <label class="field field--wide">
            <span>Diagnóstico</span>
            <textarea
              v-model.trim="form.diagnostico"
              rows="3"
              placeholder="Especifica el diagnóstico principal (opcional)"
            ></textarea>
          </label>

          <label class="field field--wide">
            <span>Recomendaciones</span>
            <textarea
              v-model.trim="form.recomendaciones"
              rows="3"
              placeholder="Incluye recomendaciones, estudios o medicamentos sugeridos"
            ></textarea>
          </label>

          <label class="field">
            <span>Fecha y hora de la consulta</span>
            <input v-model="form.fechaConsulta" type="datetime-local" />
            <small>Si se omite, se registrará con la fecha y hora actuales.</small>
          </label>
        </fieldset>

        <div class="form__actions">
          <button class="button primary" type="submit" :disabled="submitting || loadingCatalogs">
            <span v-if="submitting" class="loader" aria-hidden="true"></span>
            <span>{{ submitting ? 'Guardando consulta...' : 'Guardar consulta' }}</span>
          </button>
        </div>
      </form>

      <p v-if="feedback" class="alert" :class="{ 'alert--success': feedbackSuccess }" role="alert">
        {{ feedback }}
      </p>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ApiAuthError, createConsulta, fetchMedicos, fetchPacientes } from '@/services/api';
import { useAuthStore } from '@/stores/auth';
import type { Medico } from '@/types/Medico';
import type { Paciente } from '@/types/Paciente';

interface ConsultaForm {
  idMedico: string;
  idPaciente: string;
  sintomas: string;
  recomendaciones: string;
  diagnostico: string;
  fechaConsulta: string;
}

const router = useRouter();
const route = useRoute();
const { clearSession } = useAuthStore();

const medicos = ref<Medico[]>([]);
const pacientes = ref<Paciente[]>([]);
const loadingCatalogs = ref(false);
const submitting = ref(false);
const feedback = ref('');
const feedbackSuccess = ref(false);

const form = reactive<ConsultaForm>({
  idMedico: '',
  idPaciente: '',
  sintomas: '',
  recomendaciones: '',
  diagnostico: '',
  fechaConsulta: ''
});

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

const isFormValid = computed(() => !!form.idMedico && !!form.idPaciente);

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
  loadingCatalogs.value = true;
  feedback.value = '';

  try {
    const [medicosResultado, pacientesResultado] = await Promise.all([
      fetchMedicos(),
      fetchPacientes()
    ]);

    if (medicosResultado.value && medicosResultado.data) {
      medicos.value = medicosResultado.data.filter(medico => medico.activo);
    } else {
      feedback.value = medicosResultado.message;
      feedbackSuccess.value = false;
    }

    if (pacientesResultado.value && pacientesResultado.data) {
      pacientes.value = pacientesResultado.data.filter(paciente => paciente.activo);
    } else if (!feedback.value) {
      feedback.value = pacientesResultado.message;
      feedbackSuccess.value = false;
    }
  } catch (error) {
    if (await handleAuthError(error)) {
      return;
    }

    feedback.value =
      error instanceof Error
        ? error.message
        : 'No fue posible cargar el catálogo de médicos y pacientes.';
    feedbackSuccess.value = false;
  } finally {
    loadingCatalogs.value = false;
  }
}

function resetForm() {
  form.idMedico = '';
  form.idPaciente = '';
  form.sintomas = '';
  form.recomendaciones = '';
  form.diagnostico = '';
  form.fechaConsulta = '';
}

async function handleSubmit() {
  if (!isFormValid.value) {
    feedback.value = 'Selecciona un médico y un paciente para registrar la consulta.';
    feedbackSuccess.value = false;
    return;
  }

  submitting.value = true;
  feedback.value = '';

  try {
    const resultado = await createConsulta({
      idMedico: Number(form.idMedico),
      idPaciente: Number(form.idPaciente),
      sintomas: form.sintomas || null,
      recomendaciones: form.recomendaciones || null,
      diagnostico: form.diagnostico || null,
      fechaConsulta: form.fechaConsulta ? new Date(form.fechaConsulta).toISOString() : null
    });

    feedback.value = resultado.message;
    feedbackSuccess.value = resultado.value;

    if (resultado.value) {
      resetForm();
    }
  } catch (error) {
    if (await handleAuthError(error)) {
      return;
    }

    feedback.value =
      error instanceof Error
        ? error.message
        : 'No fue posible registrar la consulta. Inténtalo nuevamente.';
    feedbackSuccess.value = false;
  } finally {
    submitting.value = false;
  }
}

onMounted(() => {
  loadCatalogs();
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

.form {
  display: grid;
  gap: 1.5rem;
}

.form__grid {
  display: grid;
  gap: 1.25rem;
  border: none;
  margin: 0;
  padding: 0;
}

@media (min-width: 960px) {
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

select,
textarea,
input[type='datetime-local'] {
  border-radius: 16px;
  border: 1px solid var(--border-subtle);
  padding: 0.85rem 1rem;
  font-size: 1rem;
  background: var(--surface-subtle);
  color: var(--text-primary);
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

textarea {
  resize: vertical;
}

select:focus,
textarea:focus,
input[type='datetime-local']:focus {
  outline: none;
  border-color: var(--brand-primary);
  box-shadow: 0 0 0 4px var(--focus-ring);
}

small {
  color: var(--text-muted);
}

.form__actions {
  display: flex;
  justify-content: flex-end;
}

.button.primary {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.6rem;
  background: linear-gradient(135deg, var(--brand-primary), var(--brand-secondary));
  color: #fff;
  border: none;
  border-radius: 999px;
  padding: 0.9rem 1.6rem;
  font-weight: 600;
  cursor: pointer;
  box-shadow: var(--shadow-md);
}

.button.primary:disabled {
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
</style>
