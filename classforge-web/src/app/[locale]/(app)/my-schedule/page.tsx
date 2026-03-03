"use client";

import { useQuery } from "@tanstack/react-query";
import { useTranslations } from "next-intl";
import { apiClient } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

type TeacherResponse = components["schemas"]["TeacherResponse"];

function useMyTeacher() {
  return useQuery({
    queryKey: ["my-teacher"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/auth/my-teacher");
      if (error) throw error;
      return data as TeacherResponse;
    },
  });
}

export default function MySchedulePage() {
  const tc = useTranslations("common");
  const t = useTranslations("timetable");
  const { data: teacher, isLoading, error } = useMyTeacher();

  if (isLoading) {
    return <div className="p-8 text-muted-foreground">{tc("loading")}</div>;
  }

  if (error || teacher == null) {
    return (
      <div className="p-8 space-y-2">
        <h1 className="text-2xl font-bold">{tc("mySchedule")}</h1>
        <p className="text-muted-foreground">
          {t("noTeacherProfile")}
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">{tc("mySchedule")}</h1>
        <p className="text-muted-foreground mt-1">
          {teacher.name ?? "-"}
          {teacher.email ? " " + teacher.email : ""}
        </p>
      </div>
      <div className="rounded-md border p-6 text-center text-muted-foreground">
        {t("noPublishedSchedule")}
      </div>
    </div>
  );
}
