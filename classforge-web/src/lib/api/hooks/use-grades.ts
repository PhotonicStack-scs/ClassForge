
"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

type GradeResponse = components["schemas"]["GradeResponse"];
type GroupResponse = components["schemas"]["GroupResponse"];
type CreateGradeRequest = components["schemas"]["CreateGradeRequest"];
type UpdateGradeRequest = components["schemas"]["UpdateGradeRequest"];
type CreateGroupRequest = components["schemas"]["CreateGroupRequest"];

export function useGrades() {
  return useQuery({
    queryKey: ["grades"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/grades");
      if (error) throw error;
      return (data ?? []) as GradeResponse[];
    },
  });
}

export function useCreateGrade() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateGradeRequest) => {
      const { data, error } = await apiClient.POST("/api/v1/grades", { body });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["grades"] }),
  });
}

export function useUpdateGrade() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, body }: { id: string; body: UpdateGradeRequest }) => {
      const { data, error } = await apiClient.PUT("/api/v1/grades/{id}", {
        params: { path: { id } }, body,
      });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["grades"] }),
  });
}

export function useDeleteGrade() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await apiClient.DELETE("/api/v1/grades/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["grades"] }),
  });
}

export function useGroups(gradeId: string) {
  return useQuery({
    queryKey: ["grades", gradeId, "groups"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET(
        "/api/v1/grades/{gradeId}/groups",
        { params: { path: { gradeId } } }
      );
      if (error) throw error;
      return (data ?? []) as GroupResponse[];
    },
    enabled: !!gradeId,
  });
}

export function useCreateGroup(gradeId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateGroupRequest) => {
      const { data, error } = await apiClient.POST(
        "/api/v1/grades/{gradeId}/groups",
        { params: { path: { gradeId } }, body }
      );
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["grades", gradeId, "groups"] }),
  });
}
