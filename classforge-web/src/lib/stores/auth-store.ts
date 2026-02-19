import { create } from "zustand";
import { setAccessToken, scheduleRefresh, getRefreshToken } from "@/lib/api/client";

export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  role: "OrgAdmin" | "ScheduleManager" | "Viewer";
  tenantId: string;
  languagePreference?: string | null;
}

interface AuthState {
  user: UserProfile | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (accessToken: string, refreshToken: string, user: UserProfile) => void;
  logout: () => void;
  setUser: (user: UserProfile) => void;
  setLoading: (loading: boolean) => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  isAuthenticated: false,
  isLoading: true,

  login: (accessToken, refreshToken, user) => {
    setAccessToken(accessToken);
    scheduleRefresh(accessToken);
    if (typeof window !== "undefined") {
      localStorage.setItem("refreshToken", refreshToken);
    }
    set({ user, isAuthenticated: true, isLoading: false });
  },

  logout: () => {
    setAccessToken(null);
    if (typeof window !== "undefined") {
      localStorage.removeItem("refreshToken");
    }
    set({ user: null, isAuthenticated: false, isLoading: false });
  },

  setUser: (user) => set({ user }),
  setLoading: (isLoading) => set({ isLoading }),
}));

// Restore auth state from refresh token on app init
export async function initAuth() {
  const store = useAuthStore.getState();
  const refreshToken = getRefreshToken();

  if (!refreshToken) {
    store.setLoading(false);
    return;
  }

  try {
    const response = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}api/v1/auth/refresh`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ accessToken: "", refreshToken }),
      }
    );

    if (!response.ok) {
      store.logout();
      return;
    }

    const data = await response.json();
    // Decode JWT to get user info
    const payload = parseJwt(data.accessToken);
    const user: UserProfile = {
      id: payload.sub,
      email: payload.email,
      displayName: payload.name || payload.email,
      role: payload.role,
      tenantId: payload.tenant_id,
      languagePreference: payload.language_preference,
    };

    store.login(data.accessToken, data.refreshToken, user);
  } catch {
    store.logout();
  }
}

function parseJwt(token: string) {
  const base64 = token.split(".")[1];
  return JSON.parse(atob(base64));
}
