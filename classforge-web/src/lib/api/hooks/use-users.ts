"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

type UserResponse = components["schemas"]["UserResponse"];
type CreateUserRequest = components["schemas"]["CreateUserRequest"];
type UpdateUserRequest = components["schemas"]["UpdateUserRequest"];

export function useUsers() {
  return useQuery({
    queryKey: ["users"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/users");
      if (error) throw error;
      return (data ?? []) as UserResponse[];
    },
  });
}

export function useCreateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateUserRequest) => {
      const { data, error } = await apiClient.POST("/api/v1/users", { body });
      if (error) throw error;
      return data as UserResponse;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}

export function useUpdateUserRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, body }: { id: string; body: UpdateUserRequest }) => {
      const { data, error } = await apiClient.PUT("/api/v1/users/{id}", {
        params: { path: { id } },
        body,
      });
      if (error) throw error;
      return data as UserResponse;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}

export function useDeleteUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await apiClient.DELETE("/api/v1/users/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}
