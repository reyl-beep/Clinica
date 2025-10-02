<template>
  <div class="dashboard" v-if="user">
    <aside class="dashboard__sidebar">
      <div class="sidebar__header">
        <p class="sidebar__eyebrow">Panel clínico</p>
        <h2>Gestión integral</h2>
      </div>

      <nav class="sidebar__nav" aria-label="Secciones principales">
        <ul>
          <li v-for="option in menuOptions" :key="option.label">
            <button type="button" class="sidebar__nav-item">
              <span class="sidebar__nav-title">{{ option.label }}</span>
              <span class="sidebar__nav-description">{{ option.description }}</span>
            </button>
          </li>
        </ul>
      </nav>

      <footer class="sidebar__footer">
        <RouterLink class="sidebar__link" :to="{ name: 'home' }">Ir al sitio público</RouterLink>
        <button class="sidebar__logout" type="button" @click="handleLogout">Cerrar sesión</button>
      </footer>
    </aside>

    <section class="dashboard__main" aria-labelledby="bienvenida">
      <header class="welcome" id="bienvenida">
        <p class="welcome__eyebrow">Bienvenido de nuevo</p>
        <h1 class="welcome__title">Hola, {{ primerNombre }}</h1>
        <p class="welcome__description">Nos alegra tenerte de regreso en Clínica del Rey.</p>
        <p class="welcome__meta">Miembro desde {{ fechaCreacion }}</p>
      </header>
    </section>
  </div>

  <div v-else class="empty-state">
    <h1>Tu sesión ha finalizado</h1>
    <p>Vuelve a iniciar sesión para acceder al panel de Clínica del Rey.</p>
    <RouterLink class="button primary" :to="{ name: 'login' }">Iniciar sesión</RouterLink>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

type MenuOption = {
  label: string;
  description: string;
};

const router = useRouter();
const { user, clearUser } = useAuthStore();

const menuOptions: MenuOption[] = [
  {
    label: 'Crear consultas',
    description: 'Selecciona médico y paciente, captura la información y guarda la consulta.'
  },
  {
    label: 'Historial de consultas',
    description: 'Revisa las consultas filtrando por médico, paciente o rangos de fechas.'
  },
  {
    label: 'Gestión de médicos',
    description: 'Crea, edita y activa o desactiva a los médicos de la clínica.'
  },
  {
    label: 'Gestión de usuarios',
    description: 'Administra usuarios del sistema y resguarda contraseñas en formato hash.'
  }
];

const primerNombre = computed(() => {
  const nombre = user.value?.nombreCompleto ?? '';
  return nombre.split(' ')[0] ?? nombre;
});

const fechaCreacion = computed(() => {
  if (!user.value) {
    return '';
  }

  return new Date(user.value.fechaCreacion).toLocaleDateString('es-MX', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
});

const handleLogout = async () => {
  clearUser();
  await router.push({ name: 'login' });
};
</script>

<style scoped>
.dashboard {
  display: grid;
  gap: clamp(1.5rem, 4vw, 2.5rem);
  padding: clamp(1.5rem, 4vw, 3rem);
  background: var(--surface-raised);
  border-radius: 28px;
  box-shadow: var(--shadow-xl);
}

@media (min-width: 960px) {
  .dashboard {
    grid-template-columns: minmax(260px, 320px) minmax(0, 1fr);
    min-height: clamp(460px, 72vh, 640px);
  }
}

.dashboard__sidebar {
  display: grid;
  gap: 2rem;
  background: var(--surface-base);
  border-radius: 24px;
  padding: clamp(1.5rem, 3vw, 2.25rem);
  border: 1px solid var(--border-subtle);
  box-shadow: var(--shadow-lg);
}

.sidebar__header h2 {
  margin: 0;
  font-size: 1.5rem;
}

.sidebar__eyebrow {
  margin: 0 0 0.25rem;
  font-size: 0.85rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.12em;
  color: var(--text-muted);
}

.sidebar__nav ul {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.75rem;
}

.sidebar__nav-item {
  width: 100%;
  text-align: left;
  background: var(--surface-raised);
  border: 1px solid transparent;
  border-radius: 18px;
  padding: 1rem 1.25rem;
  display: grid;
  gap: 0.35rem;
  transition: border-color 0.2s ease, box-shadow 0.2s ease, transform 0.2s ease;
  cursor: pointer;
}

.sidebar__nav-item:hover {
  border-color: var(--brand-primary);
  box-shadow: var(--shadow-lg);
  transform: translateY(-2px);
}

.sidebar__nav-title {
  font-weight: 600;
  font-size: 1.05rem;
}

.sidebar__nav-description {
  color: var(--text-muted);
  font-size: 0.95rem;
}

.sidebar__footer {
  display: grid;
  gap: 0.75rem;
}

.sidebar__link {
  color: var(--brand-primary);
  font-weight: 600;
  text-decoration: none;
}

.sidebar__link:hover {
  text-decoration: underline;
}

.sidebar__logout {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.85rem 1.25rem;
  border-radius: 999px;
  font-weight: 600;
  background: linear-gradient(135deg, #ef4444, #dc2626);
  color: #fff;
  border: none;
  cursor: pointer;
  box-shadow: var(--shadow-lg);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.sidebar__logout:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-xl);
}

.dashboard__main {
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--surface-base);
  border-radius: 24px;
  padding: clamp(2rem, 6vw, 3.5rem);
  border: 1px solid var(--border-subtle);
  box-shadow: var(--shadow-lg);
}

.welcome {
  display: grid;
  gap: 0.75rem;
  text-align: center;
  max-width: 32rem;
}

.welcome__eyebrow {
  margin: 0;
  font-size: 0.95rem;
  font-weight: 600;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--text-muted);
}

.welcome__title {
  margin: 0;
  font-size: clamp(2rem, 6vw, 2.8rem);
  font-weight: 700;
}

.welcome__description {
  margin: 0;
  color: var(--text-muted);
  font-size: 1.05rem;
}

.welcome__meta {
  margin: 0;
  font-size: 0.95rem;
  color: var(--text-muted);
}

.empty-state {
  text-align: center;
  display: grid;
  gap: 1.5rem;
  background: var(--surface-raised);
  border-radius: 28px;
  padding: clamp(2rem, 5vw, 3rem);
  box-shadow: var(--shadow-xl);
}

.empty-state h1 {
  margin: 0;
}

.empty-state p {
  margin: 0;
  color: var(--text-muted);
}

.button.primary {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.85rem 1.5rem;
  border-radius: 999px;
  font-weight: 600;
  background: linear-gradient(135deg, var(--brand-primary), var(--brand-secondary));
  color: #fff;
  border: none;
  cursor: pointer;
  box-shadow: var(--shadow-lg);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.button.primary:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-xl);
}
</style>
