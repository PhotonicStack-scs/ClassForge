"use client";

import { useState } from "react";
import { useWizardStore } from "@/lib/stores/wizard-store";
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
import { Button } from "@/components/ui/button";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { Clock } from "lucide-react";
import { toast } from "sonner";
import type { components } from "@/lib/api/schema";

type TimeSlotResponse = components["schemas"]["TimeSlotResponse"];

const WEEKDAYS = [
  { dow: 1, label: "Monday" },
  { dow: 2, label: "Tuesday" },
  { dow: 3, label: "Wednesday" },
  { dow: 4, label: "Thursday" },
  { dow: 5, label: "Friday" },
];

const WEEKEND = [
  { dow: 6, label: "Saturday" },
  { dow: 7, label: "Sunday" },
];

export function Step4TimeStructure() {
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  const { data: days = [], isLoading } = useTeachingDays();
  const createDay = useCreateTeachingDay();
  const updateDay = useUpdateTeachingDay();

  const [periods, setPeriods] = useState<PeriodDef[]>([]);
  const [applying, setApplying] = useState(false);

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

  async function handleApplyAndContinue() {
    if (activeDays.length === 0) {
      toast.error("Enable at least one teaching day first");
      return;
    }
    if (periods.filter((p) => !p.isBreak).length === 0) {
      toast.error("Add at least one period to the template first");
      return;
    }

    setApplying(true);
    let errors = 0;

    for (const day of activeDays) {
      try {
        // Fetch current slots
        const { data: existing } = await apiClient.GET(
          "/api/v1/teaching-days/{dayId}/time-slots",
          { params: { path: { dayId: day.id! } } }
        );

        // Delete existing slots in parallel
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
      } catch {
        errors++;
      }
    }

    setApplying(false);

    if (errors > 0) {
      toast.error(`Failed to apply template to ${errors} day(s)`);
      return;
    }

    markStepCompleted(4);
    setCurrentStep(5);
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-xl font-semibold flex items-center gap-2">
          <Clock className="w-5 h-5" />
          Time Structure
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          Enable teaching days and define a period schedule to apply to all of them.
        </p>
      </div>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">Loading…</p>
      ) : (
        <>
          {/* Day toggles */}
          <div className="space-y-1">
            <p className="text-sm font-medium">Teaching Days</p>
            <div className="flex flex-wrap items-center gap-4 py-2">
              {WEEKDAYS.map(({ dow, label }) => {
                const day = days.find((d) => d.dayOfWeek === dow);
                const isActive = day?.isActive ?? false;
                return (
                  <div key={dow} className="flex items-center gap-2">
                    <Switch
                      id={`wizard-day-${dow}`}
                      checked={isActive}
                      onCheckedChange={() => toggleDay(dow)}
                      disabled={createDay.isPending || updateDay.isPending}
                    />
                    <Label
                      htmlFor={`wizard-day-${dow}`}
                      className="text-sm cursor-pointer"
                    >
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
                        id={`wizard-day-${dow}`}
                        checked={isActive}
                        onCheckedChange={() => toggleDay(dow)}
                        disabled={createDay.isPending || updateDay.isPending}
                      />
                      <Label
                        htmlFor={`wizard-day-${dow}`}
                        className="text-sm cursor-pointer"
                      >
                        {label}
                      </Label>
                    </div>
                  );
                })}
              </div>
            </div>
          </div>

          {/* Period template */}
          <div className="space-y-2">
            <p className="text-sm font-medium">Period Schedule</p>
            <PeriodTemplateBuilder periods={periods} onChange={setPeriods} />
          </div>
        </>
      )}

      <div className="pt-2 space-y-1">
        <Button
          onClick={handleApplyAndContinue}
          disabled={applying || activeDays.length === 0 || periods.filter((p) => !p.isBreak).length === 0}
        >
          {applying
            ? "Applying…"
            : "Apply to all active days & continue →"}
        </Button>
        {activeDays.length === 0 && (
          <p className="text-xs text-muted-foreground">
            Enable at least one teaching day to continue.
          </p>
        )}
        {activeDays.length > 0 && periods.filter((p) => !p.isBreak).length === 0 && (
          <p className="text-xs text-muted-foreground">
            Add at least one period to the template to continue.
          </p>
        )}
      </div>
    </div>
  );
}
