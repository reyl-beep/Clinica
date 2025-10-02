<template>
  <section class="card">
    <h2>¿Qué sigue?</h2>
    <ol>
      <li>Crea componentes para administrar médicos, pacientes y consultas.</li>
      <li>Consume los endpoints expuestos en <code>/api</code>.</li>
      <li>Valida las respuestas usando la clase <code>Resultado</code>.</li>
    </ol>
  </section>
</template>

<style scoped>
.card {
  background: #fff;
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.08);
}

ol {
  margin: 1rem 0 0;
  padding-left: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

code {
  background-color: rgba(15, 23, 42, 0.08);
  padding: 0.2rem 0.4rem;
  border-radius: 4px;
}
</style>
