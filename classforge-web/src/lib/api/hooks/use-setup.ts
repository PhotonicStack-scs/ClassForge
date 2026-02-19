"use client";

import { useMutation } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

type UpdateSetupProgressRequest =
  components["schemas"]["UpdateSetupProgressRequest"];

export function useUpdateSetupProgress() {
  return useMutation({
    mutationFn: async (data: UpdateSetupProgressRequest) => {
      const { data: result, error } = await apiClient.PUT(
        "/api/v1/school/setup-progress",
        { body: data }
      );
      if (error) throw error;
      return result;
    },
  });
}
