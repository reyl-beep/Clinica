<template>
  <div class="app-shell">
    <header class="topbar">
      <RouterLink class="brand" :to="{ name: 'home' }">
        <span class="brand__icon" aria-hidden="true">
          <svg viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path
              d="M14 26L32 6L50 26L44 30L52 38L32 58L12 38L20 30L14 26Z"
              stroke="currentColor"
              stroke-width="4"
              stroke-linecap="round"
              stroke-linejoin="round"
            />
            <circle cx="32" cy="30" r="6" fill="currentColor" />
          </svg>
        </span>
        <div class="brand__copy">
          <span class="brand__name">Clínica del Rey</span>
          <span class="brand__tagline">Excelencia y calidez en cada consulta</span>
        </div>
      </RouterLink>

      <nav class="nav">
        <template v-for="item in navigationItems">
          <RouterLink
            v-if="item.type === 'link'"
            :key="item.label"
            class="nav__link"
            :class="{ 'is-active': activeRoute === item.name }"
            :to="{ name: item.name }"
          >
            {{ item.label }}
          </RouterLink>
          <button
            v-else
            :key="item.label"
            class="nav__link nav__link--action"
            type="button"
            @click="handleLogout"
          >
            {{ item.label }}
          </button>
        </template>
      </nav>

      <button class="theme-toggle" type="button" @click="toggleTheme" :aria-label="themeAriaLabel">
        <span class="theme-toggle__icon" aria-hidden="true">
          <svg
            v-if="isDark"
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="1.6"
          >
            <path
              d="M21 12.79A9 9 0 0 1 11.21 3 7 7 0 1 0 21 12.79Z"
              stroke-linecap="round"
              stroke-linejoin="round"
            />
          </svg>
          <svg
            v-else
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="1.6"
          >
            <circle cx="12" cy="12" r="5" />
            <path
              stroke-linecap="round"
              d="M12 1v2m0 18v2m11-11h-2M3 12H1m18.36 6.36-1.42-1.42M6.05 6.05 4.64 4.64m12.72 0-1.41 1.41M6.05 17.95l-1.41 1.41"
            />
          </svg>
        </span>
        <span class="theme-toggle__label">{{ isDark ? 'Modo noche' : 'Modo día' }}</span>
      </button>
    </header>

    <main class="content">
      <RouterView />
    </main>

    <footer class="footer">
      <p>© {{ currentYear }} Clínica del Rey. Cuidamos de ti con tecnología y humanidad.</p>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

type Theme = 'light' | 'dark';

const route = useRoute();
const router = useRouter();
const { isAuthenticated, clearUser } = useAuthStore();
const theme = ref<Theme>('light');
const isDark = computed(() => theme.value === 'dark');
const themeStorageKey = 'clinica-del-rey:theme';
const currentYear = new Date().getFullYear();
const activeRoute = computed(() => (route.name ?? '').toString());

type NavigationItem =
  | { type: 'link'; name: string; label: string }
  | { type: 'action'; label: string };

const navigationItems = computed<NavigationItem[]>(() => {
  if (!isAuthenticated.value) {
    return [
      { type: 'link', name: 'home', label: 'Inicio' },
      { type: 'link', name: 'login', label: 'Iniciar sesión' }
    ];
  }

  return [
    { type: 'link', name: 'home', label: 'Inicio' },
    { type: 'link', name: 'dashboard', label: 'Dashboard' },
    { type: 'action', label: 'Cerrar sesión' }
  ];
});

const applyTheme = (value: Theme) => {
  if (typeof document !== 'undefined') {
    document.documentElement.setAttribute('data-theme', value);
  }

  if (typeof localStorage !== 'undefined') {
    localStorage.setItem(themeStorageKey, value);
  }
};

const toggleTheme = () => {
  theme.value = theme.value === 'light' ? 'dark' : 'light';
};

const themeAriaLabel = computed(() => `Cambiar a modo ${isDark.value ? 'día' : 'noche'}`);

