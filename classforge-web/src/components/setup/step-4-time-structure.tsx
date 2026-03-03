"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { useWizardStore } from "@/lib/stores/wizard-store";
import {
  useSchoolDays,
  useCreateSchoolDay,
  useUpdateSchoolDay,
} from "@/lib/api/hooks/use-school-days";
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

export function Step4TimeStructure() {
  const t = useTranslations("setup");
  const ts = useTranslations("timeStructure");
  const tc = useTranslations("common");
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  const { data: days = [], isLoading } = useSchoolDays();
  const createDay = useCreateSchoolDay();
  const updateDay = useUpdateSchoolDay();

  const [periods, setPeriods] = useState<PeriodDef[]>([]);
  const [applying, setApplying] = useState(false);

  const activeDays = days.filter((d) => d.isActive);

  const WEEKDAYS = [
    { dow: 1, label: ts("monday") },
    { dow: 2, label: ts("tuesday") },
    { dow: 3, label: ts("wednesday") },
    { dow: 4, label: ts("thursday") },
    { dow: 5, label: ts("friday") },
  ];

  const WEEKEND = [
    { dow: 6, label: ts("saturdayShort") },
    { dow: 0, label: ts("sundayShort") },
  ];

  async function toggleDay(dow: number) {
    const existing = days.find((d) => d.dayOfWeek === dow);
    if (existing) {
      try {
        await updateDay.mutateAsync({
          id: existing.id!,
          body: { isActive: !existing.isActive, sortOrder: existing.sortOrder ?? dow },
        });
      } catch {
        toast.error(tc("error"));
      }
    } else {
      try {
        await createDay.mutateAsync({ dayOfWeek: dow, isActive: true, sortOrder: dow === 0 ? 7 : dow });
      } catch {
        toast.error(tc("error"));
      }
    }
  }

  async function handleApplyAndContinue() {
    if (activeDays.length === 0) {
      toast.error(t("enableSchoolDayFirst"));
      return;
    }
    if (periods.filter((p) => !p.isBreak).length === 0) {
      toast.error(t("addPeriodFirst"));
      return;
    }

    setApplying(true);
    let errors = 0;

    for (const day of activeDays) {
      try {
        const { data: existing } = await apiClient.GET(
          "/api/v1/school-days/{dayId}/time-slots",
          { params: { path: { dayId: day.id! } } }
        );

        await Promise.all(
          (existing ?? []).map((slot: TimeSlotResponse) =>
            apiClient.DELETE("/api/v1/school-days/{dayId}/time-slots/{id}", {
              params: { path: { dayId: day.id!, id: slot.id! } },
            })
          )
        );

        const { error } = await apiClient.POST(
          "/api/v1/school-days/{dayId}/time-slots/bulk",
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
      toast.error(tc("error"));
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
          {ts("title")}
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          {t("step4Description")}
        </p>
      </div>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">{tc("loading")}</p>
      ) : (
        <>
          {/* Day toggles */}
          <div className="space-y-1">
            <p className="text-sm font-medium">{ts("schoolDays")}</p>
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
                <span className="text-xs text-muted-foreground">{ts("weekend")}</span>
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
            <p className="text-sm font-medium">{ts("periodTemplate")}</p>
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
            ? ts("applying")
            : ts("applyToAllAndContinue")}
        </Button>
        {activeDays.length === 0 && (
          <p className="text-xs text-muted-foreground">
            {t("enableSchoolDayFirst")}
          </p>
        )}
        {activeDays.length > 0 && periods.filter((p) => !p.isBreak).length === 0 && (
          <p className="text-xs text-muted-foreground">
            {t("addPeriodFirst")}
          </p>
        )}
      </div>
    </div>
  );
}
