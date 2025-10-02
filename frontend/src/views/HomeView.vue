<template>
  <div class="home">
    <section class="hero">
      <div class="hero__content">
        <p class="eyebrow">Bienvenido a Clínica del Rey</p>
        <h1>Gestión médica moderna para tu equipo de salud</h1>
        <p class="description">
          Centraliza la administración de tus médicos, pacientes y consultas en un entorno
          seguro, intuitivo y accesible desde cualquier dispositivo.
        </p>
        <div class="actions">
          <RouterLink class="button primary" :to="{ name: 'login' }">Iniciar sesión</RouterLink>
          <a class="button ghost" href="#features">Explorar características</a>
        </div>
        <dl class="stats">
          <div>
            <dt>Pacientes atendidos</dt>
            <dd>+1,200</dd>
          </div>
          <div>
            <dt>Profesionales activos</dt>
            <dd>86</dd>
          </div>
          <div>
            <dt>Consultas registradas</dt>
            <dd>9,540</dd>
          </div>
        </dl>
      </div>
      <div class="hero__card">
        <h2>Experiencia centrada en el paciente</h2>
        <p>
          Nuestra plataforma está diseñada para agilizar cada paso del proceso clínico y ofrecer
          a tu equipo información clara y en tiempo real.
        </p>
        <ul>
          <li>Panel inteligente con indicadores clave.</li>
          <li>Historial clínico digitalizado y seguro.</li>
          <li>Recordatorios y seguimiento personalizado.</li>
        </ul>
      </div>
    </section>

    <section id="features" class="features">
      <h2>Herramientas que marcan la diferencia</h2>
      <p class="subtitle">
        Diseñadas para optimizar la operación de una clínica moderna y ofrecer un servicio
        excepcional a cada paciente.
      </p>
      <div class="feature-grid">
        <article class="feature">
          <span class="icon" aria-hidden="true">💙</span>
          <h3>Agenda inteligente</h3>
          <p>Coordina horarios, evita solapamientos y recibe alertas automáticas.</p>
        </article>
        <article class="feature">
          <span class="icon" aria-hidden="true">🔒</span>
          <h3>Seguridad clínica</h3>
          <p>Cumple con estándares de protección de datos y controla accesos.</p>
        </article>
        <article class="feature">
          <span class="icon" aria-hidden="true">📊</span>
          <h3>Indicadores en tiempo real</h3>
          <p>Visualiza métricas clave para tomar decisiones informadas.</p>
        </article>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router';
</script>

<style scoped>
.home {
  display: flex;
  flex-direction: column;
  gap: 4rem;
}

.hero {
  display: grid;
  gap: 2rem;
  align-items: stretch;
}

@media (min-width: 960px) {
  .hero {
    grid-template-columns: minmax(0, 1.3fr) minmax(0, 1fr);
    gap: 3rem;
    align-items: center;
  }
}

.hero__content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  background: var(--surface-raised);
  border-radius: 28px;
  padding: clamp(2rem, 5vw, 3rem);
  box-shadow: var(--shadow-xl);
}

.eyebrow {
  text-transform: uppercase;
  letter-spacing: 0.25em;
  font-weight: 600;
  color: var(--brand-primary);
}

h1 {
  font-size: clamp(2.5rem, 5vw, 3.6rem);
  line-height: 1.05;
  font-weight: 700;
}

.description {
  font-size: 1.125rem;
  color: var(--text-muted);
}

.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
}

.button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.85rem 1.8rem;
  border-radius: 999px;
  font-weight: 600;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  border: 1px solid transparent;
}

.button.primary {
  background: linear-gradient(135deg, var(--brand-primary), var(--brand-secondary));
  color: #fff;
  box-shadow: var(--shadow-lg);
}

.button.primary:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-xl);
}

.button.ghost {
  border-color: var(--border-strong);
  color: var(--text-primary);
}

.button.ghost:hover {
  border-color: var(--brand-primary);
  color: var(--brand-primary);
}

.stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1rem;
  margin: 0;
}

.stats div {
  background: var(--surface-subtle);
  padding: 1.25rem 1.5rem;
  border-radius: 20px;
  display: grid;
  gap: 0.35rem;
  border: 1px solid var(--border-subtle);
}

.stats dt {
  font-size: 0.85rem;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  color: var(--text-muted);
}

.stats dd {
  font-size: 1.7rem;
  font-weight: 700;
  margin: 0;
  color: var(--text-primary);
}

.hero__card {
  background: var(--surface-glass);
  padding: clamp(2rem, 4vw, 2.8rem);
  border-radius: 32px;
  backdrop-filter: blur(14px);
  border: 1px solid var(--border-strong);
  box-shadow: var(--shadow-lg);
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.hero__card h2 {
  font-size: 1.75rem;
  margin: 0;
}

.hero__card p {
  color: var(--text-muted);
}

.hero__card ul {
  margin: 0;
  padding-left: 1.2rem;
  display: grid;
  gap: 0.75rem;
}

.features {
  display: grid;
  gap: 1.5rem;
}

.features h2 {
  font-size: clamp(2rem, 4vw, 2.6rem);
  text-align: center;
}

.subtitle {
  text-align: center;
  margin: 0 auto;
  max-width: 700px;
  color: var(--text-muted);
}

.feature-grid {
  display: grid;
  gap: 1.5rem;
}

@media (min-width: 768px) {
  .feature-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

.feature {
  background: var(--surface-raised);
  border-radius: 24px;
  padding: 1.8rem;
  box-shadow: var(--shadow-md);
  display: grid;
  gap: 0.9rem;
  border: 1px solid var(--border-subtle);
}

.icon {
  font-size: 2rem;
}

.feature p {
  color: var(--text-muted);
}
</style>

