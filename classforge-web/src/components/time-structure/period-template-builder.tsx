"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { X, Plus, Coffee } from "lucide-react";
import { cn } from "@/lib/utils";

export type PeriodDef = {
  slotNumber: number;
  startTime: string;
  endTime: string;
  isBreak: boolean;
};

function addMinutes(time: string, minutes: number): string {
  const [h, m] = time.split(":").map(Number);
  const total = h * 60 + m + minutes;
  const hh = Math.floor(total / 60) % 24;
  const mm = total % 60;
  return `${String(hh).padStart(2, "0")}:${String(mm).padStart(2, "0")}`;
}

type Props = {
  periods: PeriodDef[];
  onChange: (periods: PeriodDef[]) => void;
};

export function PeriodTemplateBuilder({ periods, onChange }: Props) {
  function addEntry(isBreak: boolean) {
    const last = periods[periods.length - 1];
    const startTime = last ? last.endTime : "08:00";
    const endTime = addMinutes(startTime, isBreak ? 15 : 45);
    onChange([
      ...periods,
      { slotNumber: Math.max(0, ...periods.map((p) => p.slotNumber)) + 1, startTime, endTime, isBreak },
    ]);
  }

  function removeEntry(idx: number) {
    const next = periods
      .filter((_, i) => i !== idx)
      .map((p, i) => ({ ...p, slotNumber: i + 1 }));
    onChange(next);
  }

  function updateEntry(idx: number, field: "startTime" | "endTime", value: string) {
    onChange(periods.map((p, i) => (i === idx ? { ...p, [field]: value } : p)));
  }

  // Lesson number = count of non-break entries up to and including a given index
  function lessonNumber(upToIdx: number) {
    return periods.slice(0, upToIdx + 1).filter((e) => !e.isBreak).length;
  }

  return (
    <div className="space-y-2">
      {periods.length > 0 && (
        <div className="grid grid-cols-[48px_1fr_1fr_36px] gap-2 text-xs font-medium text-muted-foreground px-1">
          <span>#</span>
          <span>Start</span>
          <span>End</span>
          <span />
        </div>
      )}

      {periods.map((p, idx) => (
        <div
          key={idx}
          className={cn(
            "grid grid-cols-[48px_1fr_1fr_36px] gap-2 items-center rounded-md px-1 py-0.5",
            p.isBreak && "bg-muted/70"
          )}
        >
          <div className="flex items-center justify-center">
            {p.isBreak ? (
              <Badge
                variant="outline"
                className="text-xs px-1.5 py-0 font-normal text-muted-foreground"
              >
                Brk
              </Badge>
            ) : (
              <span className="text-sm font-medium text-center">
                {lessonNumber(idx)}
              </span>
            )}
          </div>
          <Input
            type="time"
            value={p.startTime}
            onChange={(e) => updateEntry(idx, "startTime", e.target.value)}
            className="h-8 text-sm"
          />
          <Input
            type="time"
            value={p.endTime}
            onChange={(e) => updateEntry(idx, "endTime", e.target.value)}
            className="h-8 text-sm"
          />
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="h-8 w-8"
            onClick={() => removeEntry(idx)}
          >
            <X className="w-4 h-4" />
          </Button>
        </div>
      ))}

      <div className="flex gap-2 mt-1">
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => addEntry(false)}
          className="flex-1"
        >
          <Plus className="w-4 h-4 mr-1" />
          Add period
        </Button>
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => addEntry(true)}
          className="flex-1"
        >
          <Coffee className="w-4 h-4 mr-1" />
          Add break
        </Button>
      </div>
    </div>
  );
}
