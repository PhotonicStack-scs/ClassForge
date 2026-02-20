"use client";

import { useState } from "react";
import { useWizardStore } from "@/lib/stores/wizard-store";
import {
  useTeachingDays,
  useCreateTeachingDay,
  useUpdateTeachingDay,
  useTimeSlots,
  useCreateTimeSlot,
} from "@/lib/api/hooks/use-teaching-days";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Plus, Clock } from "lucide-react";
import { toast } from "sonner";

const WEEKDAYS = [
  { dow: 1, label: "Monday" },
  { dow: 2, label: "Tuesday" },
  { dow: 3, label: "Wednesday" },
  { dow: 4, label: "Thursday" },
  { dow: 5, label: "Friday" },
];

function TimeSlotList({ dayId }: { dayId: string }) {
  const { data: slots = [] } = useTimeSlots(dayId);
  const createSlot = useCreateTimeSlot(dayId);
  const [start, setStart] = useState("08:00");
  const [end, setEnd] = useState("08:45");

  async function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    try {
      await createSlot.mutateAsync({
        slotNumber: slots.length + 1,
        startTime: start,
        endTime: end,
        isBreak: false,
      });
    } catch {
      toast.error("Failed to add time slot");
    }
  }

  return (
    <div className="mt-2 ml-4 space-y-2 border-l-2 border-muted pl-4">
      {slots.length > 0 && (
        <div className="space-y-1">
          {slots.map((s) => (
            <div key={s.id} className="text-xs text-muted-foreground">
              Period {s.slotNumber}: {s.startTime} – {s.endTime}
              {s.isBreak && <Badge variant="outline" className="ml-1 text-xs">Break</Badge>}
            </div>
          ))}
        </div>
      )}
      <form onSubmit={handleAdd} className="flex items-center gap-1.5">
        <Input value={start} onChange={(e) => setStart(e.target.value)} type="time" className="w-28 h-7 text-xs" />
        <span className="text-xs text-muted-foreground">–</span>
        <Input value={end} onChange={(e) => setEnd(e.target.value)} type="time" className="w-28 h-7 text-xs" />
        <Button type="submit" size="sm" className="h-7 text-xs" disabled={createSlot.isPending}>
          <Plus className="w-3 h-3" />
        </Button>
      </form>
    </div>
  );
}

export function Step4TimeStructure() {
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  const { data: days = [], isLoading } = useTeachingDays();
  const createDay = useCreateTeachingDay();
  const updateDay = useUpdateTeachingDay();
  const [expandedDay, setExpandedDay] = useState<string | null>(null);

  async function toggleDay(dow: number) {
    const existing = days.find((d) => d.dayOfWeek === dow);
    if (existing) {
      try {
        await updateDay.mutateAsync({
          id: existing.id!,
          body: { dayOfWeek: dow, isActive: !existing.isActive, sortOrder: existing.sortOrder ?? dow },
        });
      } catch {
        toast.error("Failed to update day");
      }
    } else {
      try {
        await createDay.mutateAsync({ dayOfWeek: dow, isActive: true, sortOrder: dow });
      } catch {
        toast.error("Failed to enable day");
      }
    }
  }

  const activeDays = days.filter((d) => d.isActive);

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold flex items-center gap-2">
          <Clock className="w-5 h-5" />
          Time Structure
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          Enable teaching days and add time slots (periods) for each day.
        </p>
      </div>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">Loading…</p>
      ) : (
        <div className="space-y-3">
          {WEEKDAYS.map(({ dow, label }) => {
            const day = days.find((d) => d.dayOfWeek === dow);
            const isActive = day?.isActive ?? false;

            return (
              <div key={dow} className="space-y-1">
                <div className="flex items-center justify-between py-2 px-3 rounded-md bg-muted/50">
                  <div className="flex items-center gap-3">
                    <Switch
                      id={`day-${dow}`}
                      checked={isActive}
                      onCheckedChange={() => toggleDay(dow)}
                      disabled={createDay.isPending || updateDay.isPending}
                    />
                    <Label htmlFor={`day-${dow}`} className="text-sm font-medium cursor-pointer">
                      {label}
                    </Label>
                  </div>
                  {isActive && day && (
                    <Button
                      variant="ghost"
                      size="sm"
                      className="h-7 text-xs"
                      onClick={() => setExpandedDay(expandedDay === day.id ? null : day.id!)}
                    >
                      {expandedDay === day.id ? "Hide slots" : "Add time slots"}
                    </Button>
                  )}
                </div>
                {isActive && day && expandedDay === day.id && (
                  <TimeSlotList dayId={day.id!} />
                )}
              </div>
            );
          })}
        </div>
      )}

      <div className="pt-2 space-y-1">
        <Button onClick={() => { markStepCompleted(4); setCurrentStep(5); }} disabled={activeDays.length === 0}>
          Continue{activeDays.length > 0 && <Badge variant="secondary" className="ml-2">{activeDays.length} day{activeDays.length !== 1 ? "s" : ""}</Badge>}
        </Button>
        {activeDays.length === 0 && (
          <p className="text-xs text-muted-foreground">Enable at least one teaching day to continue</p>
        )}
      </div>
    </div>
  );
}
