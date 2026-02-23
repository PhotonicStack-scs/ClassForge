"use client";

import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import {
  useTeachingDays,
  useCreateTeachingDay,
  useUpdateTeachingDay,
  useTimeSlots,
  useCreateTimeSlot,
  useDeleteTimeSlot,
} from "@/lib/api/hooks/use-teaching-days";
import { apiClient } from "@/lib/api/client";
import {
  PeriodTemplateBuilder,
  type PeriodDef,
} from "@/components/time-structure/period-template-builder";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { ChevronDown, ChevronRight, X, Plus } from "lucide-react";
import { toast } from "sonner";
import type { components } from "@/lib/api/schema";

type TeachingDayResponse = components["schemas"]["TeachingDayResponse"];
type TimeSlotResponse = components["schemas"]["TimeSlotResponse"];

const WEEKDAYS = [
  { dow: 1, label: "Mon" },
  { dow: 2, label: "Tue" },
  { dow: 3, label: "Wed" },
  { dow: 4, label: "Thu" },
  { dow: 5, label: "Fri" },
];

function DaySlotEditor({ dayId }: { dayId: string }) {
  const { data: slots = [], isLoading } = useTimeSlots(dayId);
  const createSlot = useCreateTimeSlot(dayId);
  const deleteSlot = useDeleteTimeSlot(dayId);
  const [newStart, setNewStart] = useState("08:00");
  const [newEnd, setNewEnd] = useState("08:45");

  if (isLoading) {
    return <p className="text-sm text-muted-foreground py-2">Loading slots…</p>;
  }

  async function handleAdd() {
    try {
      await createSlot.mutateAsync({
        slotNumber: slots.length + 1,
        startTime: newStart,
        endTime: newEnd,
        isBreak: false,
      });
    } catch {
      toast.error("Failed to add slot");
    }
  }

  async function handleDelete(id: string) {
    try {
      await deleteSlot.mutateAsync(id);
    } catch {
      toast.error("Failed to delete slot");
    }
  }

  return (
    <div className="space-y-2">
      {slots.map((slot) => (
        <div key={slot.id!} className="flex items-center gap-3 text-sm">
          <span className="w-20 text-muted-foreground shrink-0">
            Period {slot.slotNumber}
          </span>
          <span>
            {slot.startTime} – {slot.endTime}
          </span>
          <Button
            variant="ghost"
            size="icon"
            className="h-6 w-6 ml-auto"
            onClick={() => handleDelete(slot.id!)}
            disabled={deleteSlot.isPending}
          >
            <X className="w-3 h-3" />
          </Button>
        </div>
      ))}
      <div className="flex gap-2 pt-2 border-t">
        <Input
          type="time"
          value={newStart}
          onChange={(e) => setNewStart(e.target.value)}
          className="h-8 text-sm w-32"
        />
        <span className="self-center text-muted-foreground">–</span>
        <Input
          type="time"
          value={newEnd}
          onChange={(e) => setNewEnd(e.target.value)}
          className="h-8 text-sm w-32"
        />
        <Button
          size="sm"
          className="h-8"
          onClick={handleAdd}
          disabled={createSlot.isPending}
        >
          <Plus className="w-4 h-4 mr-1" />
          Add slot
        </Button>
      </div>
    </div>
  );
}

function PerDaySection({ day }: { day: TeachingDayResponse }) {
  const [expanded, setExpanded] = useState(false);
  const { data: slots = [] } = useTimeSlots(day.id!);

  return (
    <div className="border rounded-lg overflow-hidden">
      <button
        className="w-full flex items-center justify-between px-4 py-3 text-left hover:bg-muted/50 transition-colors"
        onClick={() => setExpanded((v) => !v)}
      >
        <div className="flex items-center gap-2">
          {expanded ? (
            <ChevronDown className="w-4 h-4 text-muted-foreground" />
          ) : (
            <ChevronRight className="w-4 h-4 text-muted-foreground" />
          )}
          <span className="font-medium">{day.name ?? ""}</span>
          <Badge variant="secondary" className="text-xs">
            {slots.length} period{slots.length !== 1 ? "s" : ""}
          </Badge>
        </div>
      </button>
      {expanded && (
        <div className="px-4 pb-4 border-t">
          <DaySlotEditor dayId={day.id!} />
        </div>
      )}
    </div>
  );
}

