"use client";

import { useEffect } from "react";
import { initAuth } from "@/lib/stores/auth-store";
import { useAuthStore } from "@/lib/stores/auth-store";

export function AuthInitializer() {
  const setLoading = useAuthStore((s) => s.setLoading);

  useEffect(() => {
    initAuth().finally(() => setLoading(false));
  }, [setLoading]);

  return null;
}
