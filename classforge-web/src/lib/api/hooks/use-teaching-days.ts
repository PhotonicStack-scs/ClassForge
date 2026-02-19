
"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

type TeachingDayResponse = components["schemas"]["TeachingDayResponse"];
type TimeSlotResponse = components["schemas"]["TimeSlotResponse"];
type CreateTeachingDayRequest = components["schemas"]["CreateTeachingDayRequest"];
type CreateTimeSlotRequest = components["schemas"]["CreateTimeSlotRequest"];

export function useTeachingDays() {
  return useQuery({
    queryKey: ["teaching-days"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/teaching-days");
      if (error) throw error;
      return (data ?? []) as TeachingDayResponse[];
    },
  });
}

export function useCreateTeachingDay() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateTeachingDayRequest) => {
      const { data, error } = await apiClient.POST("/api/v1/teaching-days", { body });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["teaching-days"] }),
  });
}

export function useUpdateTeachingDay() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, body }: { id: string; body: CreateTeachingDayRequest }) => {
      const { data, error } = await apiClient.PUT("/api/v1/teaching-days/{id}", {
        params: { path: { id } }, body,
      });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["teaching-days"] }),
  });
}

export function useTimeSlots(dayId: string) {
  return useQuery({
    queryKey: ["teaching-days", dayId, "time-slots"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET(
        "/api/v1/teaching-days/{dayId}/time-slots",
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
        "/api/v1/teaching-days/{dayId}/time-slots",
        { params: { path: { dayId } }, body }
      );
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["teaching-days", dayId, "time-slots"] }),
  });
}
