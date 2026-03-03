"use client";

import { useTranslations } from "next-intl";
import { useTeacher, useQualifications, useCreateQualification, useBlockedSlots, useCreateBlockedSlot } from "@/lib/api/hooks/use-teachers";
import { useSubjects } from "@/lib/api/hooks/use-subjects";
import { use, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function TeacherDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const t = useTranslations("teachers");
  const tc = useTranslations("common");
  const { data: teacher, isLoading } = useTeacher(id);
  const { data: qualifications } = useQualifications(id);
  const { data: blocked } = useBlockedSlots(id);
  const { data: subjects } = useSubjects();
  const createQual = useCreateQualification(id);
  const createBlocked = useCreateBlockedSlot(id);
  const [subjectId, setSubjectId] = useState("");
  const [timeSlotId, setTimeSlotId] = useState("");
  const [reason, setReason] = useState("");

  if (isLoading) return <div className="p-8">{tc("loading")}</div>;
  if (!teacher) return <div className="p-8">{t("teacherNotFound")}</div>;

  return (
    <div className="container mx-auto p-8 max-w-3xl">
      <h1 className="text-2xl font-bold mb-2">{teacher.name}</h1>
      <p className="text-muted-foreground mb-6">{teacher.email}</p>
      <div className="grid gap-6">
        <Card>
          <CardHeader><CardTitle>{t("qualifications")}</CardTitle></CardHeader>
          <CardContent>
            <div className="flex gap-2 mb-3">
              <select value={subjectId} onChange={(e) => setSubjectId(e.target.value)} className="border rounded px-2 py-1 text-sm">
                <option value="">{t("selectSubject")}</option>
                {subjects?.map((s) => (
                  <option key={s.id!} value={s.id}>{s.name}</option>
                ))}
              </select>
              <Button size="sm" onClick={() => createQual.mutate({ subjectId })} disabled={!subjectId}>{tc("add")}</Button>
            </div>
            <div className="space-y-1">
              {qualifications?.map((q) => (
                <div key={q.id!} className="text-sm">{q.subjectName}</div>
              ))}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>{t("blockedSlots")}</CardTitle></CardHeader>
          <CardContent>
            <div className="flex gap-2 mb-3">
              <Input placeholder={t("timeSlotId")} value={timeSlotId} onChange={(e) => setTimeSlotId(e.target.value)} />
              <Input placeholder={t("reason")} value={reason} onChange={(e) => setReason(e.target.value)} />
              <Button size="sm" onClick={() => createBlocked.mutate({ timeSlotId, reason })} disabled={!timeSlotId}>{t("block")}</Button>
            </div>
            {blocked?.map((b) => (
              <div key={b.id!} className="text-sm">{b.reason || t("blocked")}</div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
