<template>
  <div class="dashboard" v-if="user">
    <section class="welcome">
      <div class="welcome__content">
        <p class="eyebrow">Panel clínico</p>
        <h1>Bienvenido, {{ primerNombre }}</h1>
        <p>
          Mantén el pulso de la operación de la clínica con indicadores claros y acciones rápidas.
          Puedes personalizar esta vista para mostrar los módulos más relevantes.
        </p>

        <dl class="user-meta">
          <div>
            <dt>Correo</dt>
            <dd>{{ user.correo }}</dd>
          </div>
          <div>
            <dt>Rol asignado</dt>
            <dd>{{ user.idMedico ? 'Médico asociado' : 'Administrador' }}</dd>
          </div>
          <div>
            <dt>Miembro desde</dt>
            <dd>{{ fechaCreacion }}</dd>
          </div>
          <div>
            <dt>Estado</dt>
            <dd :class="{ active: user.activo }">{{ user.activo ? 'Activo' : 'Inactivo' }}</dd>
          </div>
        </dl>

        <div class="welcome__actions">
          <RouterLink class="button ghost" :to="{ name: 'home' }">Ir al sitio</RouterLink>
          <button class="button danger" type="button" @click="handleLogout">Cerrar sesión</button>
        </div>
      </div>
      <div class="welcome__insight">
        <h2>Resumen del día</h2>
        <ul>
          <li>
            <strong>12</strong>
            Consultas agendadas
          </li>
          <li>
            <strong>5</strong>
            Pacientes nuevos
          </li>
          <li>
            <strong>3</strong>
            Seguimientos pendientes
          </li>
        </ul>
      </div>
    </section>

    <section class="widgets">
      <article class="widget">
        <h3>Consultas de hoy</h3>
        <p>
          Revisa las consultas programadas, asigna salas y verifica la disponibilidad de cada
          médico.
        </p>
        <button type="button">Ver agenda</button>
      </article>
      <article class="widget">
        <h3>Pacientes destacados</h3>
        <p>
          Mantente al tanto de los pacientes que requieren atención prioritaria o seguimiento
          personalizado.
        </p>
        <button type="button">Ver pacientes</button>
      </article>
      <article class="widget">
        <h3>Indicadores clínicos</h3>
        <p>
          Visualiza métricas clave como tiempos de espera, satisfacción y efectividad de
          tratamientos.
        </p>
        <button type="button">Ver reportes</button>
      </article>
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

const router = useRouter();
const { user, clearUser } = useAuthStore();

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
  display: flex;
  flex-direction: column;
  gap: 3rem;
}

.welcome {
  display: grid;
  gap: 2rem;
}

@media (min-width: 1024px) {
  .welcome {
    grid-template-columns: minmax(0, 1.4fr) minmax(0, 1fr);
    gap: 3rem;
    align-items: stretch;
  }
}

.welcome__content {
  background: var(--surface-raised);
  border-radius: 32px;
  padding: clamp(2rem, 5vw, 3.2rem);
  box-shadow: var(--shadow-xl);
  display: grid;
  gap: 1.5rem;
}

.welcome__content p {
  margin: 0;
  color: var(--text-muted);
}

.user-meta {
  display: grid;
  gap: 1rem;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  margin: 0;
}

.user-meta div {
  background: var(--surface-subtle);
  border-radius: 20px;
  padding: 1rem 1.25rem;
  border: 1px solid var(--border-subtle);
  display: grid;
  gap: 0.35rem;
}

.user-meta dt {
  text-transform: uppercase;
  font-size: 0.75rem;
  letter-spacing: 0.12em;
  color: var(--text-muted);
}

.user-meta dd {
  margin: 0;
  font-weight: 600;
}

.user-meta dd.active {
  color: var(--success-text);
}

.welcome__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
}

.welcome__insight {
  background: var(--surface-glass);
  border-radius: 32px;
  border: 1px solid var(--border-strong);
  padding: clamp(2rem, 5vw, 2.8rem);
  box-shadow: var(--shadow-lg);
  display: grid;
  gap: 1.25rem;
  backdrop-filter: blur(14px);
}

.welcome__insight h2 {
  margin: 0;
  font-size: clamp(1.6rem, 3vw, 2rem);
}

.welcome__insight ul {
  margin: 0;
  padding: 0;
  list-style: none;
  display: grid;
  gap: 1rem;
}

.welcome__insight li {
  display: grid;
  gap: 0.3rem;
  font-weight: 500;
}

.welcome__insight strong {
  font-size: 1.5rem;
  color: var(--brand-primary);
}

.widgets {
  display: grid;
  gap: 1.5rem;
}

@media (min-width: 900px) {
  .widgets {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

.widget {
  background: var(--surface-raised);
  border-radius: 24px;
  padding: 2rem;
  border: 1px solid var(--border-subtle);
  box-shadow: var(--shadow-md);
  display: grid;
  gap: 1rem;
}

.widget h3 {
  margin: 0;
}

.widget p {
  margin: 0;
  color: var(--text-muted);
}

.widget button {
  justify-self: start;
  background: transparent;
  border: none;
  color: var(--brand-primary);
  font-weight: 600;
  cursor: pointer;
  padding: 0;
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

