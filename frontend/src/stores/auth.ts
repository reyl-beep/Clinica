import { computed, readonly, ref } from 'vue';
import type { Usuario } from '@/types/Usuario';

const storageKey = 'clinica-del-rey:user';
const user = ref<Usuario | null>(null);
let initialized = false;

function loadFromStorage() {
  if (initialized || typeof window === 'undefined') {
    return;
  }

  initialized = true;
  try {
    const serialized = window.localStorage.getItem(storageKey);
    if (serialized) {
      user.value = JSON.parse(serialized) as Usuario;
    }
  } catch (error) {
    console.error('No fue posible recuperar la sesión almacenada', error);
    window.localStorage.removeItem(storageKey);
  }
}

function persistUser(value: Usuario | null) {
  if (typeof window === 'undefined') {
    return;
  }

  if (value) {
    window.localStorage.setItem(storageKey, JSON.stringify(value));
  } else {
    window.localStorage.removeItem(storageKey);
  }
}

export function useAuthStore() {
  loadFromStorage();

  const setUser = (value: Usuario | null) => {
    user.value = value;
    persistUser(value);
  };

  const clearUser = () => setUser(null);

  return {
    user: readonly(user),
    isAuthenticated: computed(() => user.value !== null),
    setUser,
    clearUser
  };
}

