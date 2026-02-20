
"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

type SubjectResponse = components["schemas"]["SubjectResponse"];
type CreateSubjectRequest = components["schemas"]["CreateSubjectRequest"];
type UpdateSubjectRequest = components["schemas"]["UpdateSubjectRequest"];

export function useSubjects() {
  return useQuery({
    queryKey: ["subjects"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/subjects");
      if (error) throw error;
      return (data ?? []) as SubjectResponse[];
    },
  });
}

export function useCreateSubject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateSubjectRequest) => {
      const { data, error } = await apiClient.POST("/api/v1/subjects", { body });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["subjects"] }),
  });
}

export function useUpdateSubject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, body }: { id: string; body: UpdateSubjectRequest }) => {
      const { data, error } = await apiClient.PUT("/api/v1/subjects/{id}", {
        params: { path: { id } }, body,
      });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["subjects"] }),
  });
}

export function useDeleteSubject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await apiClient.DELETE("/api/v1/subjects/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["subjects"] }),
  });
}
