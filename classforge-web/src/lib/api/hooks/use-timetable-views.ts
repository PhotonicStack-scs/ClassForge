"use client";

import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

type TimetableViewResponse = components["schemas"]["TimetableViewResponse"];
type TimetableReportResponse = components["schemas"]["TimetableReportResponse"];

export function useTimetableByGroup(
  timetableId: string,
  groupId: string | null
) {
  return useQuery({
    queryKey: ["timetable-view", timetableId, "group", groupId],
    queryFn: async () => {
      const { data, error } = await apiClient.GET(
        "/api/v1/timetables/{id}/by-group/{groupId}",
        { params: { path: { id: timetableId, groupId: groupId! } } }
      );
      if (error) throw error;
      return data as TimetableViewResponse;
    },
    enabled: !!timetableId && !!groupId,
  });
}

export function useTimetableByTeacher(
  timetableId: string,
  teacherId: string | null
) {
  return useQuery({
    queryKey: ["timetable-view", timetableId, "teacher", teacherId],
    queryFn: async () => {
      const { data, error } = await apiClient.GET(
        "/api/v1/timetables/{id}/by-teacher/{teacherId}",
        { params: { path: { id: timetableId, teacherId: teacherId! } } }
      );
      if (error) throw error;
      return data as TimetableViewResponse;
    },
    enabled: !!timetableId && !!teacherId,
  });
}

export function useTimetableReport(timetableId: string) {
  return useQuery({
    queryKey: ["timetable-report", timetableId],
    queryFn: async () => {
      const { data, error } = await apiClient.GET(
        "/api/v1/timetables/{id}/report",
        { params: { path: { id: timetableId } } }
      );
      if (error) throw error;
      return (data ?? []) as TimetableReportResponse[];
    },
    enabled: !!timetableId,
  });
}

export function useTimetableEntries(
  timetableId: string,
  params?: { groupId?: string; teacherId?: string }
) {
  return useQuery({
    queryKey: ["timetable-entries", timetableId, params],
    queryFn: async () => {
      const { data, error } = await apiClient.GET(
        "/api/v1/timetables/{id}/entries",
        {
          params: {
            path: { id: timetableId },
            query: params,
          },
        }
      );
      if (error) throw error;
      return data ?? [];
    },
    enabled: !!timetableId,
  });
}
