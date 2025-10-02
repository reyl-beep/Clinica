<template>
  <div class="dashboard" v-if="user">
    <header class="dashboard__header">
      <p class="dashboard__eyebrow">Panel clínico</p>
      <h1>Hola, {{ primerNombre }}</h1>
      <p class="dashboard__subtitle">
        Nos alegra verte de nuevo. Selecciona uno de los menús principales para continuar con tus tareas del día.
      </p>
      <p class="dashboard__meta">Miembro desde {{ fechaCreacion }}</p>
    </header>

    <section class="dashboard__menus">
      <article class="menu-card" v-for="menu in mainMenus" :key="menu.title">
        <div class="menu-card__header">
          <h2>{{ menu.title }}</h2>
          <p>{{ menu.description }}</p>
        </div>
        <ul class="menu-card__actions">
          <li v-for="action in menu.actions" :key="action.label">
            <button type="button" class="menu-card__action">
              <span class="menu-card__action-title">{{ action.label }}</span>
              <span class="menu-card__action-hint">{{ action.hint }}</span>
            </button>
          </li>
        </ul>
      </article>
    </section>

    <footer class="dashboard__footer">
      <RouterLink class="button ghost" :to="{ name: 'home' }">Ir al sitio</RouterLink>
      <button class="button danger" type="button" @click="handleLogout">Cerrar sesión</button>
    </footer>
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

type MenuAction = {
  label: string;
  hint: string;
};

type MainMenu = {
  title: string;
  description: string;
  actions: MenuAction[];
};

const router = useRouter();
const { user, clearUser } = useAuthStore();

const mainMenus: MainMenu[] = [
  {
    title: 'Gestión clínica',
    description: 'Centraliza las consultas, pacientes y seguimiento médico en un solo lugar.',
    actions: [
      {
        label: 'Consultas',
        hint: 'Programa, revisa y da seguimiento a las citas.'
      },
      {
        label: 'Pacientes',
        hint: 'Actualiza expedientes y mantén la información de tus pacientes al día.'
      }
    ]
  },
  {
    title: 'Administración',
    description: 'Administra tu equipo y controla el acceso al sistema de la clínica.',
    actions: [
      {
        label: 'Médicos',
        hint: 'Gestiona el personal médico y sus especialidades.'
      },
      {
        label: 'Usuarios del sistema',
        hint: 'Define roles y permisos para cada miembro del equipo.'
      }
    ]
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
  gap: 2rem;
  padding: clamp(2rem, 5vw, 3rem);
  background: var(--surface-raised);
  border-radius: 28px;
  box-shadow: var(--shadow-xl);
}

.dashboard__header {
  display: grid;
  gap: 0.75rem;
}

.dashboard__eyebrow {
  margin: 0;
  font-size: 0.85rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.12em;
  color: var(--text-muted);
}

.dashboard__header h1 {
  margin: 0;
  font-size: clamp(2rem, 6vw, 2.6rem);
  font-weight: 700;
}

.dashboard__subtitle {
  margin: 0;
  color: var(--text-muted);
  max-width: 52ch;
}

.dashboard__meta {
  margin: 0;
  font-size: 0.95rem;
  color: var(--text-muted);
}

.dashboard__menus {
  display: grid;
  gap: 1.5rem;
}

@media (min-width: 900px) {
  .dashboard__menus {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

.menu-card {
  background: var(--surface-base);
  border-radius: 24px;
  padding: clamp(1.5rem, 4vw, 2rem);
  border: 1px solid var(--border-subtle);
  box-shadow: var(--shadow-md);
  display: grid;
  gap: 1.5rem;
}

.menu-card__header h2 {
  margin: 0 0 0.5rem;
  font-size: 1.5rem;
}

.menu-card__header p {
  margin: 0;
  color: var(--text-muted);
}

.menu-card__actions {
  margin: 0;
  padding: 0;
  list-style: none;
  display: grid;
  gap: 0.75rem;
}

.menu-card__action {
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

.menu-card__action:hover {
  border-color: var(--brand-primary);
  box-shadow: var(--shadow-lg);
  transform: translateY(-2px);
}

.menu-card__action-title {
  font-weight: 600;
  font-size: 1.05rem;
}

.menu-card__action-hint {
  color: var(--text-muted);
  font-size: 0.95rem;
}

.dashboard__footer {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
}

.button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.85rem 1.5rem;
  border-radius: 999px;
  font-weight: 600;
  border: 1px solid transparent;
  transition: transform 0.2s ease, box-shadow 0.2s ease, border-color 0.2s ease;
}

.button.ghost {
  border-color: var(--border-strong);
  color: var(--text-primary);
}

.button.ghost:hover {
  border-color: var(--brand-primary);
  color: var(--brand-primary);
}

.button.danger {
  background: linear-gradient(135deg, #ef4444, #dc2626);
  color: #fff;
  box-shadow: var(--shadow-lg);
}

.button.primary {
  background: linear-gradient(135deg, var(--brand-primary), var(--brand-secondary));
  color: #fff;
  box-shadow: var(--shadow-lg);
}

.button:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-xl);
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
</style>
