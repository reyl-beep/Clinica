<template>
  <section class="page">
    <header class="page__header">
      <p class="page__eyebrow">Usuarios</p>
      <h1>Administración de usuarios</h1>
      <p class="page__description">
        Registra y controla el acceso al sistema clínico. Asigna los permisos necesarios a cada
        integrante del equipo y mantén la seguridad de las credenciales.
      </p>
    </header>

    <div class="layout">
      <div class="card">
        <header class="card__header">
          <h2>{{ isEditing ? 'Editar usuario' : 'Registrar nuevo usuario' }}</h2>
          <p class="card__description">
            Completa el formulario con los datos de acceso. Al actualizar un usuario se requiere
            establecer una nueva contraseña.
          </p>
        </header>

        <form class="form" @submit.prevent="handleSubmit">
          <fieldset class="form__grid" :disabled="submitting">
            <label class="field">
              <span>Correo electrónico *</span>
              <input v-model.trim="form.correo" type="email" required />
            </label>

            <label class="field">
              <span>Nombre completo *</span>
              <input v-model.trim="form.nombreCompleto" type="text" required />
            </label>

            <label class="field">
              <span>{{ isEditing ? 'Nueva contraseña *' : 'Contraseña *' }}</span>
              <input v-model="form.password" type="password" required />
            </label>

            <label class="field">
              <span>Médico asociado</span>
              <select v-model="form.idMedico">
                <option value="">Sin asociación</option>
                <option v-for="medico in medicos" :key="medico.id" :value="medico.id">
                  {{ nombreMedico(medico) }}
                </option>
              </select>
            </label>
          </fieldset>

          <div class="form__actions">
            <button class="button ghost" type="button" @click="resetForm" :disabled="submitting">
              Cancelar
            </button>
            <button class="button primary" type="submit" :disabled="submitting">
              <span v-if="submitting" class="loader" aria-hidden="true"></span>
              <span>{{ submitting ? 'Guardando...' : isEditing ? 'Actualizar usuario' : 'Crear usuario' }}</span>
            </button>
          </div>
        </form>

        <p v-if="feedback" class="alert" :class="{ 'alert--success': feedbackSuccess }" role="alert">
          {{ feedback }}
        </p>
      </div>

      <div class="card">
        <header class="card__header">
          <h2>Usuarios registrados</h2>
          <p class="card__description">Administra los permisos y accesos del personal.</p>
        </header>

        <div v-if="loading" class="placeholder">Cargando usuarios...</div>

        <table v-else class="table">
          <thead>
            <tr>
              <th scope="col">Correo</th>
              <th scope="col">Nombre</th>
              <th scope="col">Médico asignado</th>
              <th scope="col">Estado</th>
              <th scope="col">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="usuario in usuarios" :key="usuario.id">
              <td>{{ usuario.correo }}</td>
              <td>{{ usuario.nombreCompleto }}</td>
              <td>{{ nombreMedicoAsignado(usuario.idMedico) }}</td>
              <td>
                <span class="badge" :class="{ 'badge--inactive': !usuario.activo }">
                  {{ usuario.activo ? 'Activo' : 'Inactivo' }}
                </span>
              </td>
              <td class="actions">
                <button class="link" type="button" @click="startEdit(usuario)">Editar</button>
                <button
                  class="link link--danger"
                  type="button"
                  @click="handleDeactivate(usuario.id)"
                  :disabled="!usuario.activo || submitting"
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
import {
  createUsuario,
  deleteUsuario,
  fetchMedicos,
  fetchUsuarios,
  updateUsuario
} from '@/services/api';
import type { Medico } from '@/types/Medico';
import type { Usuario } from '@/types/Usuario';

interface UsuarioForm {
  id: number | null;
  correo: string;
  password: string;
  nombreCompleto: string;
  idMedico: string;
}

const usuarios = ref<Usuario[]>([]);
const medicos = ref<Medico[]>([]);
const loading = ref(false);
const submitting = ref(false);
const feedback = ref('');
const feedbackSuccess = ref(false);

const form = reactive<UsuarioForm>({
  id: null,
  correo: '',
  password: '',
  nombreCompleto: '',
  idMedico: ''
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

const nombreMedicoAsignado = (id: number | null) => {
  if (!id) {
    return 'Sin asignar';
  }

  const medico = medicos.value.find(item => item.id === id);
  return medico ? nombreMedico(medico) : 'Sin asignar';
};

function resetForm() {
  form.id = null;
  form.correo = '';
  form.password = '';
  form.nombreCompleto = '';
  form.idMedico = '';
  feedback.value = '';
}

function populateForm(usuario: Usuario) {
  form.id = usuario.id;
  form.correo = usuario.correo;
  form.password = '';
  form.nombreCompleto = usuario.nombreCompleto;
  form.idMedico = usuario.idMedico ? String(usuario.idMedico) : '';
}

async function loadData() {
  loading.value = true;
  const [usuariosResultado, medicosResultado] = await Promise.all([
    fetchUsuarios(),
    fetchMedicos()
  ]);

  if (usuariosResultado.value && usuariosResultado.data) {
    usuarios.value = usuariosResultado.data;
  } else {
    feedback.value = usuariosResultado.message;
    feedbackSuccess.value = false;
  }

  if (medicosResultado.value && medicosResultado.data) {
    medicos.value = medicosResultado.data.filter(medico => medico.activo);
  }

  loading.value = false;
}

function buildPayload() {
  return {
    correo: form.correo,
    password: form.password,
    nombreCompleto: form.nombreCompleto,
    idMedico: form.idMedico ? Number(form.idMedico) : null
  };
}

async function handleSubmit() {
  if (!form.password) {
    feedback.value = 'Por seguridad, ingresa una contraseña válida.';
    feedbackSuccess.value = false;
    return;
  }

  submitting.value = true;
  feedback.value = '';

  const payload = buildPayload();
  const resultado = isEditing.value && form.id !== null
    ? await updateUsuario(form.id, payload)
    : await createUsuario(payload);

  feedback.value = resultado.message;
  feedbackSuccess.value = resultado.value;

  if (resultado.value) {
    await loadData();
    resetForm();
  }

  submitting.value = false;
}

function startEdit(usuario: Usuario) {
  populateForm(usuario);
  feedback.value = '';
}

async function handleDeactivate(id: number) {
  submitting.value = true;
  const resultado = await deleteUsuario(id);
  feedback.value = resultado.message;
  feedbackSuccess.value = resultado.value;

  if (resultado.value) {
    await loadData();
  }

  submitting.value = false;
}

onMounted(() => {
  loadData();
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
}

.field {
  display: grid;
  gap: 0.5rem;
}

.field span {
  font-weight: 600;
}

input[type='email'],
input[type='text'],
input[type='password'],
select {
  border-radius: 16px;
  border: 1px solid var(--border-subtle);
  padding: 0.85rem 1rem;
  font-size: 1rem;
  background: var(--surface-subtle);
  color: var(--text-primary);
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

input:focus,
select:focus {
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
