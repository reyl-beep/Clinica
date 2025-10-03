import { computed, readonly, ref } from 'vue';
import type { AuthSession } from '@/types/AuthSession';
import type { Usuario } from '@/types/Usuario';

const storageKey = 'clinica-del-rey:session';
const legacyStorageKey = 'clinica-del-rey:user';
const session = ref<AuthSession | null>(null);
let initialized = false;

function loadFromStorage() {
  if (initialized || typeof window === 'undefined') {
    return;
  }

  initialized = true;
  try {
    const serialized = window.localStorage.getItem(storageKey);
    if (serialized) {
      session.value = JSON.parse(serialized) as AuthSession;
      return;
    }

    if (window.localStorage.getItem(legacyStorageKey)) {
      window.localStorage.removeItem(legacyStorageKey);
    }
  } catch (error) {
    console.error('No fue posible recuperar la sesión almacenada', error);
    window.localStorage.removeItem(storageKey);
    window.localStorage.removeItem(legacyStorageKey);
  }
}

function persistSession(value: AuthSession | null) {
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

  const setSession = (value: AuthSession | null) => {
    session.value = value;
    persistSession(value);
  };

  const clearSession = () => setSession(null);

  const user = computed<Usuario | null>(() => session.value?.usuario ?? null);
  const token = computed(() => session.value?.token ?? '');

  return {
    session: readonly(session),
    user,
    token,
    isAuthenticated: computed(() => session.value !== null),
    setSession,
    clearSession
  };
}

