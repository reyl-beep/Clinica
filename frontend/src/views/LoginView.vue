<template>
  <div class="login">
    <section class="login-card">
      <header>
        <p class="eyebrow">Acceso seguro</p>
        <h1>Inicia sesión en Clínica del Rey</h1>
        <p class="subtitle">
          Ingresa tu correo institucional y contraseña para acceder al panel de control.
        </p>
      </header>

      <form @submit.prevent="handleSubmit" novalidate>
        <label class="field">
          <span>Correo electrónico</span>
          <input
            v-model.trim="form.correo"
            type="email"
            name="correo"
            inputmode="email"
            autocomplete="email"
            placeholder="tu.nombre@clinicadelrey.com"
            required
          />
        </label>

        <label class="field">
          <span>Contraseña</span>
          <input
            v-model="form.password"
            type="password"
            name="password"
            autocomplete="current-password"
            placeholder="••••••••"
            required
          />
        </label>

        <p v-if="errorMessage" class="alert" role="alert">{{ errorMessage }}</p>

        <button class="submit" type="submit" :disabled="loading">
          <span v-if="loading" class="loader" aria-hidden="true"></span>
          <span>{{ loading ? 'Validando...' : 'Ingresar al panel' }}</span>
        </button>
      </form>

      <p class="helper">
        ¿Necesitas ayuda? <a href="mailto:soporte@clinicadelrey.com">Contacta a soporte</a>
      </p>
    </section>

    <aside class="login-side">
      <h2>Confianza y calidez humana</h2>
      <p>
        Cada miembro del equipo cuenta con acceso personalizado para ofrecer una atención
        excepcional a nuestros pacientes.
      </p>
      <ul>
        <li>Autenticación protegida para el personal autorizado.</li>
        <li>Disponibilidad 24/7 desde cualquier dispositivo.</li>
        <li>Integración completa con tu flujo clínico.</li>
      </ul>
      <RouterLink class="back-link" :to="{ name: 'home' }">Regresar al inicio</RouterLink>
    </aside>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { login } from '@/services/api';
import { useAuthStore } from '@/stores/auth';

const router = useRouter();
const { setUser } = useAuthStore();

const form = reactive({
  correo: '',
  password: ''
});

const loading = ref(false);
const errorMessage = ref('');

const handleSubmit = async () => {
  if (!form.correo || !form.password) {
    errorMessage.value = 'Ingresa tu correo y contraseña para continuar.';
    return;
  }

  loading.value = true;
  errorMessage.value = '';

  try {
    const resultado = await login({
      correo: form.correo,
      password: form.password
    });

    if (resultado.value && resultado.data) {
      setUser(resultado.data);
      await router.push({ name: 'dashboard' });
    } else {
      errorMessage.value = resultado.message || 'No fue posible iniciar sesión.';
    }
  } catch (error) {
    errorMessage.value =
      error instanceof Error ? error.message : 'Ocurrió un error inesperado. Inténtalo más tarde.';
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.login {
  display: grid;
  gap: 2.5rem;
}

@media (min-width: 960px) {
  .login {
    grid-template-columns: minmax(0, 1fr) minmax(0, 1.1fr);
    gap: 3.5rem;
    align-items: center;
  }
}

.login-card {
  background: var(--surface-raised);
  padding: clamp(2rem, 4vw, 3rem);
  border-radius: 28px;
  box-shadow: var(--shadow-xl);
  display: grid;
  gap: 1.75rem;
}

.eyebrow {
  text-transform: uppercase;
  font-size: 0.8rem;
  letter-spacing: 0.3em;
  color: var(--brand-primary);
  font-weight: 600;
}

h1 {
  font-size: clamp(2rem, 4vw, 2.8rem);
  margin: 0;
}

.subtitle {
  color: var(--text-muted);
  margin: 0;
}

form {
  display: grid;
  gap: 1.25rem;
}

.field {
  display: grid;
  gap: 0.5rem;
}

.field span {
  font-weight: 600;
}

input {
  border-radius: 16px;
  border: 1px solid var(--border-subtle);
  padding: 0.85rem 1rem;
  font-size: 1rem;
  background: var(--surface-subtle);
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
  color: var(--text-primary);
}

input:focus {
  outline: none;
  border-color: var(--brand-primary);
  box-shadow: 0 0 0 4px var(--focus-ring);
}

.alert {
  background: var(--error-bg);
  color: var(--error-text);
  padding: 0.9rem 1.1rem;
  border-radius: 16px;
  border: 1px solid var(--error-border);
  font-weight: 600;
  box-shadow: var(--shadow-md);
}

.submit {
  border: none;
  border-radius: 999px;
  padding: 1rem 1.5rem;
  font-weight: 600;
  font-size: 1rem;
  display: inline-flex;
  gap: 0.75rem;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  background: linear-gradient(135deg, var(--brand-primary), var(--brand-secondary));
  color: #fff;
  box-shadow: var(--shadow-lg);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.submit:disabled {
  opacity: 0.7;
  cursor: wait;
  box-shadow: none;
}

.submit:not(:disabled):hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-xl);
}

.loader {
  width: 1rem;
  height: 1rem;
  border-radius: 50%;
  border: 2px solid rgba(255, 255, 255, 0.6);
  border-top-color: #fff;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.helper {
  margin: 0;
  color: var(--text-muted);
}

.helper a {
  color: var(--brand-primary);
  font-weight: 600;
}

.login-side {
  background: var(--surface-glass);
  border-radius: 32px;
  padding: clamp(2rem, 5vw, 3.5rem);
  border: 1px solid var(--border-strong);
  box-shadow: var(--shadow-lg);
  display: grid;
  gap: 1.25rem;
  backdrop-filter: blur(14px);
}

.login-side h2 {
  font-size: clamp(1.6rem, 3vw, 2.2rem);
  margin: 0;
}

.login-side p {
  color: var(--text-muted);
  margin: 0;
}

.login-side ul {
  margin: 0;
  padding-left: 1.2rem;
  display: grid;
  gap: 0.7rem;
}

.back-link {
  justify-self: start;
  font-weight: 600;
  color: var(--brand-primary);
}
</style>

