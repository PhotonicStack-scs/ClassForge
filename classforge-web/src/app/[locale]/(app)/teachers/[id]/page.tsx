"use client";

import { useTeacher, useQualifications, useCreateQualification, useBlockedSlots, useCreateBlockedSlot } from "@/lib/api/hooks/use-teachers";
import { useSubjects } from "@/lib/api/hooks/use-subjects";
import { use, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function TeacherDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { data: teacher, isLoading } = useTeacher(id);
  const { data: qualifications } = useQualifications(id);
  const { data: blocked } = useBlockedSlots(id);
  const { data: subjects } = useSubjects();
  const createQual = useCreateQualification(id);
  const createBlocked = useCreateBlockedSlot(id);
  const [subjectId, setSubjectId] = useState("");
  const [timeSlotId, setTimeSlotId] = useState("");
  const [reason, setReason] = useState("");

  if (isLoading) return <div className="p-8">Loading...</div>;
  if (!teacher) return <div className="p-8">Teacher not found</div>;

  return (
    <div className="container mx-auto p-8 max-w-3xl">
      <h1 className="text-2xl font-bold mb-2">{teacher.name}</h1>
      <p className="text-muted-foreground mb-6">{teacher.email}</p>
      <div className="grid gap-6">
        <Card>
          <CardHeader><CardTitle>Qualifications</CardTitle></CardHeader>
          <CardContent>
            <div className="flex gap-2 mb-3">
              <select value={subjectId} onChange={(e) => setSubjectId(e.target.value)} className="border rounded px-2 py-1 text-sm">
                <option value="">Select subject</option>
                {subjects?.map((s) => (
                  <option key={s.id!} value={s.id}>{s.name}</option>
                ))}
              </select>
              <Button size="sm" onClick={() => createQual.mutate({ subjectId })} disabled={!subjectId}>Add</Button>
            </div>
            <div className="space-y-1">
              {qualifications?.map((q) => (
                <div key={q.id!} className="text-sm">{q.subjectName}</div>
              ))}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Blocked Slots</CardTitle></CardHeader>
          <CardContent>
            <div className="flex gap-2 mb-3">
              <Input placeholder="Time slot ID" value={timeSlotId} onChange={(e) => setTimeSlotId(e.target.value)} />
              <Input placeholder="Reason" value={reason} onChange={(e) => setReason(e.target.value)} />
              <Button size="sm" onClick={() => createBlocked.mutate({ timeSlotId, reason })} disabled={!timeSlotId}>Block</Button>
            </div>
            {blocked?.map((b) => (
              <div key={b.id!} className="text-sm">{b.reason || "Blocked"}</div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
