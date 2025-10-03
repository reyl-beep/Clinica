import { createRouter, createWebHistory } from 'vue-router';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('@/views/HomeView.vue')
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue')
    },
    {
      path: '/dashboard',
      name: 'dashboard',
      component: () => import('@/views/DashboardView.vue')
    },
    {
      path: '/consultas/crear',
      name: 'consultas-create',
      component: () => import('@/views/ConsultasCreateView.vue')
    },
    {
      path: '/consultas/historial',
      name: 'consultas-historial',
      component: () => import('@/views/ConsultasHistorialView.vue')
    },
    {
      path: '/medicos',
      name: 'medicos-admin',
      component: () => import('@/views/MedicosAdminView.vue')
    },
    {
      path: '/usuarios',
      name: 'usuarios-admin',
      component: () => import('@/views/UsuariosAdminView.vue')
    }
  ],
  scrollBehavior() {
    return { top: 0 };
  }
});

export default router;

