"use client";

import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

type TimetableViewResponse = components["schemas"]["TimetableViewResponse"];
type TimetableReportResponse = components["schemas"]["TimetableReportResponse"];

export function useTimetableByClass(
  timetableId: string,
  classId: string | null
) {
  return useQuery({
    queryKey: ["timetable-view", timetableId, "class", classId],
    queryFn: async () => {
      const { data, error } = await apiClient.GET(
        "/api/v1/timetables/{id}/by-class/{classId}",
        { params: { path: { id: timetableId, classId: classId! } } }
      );
      if (error) throw error;
      return data as TimetableViewResponse;
    },
    enabled: !!timetableId && !!classId,
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
  params?: { classId?: string; teacherId?: string }
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
