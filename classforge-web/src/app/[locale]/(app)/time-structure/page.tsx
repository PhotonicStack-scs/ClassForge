"use client";

import { useState, useEffect } from "react";
import { useQueryClient, useQueries } from "@tanstack/react-query";
import {
  useTeachingDays,
  useCreateTeachingDay,
  useUpdateTeachingDay,
} from "@/lib/api/hooks/use-teaching-days";
import { apiClient } from "@/lib/api/client";
import {
  PeriodTemplateBuilder,
  type PeriodDef,
} from "@/components/time-structure/period-template-builder";
import { WeeklyCalendarGrid } from "@/components/time-structure/weekly-calendar-grid";
import { Button } from "@/components/ui/button";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { RotateCcw, TriangleAlert } from "lucide-react";
import { cn } from "@/lib/utils";
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

const WEEKEND = [
  { dow: 6, label: "Sat" },
  { dow: 7, label: "Sun" },
];

function deriveTemplate(slotsByDay: Map<string, TimeSlotResponse[]>): PeriodDef[] {
  const seen = new Map<number, PeriodDef>();
  for (const slots of slotsByDay.values()) {
    for (const s of slots) {
      if (!seen.has(s.slotNumber ?? 0)) {
        seen.set(s.slotNumber ?? 0, {
          slotNumber: s.slotNumber ?? 0,
          startTime: s.startTime ?? "",
          endTime: s.endTime ?? "",
          isBreak: s.isBreak ?? false,
        });
      }
    }
  }
  return Array.from(seen.values()).sort((a, b) => a.slotNumber - b.slotNumber);
}

