
"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

type TimetableResponse = components["schemas"]["TimetableResponse"];
type CreateTimetableRequest = components["schemas"]["CreateTimetableRequest"];

export function useTimetables() {
  return useQuery({
    queryKey: ["timetables"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/timetables");
      if (error) throw error;
      return (data ?? []) as TimetableResponse[];
    },
  });
}

export function useTimetable(id: string) {
  return useQuery({
    queryKey: ["timetables", id],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/timetables/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
      return data as TimetableResponse;
    },
    enabled: !!id,
    refetchInterval: (query) =>
      query.state.data?.status === "Generating" ? 2000 : false,
  });
}

export function useCreateTimetable() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateTimetableRequest) => {
      const { data, error } = await apiClient.POST("/api/v1/timetables", { body });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["timetables"] }),
  });
}

export function useDeleteTimetable() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await apiClient.DELETE("/api/v1/timetables/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["timetables"] }),
  });
}

export function usePublishTimetable() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { data, error } = await apiClient.POST("/api/v1/timetables/{id}/publish", {
        params: { path: { id } },
      });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["timetables"] }),
  });
}

export function usePreflight() {
  return useMutation({
    mutationFn: async () => {
      const { data, error } = await apiClient.POST("/api/v1/timetables/preflight");
      if (error) throw error;
      return data;
    },
  });
}

type UpdateTimetableEntryRequest = components["schemas"]["UpdateTimetableEntryRequest"];

export function useUpdateTimetableEntry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({
      timetableId,
      entryId,
      body,
    }: {
      timetableId: string;
      entryId: string;
      body: UpdateTimetableEntryRequest;
    }) => {
      const { data, error } = await apiClient.PUT(
        "/api/v1/timetables/{id}/entries/{entryId}",
        {
          params: { path: { id: timetableId, entryId } },
          body,
        }
      );
      if (error) throw error;
      return data;
    },
    onSuccess: (_data, { timetableId }) => {
      qc.invalidateQueries({ queryKey: ["timetable-entries", timetableId] });
      qc.invalidateQueries({ queryKey: ["timetable-view", timetableId] });
    },
  });
}
