import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "./schema";

let accessToken: string | null = null;
let refreshPromise: Promise<string | null> | null = null;

export function setAccessToken(token: string | null) {
  accessToken = token;
}

export function getAccessToken() {
  return accessToken;
}

async function doRefresh(): Promise<string | null> {
  try {
    const response = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}api/v1/auth/refresh`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ accessToken, refreshToken: getRefreshToken() }),
      }
    );
    if (!response.ok) return null;
    const data = await response.json();
    accessToken = data.accessToken;
    if (typeof window !== "undefined") {
      localStorage.setItem("refreshToken", data.refreshToken);
    }
    scheduleRefresh(data.accessToken);
    return data.accessToken;
  } catch {
    return null;
  }
}

export function getRefreshToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("refreshToken");
}

let refreshTimer: ReturnType<typeof setTimeout> | null = null;

export function scheduleRefresh(token: string) {
  if (refreshTimer) clearTimeout(refreshTimer);
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    const expiresAt = payload.exp * 1000;
    const refreshAt = expiresAt - 60_000; // 60s before expiry
    const delay = refreshAt - Date.now();
    if (delay > 0) {
      refreshTimer = setTimeout(() => {
        refreshPromise = doRefresh().finally(() => {
          refreshPromise = null;
        });
      }, delay);
    }
  } catch {
    // ignore invalid token
  }
}

const authMiddleware: Middleware = {
  async onRequest({ request }) {
    if (accessToken) {
      request.headers.set("Authorization", `Bearer ${accessToken}`);
    }
    return request;
  },
  async onResponse({ response, request }) {
    if (response.status === 401) {
      // Try refresh
      if (!refreshPromise) {
        refreshPromise = doRefresh().finally(() => {
          refreshPromise = null;
        });
      }
      const newToken = await refreshPromise;
      if (newToken) {
        // Retry original request with new token
        const retried = new Request(request, {
          headers: { ...Object.fromEntries(request.headers), Authorization: `Bearer ${newToken}` },
        });
        return fetch(retried);
      } else {
        // Redirect to login
        if (typeof window !== "undefined") {
          accessToken = null;
          localStorage.removeItem("refreshToken");
          window.location.href = "/login";
        }
      }
    }
    if (response.status === 403) {
      // Dynamic import to avoid circular deps — toast fires from the app layer
      if (typeof window !== "undefined") {
        window.dispatchEvent(new CustomEvent("classforge:forbidden"));
      }
    }
    return response;
  },
};

export const apiClient = createClient<paths>({
  baseUrl: process.env.NEXT_PUBLIC_API_URL,
});

apiClient.use(authMiddleware);
