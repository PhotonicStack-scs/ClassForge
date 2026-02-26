
"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

type SchoolDayResponse = components["schemas"]["SchoolDayResponse"];
type TimeSlotResponse = components["schemas"]["TimeSlotResponse"];
type CreateSchoolDayRequest = components["schemas"]["CreateSchoolDayRequest"];
type UpdateSchoolDayRequest = components["schemas"]["UpdateSchoolDayRequest"];
type CreateTimeSlotRequest = components["schemas"]["CreateTimeSlotRequest"];
type BulkCreateTimeSlotsRequest = components["schemas"]["BulkCreateTimeSlotsRequest"];

export type { SchoolDayResponse, TimeSlotResponse };

export function useSchoolDays() {
  return useQuery({
    queryKey: ["school-days"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/school-days");
      if (error) throw error;
      return (data ?? []) as SchoolDayResponse[];
    },
  });
}

export function useCreateSchoolDay() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateSchoolDayRequest) => {
      const { data, error } = await apiClient.POST("/api/v1/school-days", { body });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["school-days"] }),
  });
}

export function useUpdateSchoolDay() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, body }: { id: string; body: UpdateSchoolDayRequest }) => {
      const { data, error } = await apiClient.PUT("/api/v1/school-days/{id}", {
        params: { path: { id } }, body,
      });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["school-days"] }),
  });
}

export function useDeleteSchoolDay() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await apiClient.DELETE("/api/v1/school-days/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["school-days"] }),
  });
}

export function useTimeSlots(dayId: string) {
  return useQuery({
    queryKey: ["school-days", dayId, "time-slots"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET(
        "/api/v1/school-days/{dayId}/time-slots",
        { params: { path: { dayId } } }
      );
      if (error) throw error;
      return (data ?? []) as TimeSlotResponse[];
    },
    enabled: !!dayId,
  });
}

export function useCreateTimeSlot(dayId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateTimeSlotRequest) => {
      const { data, error } = await apiClient.POST(
        "/api/v1/school-days/{dayId}/time-slots",
        { params: { path: { dayId } }, body }
      );
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["school-days", dayId, "time-slots"] }),
  });
}

export function useDeleteTimeSlot(dayId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await apiClient.DELETE(
        "/api/v1/school-days/{dayId}/time-slots/{id}",
        { params: { path: { dayId, id } } }
      );
      if (error) throw error;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["school-days", dayId, "time-slots"] }),
  });
}

export function useBulkCreateTimeSlots(dayId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: BulkCreateTimeSlotsRequest) => {
      const { data, error } = await apiClient.POST(
        "/api/v1/school-days/{dayId}/time-slots/bulk",
        { params: { path: { dayId } }, body }
      );
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["school-days", dayId, "time-slots"] }),
  });
}
