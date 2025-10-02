<template>
  <main class="container">
    <section class="hero">
      <h1>Sistema Clínico</h1>
      <p>
        Este es el punto de partida para el panel administrativo de la clínica.
        El frontend está construido con <strong>Vue 3 + TypeScript</strong> y consume los
        servicios expuestos por la API minimalista.
      </p>
      <button type="button" @click="saludar">Probar mensaje</button>
    </section>

    <section class="resultado" v-if="mensaje">
      <h2>Resultado</h2>
      <p>{{ mensaje }}</p>
    </section>

    <HelloWorld />
  </main>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import HelloWorld from './components/HelloWorld.vue';
import { showResultMessage } from './services/api';

const mensaje = ref('');

const saludar = async () => {
  const resultado = await showResultMessage('¡Bienvenido al sistema clínico!');
  if (resultado.value) {
    mensaje.value = resultado.message;
  }
};
</script>

<style scoped>
.container {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.hero {
  background-color: #fff;
  padding: 2rem;
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.08);
}

.resultado {
  background-color: #e0f2fe;
  border-left: 4px solid #0284c7;
  padding: 1.5rem 2rem;
  border-radius: 12px;
}

.resultado h2 {
  margin-bottom: 0.5rem;
}
</style>