export default function TimeStructurePage() {
  const qc = useQueryClient();
  const { data: days = [], isLoading } = useTeachingDays();
  const createDay = useCreateTeachingDay();
  const updateDay = useUpdateTeachingDay();

  const [periods, setPeriods] = useState<PeriodDef[]>([]);
  const [applying, setApplying] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);

  const activeDays = days.filter((d) => d.isActive);

  async function toggleDay(dow: number) {
    const existing = days.find((d) => d.dayOfWeek === dow);
    if (existing) {
      try {
        await updateDay.mutateAsync({
          id: existing.id!,
          body: { isActive: !existing.isActive, sortOrder: existing.sortOrder ?? dow },
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

  async function applyTemplate() {
    setShowConfirm(false);
    setApplying(true);
    let errors = 0;

    for (const day of activeDays) {
      try {
        // Fetch current slots fresh
        const { data: existing } = await apiClient.GET(
          "/api/v1/teaching-days/{dayId}/time-slots",
          { params: { path: { dayId: day.id! } } }
        );

        // Delete all existing slots in parallel
        await Promise.all(
          (existing ?? []).map((slot: TimeSlotResponse) =>
            apiClient.DELETE("/api/v1/teaching-days/{dayId}/time-slots/{id}", {
              params: { path: { dayId: day.id!, id: slot.id! } },
            })
          )
        );

        // Bulk create from template
        const { error } = await apiClient.POST(
          "/api/v1/teaching-days/{dayId}/time-slots/bulk",
          {
            params: { path: { dayId: day.id! } },
            body: {
              items: periods.map((p) => ({
                slotNumber: p.slotNumber,
                startTime: p.startTime,
                endTime: p.endTime,
                isBreak: false,
              })),
            },
          }
        );
        if (error) errors++;

        await qc.invalidateQueries({
          queryKey: ["teaching-days", day.id!, "time-slots"],
        });
      } catch {
        errors++;
      }
    }

    setApplying(false);
    if (errors > 0) {
      toast.error(`Failed to apply template to ${errors} day(s)`);
    } else {
      toast.success(`Template applied to ${activeDays.length} day(s)`);
    }
  }

  if (isLoading) return <div className="p-8">Loading…</div>;

  return (
    <div className="container mx-auto p-8 max-w-3xl space-y-8">
      <h1 className="text-2xl font-bold">Time Structure</h1>

      {/* Teaching Days */}
      <Card>
        <CardHeader>
          <CardTitle>Teaching Days</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-4">
            {WEEKDAYS.map(({ dow, label }) => {
              const day = days.find((d) => d.dayOfWeek === dow);
              const isActive = day?.isActive ?? false;
              return (
                <div key={dow} className="flex items-center gap-2">
                  <Switch
                    id={`day-${dow}`}
                    checked={isActive}
                    onCheckedChange={() => toggleDay(dow)}
                    disabled={createDay.isPending || updateDay.isPending}
                  />
                  <Label
                    htmlFor={`day-${dow}`}
                    className="cursor-pointer text-sm font-medium"
                  >
                    {label}
                  </Label>
                </div>
              );
            })}
          </div>
        </CardContent>
      </Card>

      {/* Period Template */}
      <Card>
        <CardHeader>
          <CardTitle>Period Template</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-muted-foreground">
            Define a period schedule and apply it to all active teaching days at once.
          </p>
          <PeriodTemplateBuilder periods={periods} onChange={setPeriods} />
          <Button
            onClick={() => {
              if (periods.length === 0) {
                toast.error("Add at least one period to the template first");
                return;
              }
              setShowConfirm(true);
            }}
            disabled={applying || activeDays.length === 0 || periods.length === 0}
          >
            {applying ? "Applying…" : "Apply to all active days"}
          </Button>
          {activeDays.length === 0 && (
            <p className="text-xs text-muted-foreground">
              Enable at least one teaching day above first.
            </p>
          )}
        </CardContent>
      </Card>

      {/* Per-Day Schedule */}
      {activeDays.length > 0 && (
        <div className="space-y-3">
          <h2 className="text-lg font-semibold">Per-Day Schedule</h2>
          <p className="text-sm text-muted-foreground">
            Expand a day to view or edit its slots individually.
          </p>
          {activeDays.map((day) => (
            <PerDaySection key={day.id!} day={day} />
          ))}
        </div>
      )}

      {/* Confirm Dialog */}
      <Dialog open={showConfirm} onOpenChange={setShowConfirm}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Apply template to all active days?</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            This will replace all existing time slots for{" "}
            <strong>{activeDays.length} active day(s)</strong> with the{" "}
            <strong>{periods.length} period(s)</strong> defined in your template.
            Existing slots will be permanently deleted.
          </p>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowConfirm(false)}>
              Cancel
            </Button>
            <Button onClick={applyTemplate}>Apply</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
