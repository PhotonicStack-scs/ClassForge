"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import { useAuthStore, initAuth } from "@/lib/stores/auth-store";
import type { components } from "@/lib/api/schema";
import { useEffect } from "react";

type LoginRequest = components["schemas"]["LoginRequest"];
type RegisterRequest = components["schemas"]["RegisterRequest"];

function parseJwt(token: string) {
  const base64 = token.split(".")[1];
  return JSON.parse(atob(base64));
}

// ASP.NET Core stores role under the long URI claim key when using ClaimTypes.Role
const ASPNET_ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

function extractRole(payload: Record<string, string>): "OrgAdmin" | "ScheduleManager" | "Viewer" {
  return (payload.role ?? payload[ASPNET_ROLE_CLAIM] ?? "Viewer") as "OrgAdmin" | "ScheduleManager" | "Viewer";
}

export function useLogin() {
  const { login } = useAuthStore();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: LoginRequest) => {
      const { data: result, error } = await apiClient.POST("/api/v1/auth/login", {
        body: data,
      });
      if (error) throw error;
      return result!;
    },
    onSuccess: (data) => {
      const accessToken = data.accessToken ?? "";
      const refreshToken = data.refreshToken ?? "";
      const payload = parseJwt(accessToken);
      login(accessToken, refreshToken, {
        id: payload.sub,
        email: payload.email,
        displayName: payload.name || payload.email,
        role: extractRole(payload),
        tenantId: payload.tenant_id,
        languagePreference: payload.language_preference,
      });
      // Set session cookie for proxy/middleware
      document.cookie = `cf_has_session=1; path=/; max-age=${60 * 60 * 24 * 30}; SameSite=Lax`;
      queryClient.clear();
    },
  });
}

export function useRegister() {
  const { login } = useAuthStore();

  return useMutation({
    mutationFn: async (data: RegisterRequest) => {
      const { data: result, error } = await apiClient.POST("/api/v1/auth/register", {
        body: data,
      });
      if (error) throw error;
      return result!;
    },
    onSuccess: (data) => {
      const accessToken = data.accessToken ?? "";
      const refreshToken = data.refreshToken ?? "";
      const payload = parseJwt(accessToken);
      login(accessToken, refreshToken, {
        id: payload.sub,
        email: payload.email,
        displayName: payload.name || payload.email,
        role: extractRole(payload),
        tenantId: payload.tenant_id,
        languagePreference: payload.language_preference,
      });
      document.cookie = `cf_has_session=1; path=/; max-age=${60 * 60 * 24 * 30}; SameSite=Lax`;
    },
  });
}

export function useInitAuth() {
  const { setLoading } = useAuthStore();

  useEffect(() => {
    initAuth().finally(() => setLoading(false));
  }, [setLoading]);
}
