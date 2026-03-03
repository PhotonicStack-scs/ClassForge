
"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

type YearResponse = components["schemas"]["YearResponse"];
type ClassResponse = components["schemas"]["ClassResponse"];
type CreateYearRequest = components["schemas"]["CreateYearRequest"];
type UpdateYearRequest = components["schemas"]["UpdateYearRequest"];
type CreateClassRequest = components["schemas"]["CreateClassRequest"];
type UpdateClassRequest = components["schemas"]["UpdateClassRequest"];

export function useYears() {
  return useQuery({
    queryKey: ["years"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/years");
      if (error) throw error;
      return (data ?? []) as YearResponse[];
    },
  });
}

export function useCreateYear() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateYearRequest) => {
      const { data, error } = await apiClient.POST("/api/v1/years", { body });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["years"] }),
  });
}

export function useUpdateYear() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, body }: { id: string; body: UpdateYearRequest }) => {
      const { data, error } = await apiClient.PUT("/api/v1/years/{id}", {
        params: { path: { id } }, body,
      });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["years"] }),
  });
}

export function useDeleteYear() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await apiClient.DELETE("/api/v1/years/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["years"] }),
  });
}

export function useBulkCreateYears() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: components["schemas"]["BulkCreateYearsRequest"]) => {
      const { data, error } = await apiClient.POST("/api/v1/years/bulk", { body });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["years"] }),
  });
}

export function useClasses(yearId: string) {
  return useQuery({
    queryKey: ["years", yearId, "classes"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET(
        "/api/v1/years/{yearId}/classes",
        { params: { path: { yearId } } }
      );
      if (error) throw error;
      return (data ?? []) as ClassResponse[];
    },
    enabled: !!yearId,
  });
}

export function useCreateClass(yearId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateClassRequest) => {
      const { data, error } = await apiClient.POST(
        "/api/v1/years/{yearId}/classes",
        { params: { path: { yearId } }, body }
      );
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["years", yearId, "classes"] }),
  });
}

export function useUpdateClass(yearId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, body }: { id: string; body: UpdateClassRequest }) => {
      const { data, error } = await apiClient.PUT(
        "/api/v1/years/{yearId}/classes/{id}",
        { params: { path: { yearId, id } }, body }
      );
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["years", yearId, "classes"] }),
  });
}

export function useDeleteClass(yearId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await apiClient.DELETE(
        "/api/v1/years/{yearId}/classes/{id}",
        { params: { path: { yearId, id } } }
      );
      if (error) throw error;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["years", yearId, "classes"] }),
  });
}