onMounted(() => {
  if (typeof window === 'undefined') {
    return;
  }

  const stored = window.localStorage.getItem(themeStorageKey);
  if (stored === 'light' || stored === 'dark') {
    theme.value = stored;
  } else if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
    theme.value = 'dark';
  }

  applyTheme(theme.value);
});

watch(theme, value => {
  applyTheme(value);
});

const handleLogout = async () => {
  clearUser();
  await router.push({ name: 'home' });
};
</script>

<style scoped>
.app-shell {
  display: flex;
  flex-direction: column;
  gap: clamp(2rem, 3vw, 2.8rem);
  min-height: calc(100vh - clamp(2rem, 4vw, 3.5rem));
}

.topbar {
  display: flex;
  flex-wrap: wrap;
  gap: 1.5rem;
  align-items: center;
  justify-content: space-between;
  padding: 1.2rem 1.5rem;
  background: var(--surface-glass);
  border-radius: 32px;
  border: 1px solid var(--border-strong);
  box-shadow: var(--shadow-lg);
  position: sticky;
  top: clamp(1rem, 2vw, 1.5rem);
  z-index: 10;
  backdrop-filter: blur(18px);
}

.brand {
  display: inline-flex;
  align-items: center;
  gap: 0.9rem;
  color: var(--text-primary);
}

.brand__icon {
  width: 3rem;
  height: 3rem;
  border-radius: 16px;
  background: linear-gradient(135deg, var(--brand-primary), var(--brand-secondary));
  display: inline-flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  box-shadow: var(--shadow-md);
}

.brand__icon svg {
  width: 1.6rem;
  height: 1.6rem;
}

.brand__copy {
  display: grid;
  gap: 0.2rem;
}

.brand__name {
  font-size: 1.35rem;
  font-weight: 700;
  letter-spacing: 0.02em;
}

.brand__tagline {
  font-size: 0.85rem;
  color: var(--text-muted);
}

.nav {
  display: inline-flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.25rem;
  background: var(--surface-subtle);
  border-radius: 999px;
  border: 1px solid var(--border-subtle);
}

.nav__link {
  padding: 0.55rem 1.2rem;
  border-radius: 999px;
  font-weight: 600;
  color: var(--text-muted);
  transition: background-color 0.2s ease, color 0.2s ease, transform 0.2s ease;
}

.nav__link--action {
  border: none;
  background: none;
  cursor: pointer;
}

.nav__link:hover {
  color: var(--text-primary);
  transform: translateY(-1px);
}

.nav__link.is-active {
  background: linear-gradient(135deg, var(--brand-primary), var(--brand-secondary));
  color: #fff;
  box-shadow: var(--shadow-md);
}

.theme-toggle {
  display: inline-flex;
  align-items: center;
  gap: 0.75rem;
  border-radius: 999px;
  padding: 0.6rem 1.1rem;
  border: 1px solid var(--border-subtle);
  background: var(--surface-subtle);
  color: var(--text-primary);
  cursor: pointer;
  font-weight: 600;
  box-shadow: var(--shadow-md);
  transition: transform 0.2s ease, box-shadow 0.2s ease, border-color 0.2s ease;
}

.theme-toggle:hover {
  transform: translateY(-1px);
  box-shadow: var(--shadow-lg);
  border-color: var(--brand-primary);
}

.theme-toggle__icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 1.8rem;
  height: 1.8rem;
}

.theme-toggle__icon svg {
  width: 100%;
  height: 100%;
}

.theme-toggle__label {
  font-size: 0.95rem;
}

.content {
  display: block;
}

.footer {
  text-align: center;
  padding: 1.5rem 1rem 0;
  color: var(--text-muted);
  font-size: 0.95rem;
}

@media (max-width: 860px) {
  .topbar {
    flex-direction: column;
    align-items: stretch;
  }

  .nav {
    justify-content: center;
  }

  .theme-toggle {
    align-self: center;
  }
}
</style>

