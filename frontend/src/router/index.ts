import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean;
  }
}

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
      component: () => import('@/views/DashboardView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/consultas/crear',
      name: 'consultas-create',
      component: () => import('@/views/ConsultasCreateView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/consultas/historial',
      name: 'consultas-historial',
      component: () => import('@/views/ConsultasHistorialView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/medicos',
      name: 'medicos-admin',
      component: () => import('@/views/MedicosAdminView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/pacientes',
      name: 'pacientes-admin',
      component: () => import('@/views/PacientesAdminView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/usuarios',
      name: 'usuarios-admin',
      component: () => import('@/views/UsuariosAdminView.vue'),
      meta: { requiresAuth: true }
    }
  ],
  scrollBehavior() {
    return { top: 0 };
  }
});

router.beforeEach(to => {
  const { isAuthenticated } = useAuthStore();

  if (to.meta.requiresAuth && !isAuthenticated.value) {
    return {
      name: 'login',
      query: { redirect: to.fullPath }
    };
  }

  if (to.name === 'login' && isAuthenticated.value) {
    return { name: 'dashboard' };
  }
});

export default router;

