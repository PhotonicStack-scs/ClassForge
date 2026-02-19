import pathlib

def w(p,c):
    pathlib.Path(p).parent.mkdir(parents=True,exist_ok=True)
    open(p,chr(119),encoding=chr(117)+chr(116)+chr(102)+chr(45)+chr(56)).write(c)
    print(chr(87)+chr(58)+p)

w('src/lib/api/hooks/use-grades.ts', '''
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
''')

w('src/lib/api/hooks/use-subjects.ts', '''
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
''')

w('src/lib/api/hooks/use-rooms.ts', '''
"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

type RoomResponse = components["schemas"]["RoomResponse"];
type CreateRoomRequest = components["schemas"]["CreateRoomRequest"];
type UpdateRoomRequest = components["schemas"]["UpdateRoomRequest"];

export function useRooms() {
  return useQuery({
    queryKey: ["rooms"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/rooms");
      if (error) throw error;
      return (data ?? []) as RoomResponse[];
    },
  });
}

export function useCreateRoom() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateRoomRequest) => {
      const { data, error } = await apiClient.POST("/api/v1/rooms", { body });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["rooms"] }),
  });
}

export function useUpdateRoom() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, body }: { id: string; body: UpdateRoomRequest }) => {
      const { data, error } = await apiClient.PUT("/api/v1/rooms/{id}", {
        params: { path: { id } }, body,
      });
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["rooms"] }),
  });
}

export function useDeleteRoom() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await apiClient.DELETE("/api/v1/rooms/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["rooms"] }),
  });
}
''')

w('src/lib/api/hooks/use-teaching-days.ts', '''
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
''')

w('src/lib/api/hooks/use-teachers.ts', '''
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
''')

w('src/lib/api/hooks/use-timetables.ts', '''
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
  return useQuery({
    queryKey: ["preflight"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/timetables/preflight");
      if (error) throw error;
      return data;
    },
    enabled: false,
  });
}
''')

