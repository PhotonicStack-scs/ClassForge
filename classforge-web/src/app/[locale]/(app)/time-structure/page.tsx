"use client";

import { useTeachingDays, useCreateTeachingDay, useTimeSlots, useCreateTimeSlot } from "@/lib/api/hooks/use-teaching-days";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

function DaySlots({ dayId }: { dayId: string }) {
  const { data: slots } = useTimeSlots(dayId);
  const createSlot = useCreateTimeSlot(dayId);
  const [n, setN] = useState(1);
  const [s, setS] = useState("08:00");
  const [e, setE] = useState("09:00");
  return (
    <div className="mt-3">
      <div className="flex gap-2 mb-2">
        <Input type="number" value={n} onChange={(ev) => setN(Number(ev.target.value))} className="w-20" />
        <Input type="time" value={s} onChange={(ev) => setS(ev.target.value)} />
        <Input type="time" value={e} onChange={(ev) => setE(ev.target.value)} />
        <Button size="sm" onClick={() => createSlot.mutate({ slotNumber: n, startTime: s, endTime: e, isBreak: false })}>Add</Button>
      </div>
      {slots?.map((sl) => (
        <div key={sl.id!} className="text-sm">Slot {sl.slotNumber}: {sl.startTime}-{sl.endTime}</div>
      ))}
    </div>
  );
}
export default function TimeStructurePage() {
  const { data: days, isLoading } = useTeachingDays();
  const createDay = useCreateTeachingDay();
  const [dow, setDow] = useState(1);
  
  const [exp, setExp] = useState<string | null>(null);
  if (isLoading) return <div className="p-8">Loading...</div>;
  return (
    <div className="container mx-auto p-8 max-w-3xl">
      <h1 className="text-2xl font-bold mb-6">Time Structure</h1>
      <Card className="mb-6">
        <CardHeader><CardTitle>Add Teaching Day</CardTitle></CardHeader>
        <CardContent>
          <div className="flex gap-2">
            <Input type="number" min={1} max={7} value={dow} onChange={(e) => setDow(Number(e.target.value))} className="w-20" />
            
            <Button onClick={() => createDay.mutate({ dayOfWeek: dow, isActive: true, sortOrder: dow })}>Add</Button>
          </div>
        </CardContent>
      </Card>
      <div className="space-y-4">
        {days?.map((day) => (
          <Card key={day.id ?? ""}>
            <CardContent className="pt-4">
              <div className="flex items-center justify-between">
                <span className="font-medium">{day.name ?? ""}</span>
                <Button variant="outline" size="sm" onClick={() => setExp(exp === day.id ? null : (day.id ?? null))}>Slots</Button>
              </div>
              {exp === day.id && day.id && <DaySlots dayId={day.id} />}
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
