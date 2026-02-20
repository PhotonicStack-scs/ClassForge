
"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

type TeacherResponse = components["schemas"]["TeacherResponse"];
type TeacherQualificationResponse = components["schemas"]["TeacherQualificationResponse"];
type TeacherDayConfigResponse = components["schemas"]["TeacherDayConfigResponse"];
type TeacherSlotBlockResponse = components["schemas"]["TeacherSlotBlockResponse"];
type CreateTeacherRequest = components["schemas"]["CreateTeacherRequest"];
type UpdateTeacherRequest = components["schemas"]["UpdateTeacherRequest"];
type CreateTeacherQualificationRequest = components["schemas"]["CreateTeacherQualificationRequest"];
type CreateTeacherDayConfigRequest = components["schemas"]["CreateTeacherDayConfigRequest"];
type CreateTeacherSlotBlockRequest = components["schemas"]["CreateTeacherSlotBlockRequest"];

export function useTeachers() {
  return useQuery({
    queryKey: ["teachers"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/teachers");
      if (error) throw error;
      return (data ?? []) as TeacherResponse[];
    },
  });
}

export function useTeacher(id: string) {
  return useQuery({
    queryKey: ["teachers", id],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/teachers/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
      return data as TeacherResponse;
    },
    enabled: !!id,
  });
}

export function useCreateTeacher() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateTeacherRequest) => {
      const { data, error } = await apiClient.POST("/api/v1/teachers", { body });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["teachers"] }),
  });
}

export function useUpdateTeacher() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, body }: { id: string; body: UpdateTeacherRequest }) => {
      const { data, error } = await apiClient.PUT("/api/v1/teachers/{id}", {
        params: { path: { id } }, body,
      });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["teachers"] }),
  });
}

export function useQualifications(teacherId: string) {
  return useQuery({
    queryKey: ["teachers", teacherId, "qualifications"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET(
        "/api/v1/teachers/{teacherId}/qualifications",
        { params: { path: { teacherId } } }
      );
      if (error) throw error;
      return (data ?? []) as TeacherQualificationResponse[];
    },
    enabled: !!teacherId,
  });
}

export function useCreateQualification(teacherId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateTeacherQualificationRequest) => {
      const { data, error } = await apiClient.POST(
        "/api/v1/teachers/{teacherId}/qualifications",
        { params: { path: { teacherId } }, body }
      );
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["teachers", teacherId, "qualifications"] }),
  });
}

export function useTeacherDayConfig(teacherId: string) {
  return useQuery({
    queryKey: ["teachers", teacherId, "day-config"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET(
        "/api/v1/teachers/{teacherId}/day-config",
        { params: { path: { teacherId } } }
      );
      if (error) throw error;
      return (data ?? []) as TeacherDayConfigResponse[];
    },
    enabled: !!teacherId,
  });
}

export function useBlockedSlots(teacherId: string) {
  return useQuery({
    queryKey: ["teachers", teacherId, "blocked-slots"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET(
        "/api/v1/teachers/{teacherId}/blocked-slots",
        { params: { path: { teacherId } } }
      );
      if (error) throw error;
      return (data ?? []) as TeacherSlotBlockResponse[];
    },
    enabled: !!teacherId,
  });
}

export function useCreateBlockedSlot(teacherId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateTeacherSlotBlockRequest) => {
      const { data, error } = await apiClient.POST(
        "/api/v1/teachers/{teacherId}/blocked-slots",
        { params: { path: { teacherId } }, body }
      );
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["teachers", teacherId, "blocked-slots"] }),
  });
}

export function useDeleteTeacher() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await apiClient.DELETE("/api/v1/teachers/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["teachers"] }),
  });
}