export default function TimeStructurePage() {
  const qc = useQueryClient();
  const { data: days = [], isLoading } = useTeachingDays();
  const createDay = useCreateTeachingDay();
  const updateDay = useUpdateTeachingDay();

  const [periods, setPeriods] = useState<PeriodDef[]>([]);
  const [templateDirty, setTemplateDirty] = useState(false);
  const [applying, setApplying] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [toggling, setToggling] = useState<Set<string>>(new Set());

  const activeDays = days.filter((d) => d.isActive);

  // Fetch slots for all active days in parallel
  const slotQueries = useQueries({
    queries: activeDays.map((day) => ({
      queryKey: ["teaching-days", day.id!, "time-slots"],
      queryFn: async () => {
        const { data, error } = await apiClient.GET(
          "/api/v1/teaching-days/{dayId}/time-slots",
          { params: { path: { dayId: day.id! } } }
        );
        if (error) throw error;
        return (data ?? []) as TimeSlotResponse[];
      },
      enabled: !!day.id,
    })),
  });

  const slotsByDay = new Map<string, TimeSlotResponse[]>(
    activeDays.map((day, i) => [day.id!, slotQueries[i]?.data ?? []])
  );

  const allSlotsLoaded = slotQueries.every((q) => !q.isLoading);

  // Derive template from existing API data once on first load — does not mark dirty
  useEffect(() => {
    if (!allSlotsLoaded || periods.length > 0) return;
    const derived = deriveTemplate(slotsByDay);
    if (derived.length > 0) {
      setPeriods(derived);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [allSlotsLoaded]);

  // Called when the user edits the template — marks it as having unsaved changes
  function handlePeriodsChange(newPeriods: PeriodDef[]) {
    setPeriods(newPeriods);
    setTemplateDirty(true);
  }

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
        const { data: existing } = await apiClient.GET(
          "/api/v1/teaching-days/{dayId}/time-slots",
          { params: { path: { dayId: day.id! } } }
        );

        await Promise.all(
          (existing ?? []).map((slot: TimeSlotResponse) =>
            apiClient.DELETE("/api/v1/teaching-days/{dayId}/time-slots/{id}", {
              params: { path: { dayId: day.id!, id: slot.id! } },
            })
          )
        );

        const { error } = await apiClient.POST(
          "/api/v1/teaching-days/{dayId}/time-slots/bulk",
          {
            params: { path: { dayId: day.id! } },
            body: {
              items: periods.map((p) => ({
                slotNumber: p.slotNumber,
                startTime: p.startTime,
                endTime: p.endTime,
                isBreak: p.isBreak,
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
      setTemplateDirty(false);
      toast.success(`Template applied to ${activeDays.length} day(s)`);
    }
  }

  async function handleCellToggle(dayId: string, slot: PeriodDef, currentlyEnabled: boolean) {
    const key = `${dayId}:${slot.slotNumber}`;
    setToggling((prev) => new Set(prev).add(key));
    try {
      if (currentlyEnabled) {
        const existing = slotsByDay.get(dayId)?.find((s) => s.slotNumber === slot.slotNumber);
        if (existing?.id) {
          await apiClient.DELETE("/api/v1/teaching-days/{dayId}/time-slots/{id}", {
            params: { path: { dayId, id: existing.id } },
          });
        }
      } else {
        await apiClient.POST("/api/v1/teaching-days/{dayId}/time-slots", {
          params: { path: { dayId } },
          body: {
            slotNumber: slot.slotNumber,
            startTime: slot.startTime,
            endTime: slot.endTime,
            isBreak: slot.isBreak,
          },
        });
      }
      await qc.invalidateQueries({ queryKey: ["teaching-days", dayId, "time-slots"] });
    } catch {
      toast.error("Failed to update slot");
    } finally {
      setToggling((prev) => {
        const next = new Set(prev);
        next.delete(key);
        return next;
      });
    }
  }

  if (isLoading) return <div className="p-8">Loading…</div>;

  const templatePeriodCount = periods.filter((p) => !p.isBreak).length;
  const templateBreakCount = periods.filter((p) => p.isBreak).length;

  return (
    <div className="container mx-auto p-8 max-w-4xl space-y-8">
      <h1 className="text-2xl font-bold">Time Structure</h1>

      {/* Teaching Days */}
      <Card>
        <CardHeader>
          <CardTitle>Teaching Days</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap items-center gap-4">
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
                  <Label htmlFor={`day-${dow}`} className="cursor-pointer text-sm font-medium">
                    {label}
                  </Label>
                </div>
              );
            })}
            <div className="flex items-center gap-3 pl-4 border-l">
              <span className="text-xs text-muted-foreground">Weekend</span>
              {WEEKEND.map(({ dow, label }) => {
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
                    <Label htmlFor={`day-${dow}`} className="cursor-pointer text-sm font-medium">
                      {label}
                    </Label>
                  </div>
                );
              })}
            </div>
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
            Define the canonical periods and breaks for your school day. Use{" "}
            <strong>Apply to all days</strong> in the weekly schedule below to push
            these times to every active teaching day at once.
          </p>
          <PeriodTemplateBuilder periods={periods} onChange={handlePeriodsChange} />
        </CardContent>
      </Card>

      {/* Weekly Schedule */}
      {activeDays.length > 0 && periods.length > 0 && (
        <div className="space-y-3">
          {/* Section header with Apply button */}
          <div className="flex items-start justify-between gap-4">
            <div>
              <h2 className="text-lg font-semibold">Weekly Schedule</h2>
              <p className="text-sm text-muted-foreground">
                Toggle individual cells to enable or disable a slot for a specific day.
              </p>
            </div>
            <Button
              variant="outline"
              size="sm"
              className={cn(
                "shrink-0 mt-0.5",
                templateDirty && "border-amber-400 text-amber-700 hover:bg-amber-50 hover:text-amber-800"
              )}
              onClick={() => {
                if (templatePeriodCount === 0) {
                  toast.error("Add at least one period to the template first");
                  return;
                }
                setShowConfirm(true);
              }}
              disabled={applying}
            >
              <RotateCcw className="w-4 h-4 mr-2" />
              {applying ? "Applying…" : "Apply to all days"}
            </Button>
          </div>

          {/* Dirty state callout */}
          {templateDirty && (
            <div className="flex items-start gap-2 rounded-md border border-amber-200 bg-amber-50 px-3 py-2.5 text-sm text-amber-800">
              <TriangleAlert className="w-4 h-4 mt-0.5 shrink-0" />
              <span>
                The template has unsaved changes. Click{" "}
                <strong>Apply to all days</strong> to reset the entire schedule to
                the new template, or keep toggling individual cells — new cells will
                use the updated times.
              </span>
            </div>
          )}

          <WeeklyCalendarGrid
            template={periods}
            activeDays={activeDays}
            slotsByDay={slotsByDay}
            onCellToggle={handleCellToggle}
            toggling={toggling}
          />
        </div>
      )}

      {/* Prompt when no days/template yet */}
      {activeDays.length === 0 && periods.length > 0 && (
        <p className="text-sm text-muted-foreground">
          Enable at least one teaching day above to see the weekly schedule.
        </p>
      )}

      {/* Confirm Dialog */}
      <Dialog open={showConfirm} onOpenChange={setShowConfirm}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Reset all active days to template?</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            This will <strong>replace all existing time slots</strong> for{" "}
            <strong>{activeDays.length} active day(s)</strong> with{" "}
            <strong>{templatePeriodCount} period{templatePeriodCount !== 1 ? "s" : ""}</strong>
            {templateBreakCount > 0 && (
              <>
                {" "}and{" "}
                <strong>{templateBreakCount} break{templateBreakCount !== 1 ? "s" : ""}</strong>
              </>
            )}
            . Any per-day customisations will be lost.
          </p>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowConfirm(false)}>
              Cancel
            </Button>
            <Button onClick={applyTemplate}>Apply to all days</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
