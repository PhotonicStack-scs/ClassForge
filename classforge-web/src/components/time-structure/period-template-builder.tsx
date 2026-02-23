"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { X, Plus } from "lucide-react";

export type PeriodDef = {
  slotNumber: number;
  startTime: string;
  endTime: string;
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
  function addPeriod() {
    const last = periods[periods.length - 1];
    const startTime = last ? addMinutes(last.endTime, 5) : "08:00";
    const endTime = addMinutes(startTime, 45);
    onChange([
      ...periods,
      { slotNumber: periods.length + 1, startTime, endTime },
    ]);
  }

  function removePeriod(idx: number) {
    const next = periods
      .filter((_, i) => i !== idx)
      .map((p, i) => ({ ...p, slotNumber: i + 1 }));
    onChange(next);
  }

  function updatePeriod(idx: number, field: "startTime" | "endTime", value: string) {
    onChange(periods.map((p, i) => (i === idx ? { ...p, [field]: value } : p)));
  }

  return (
    <div className="space-y-2">
      {periods.length > 0 && (
        <div className="grid grid-cols-[40px_1fr_1fr_36px] gap-2 text-xs font-medium text-muted-foreground px-1">
          <span>#</span>
          <span>Start</span>
          <span>End</span>
          <span />
        </div>
      )}
      {periods.map((p, idx) => (
        <div key={idx} className="grid grid-cols-[40px_1fr_1fr_36px] gap-2 items-center">
          <span className="text-sm text-center font-medium">{p.slotNumber}</span>
          <Input
            type="time"
            value={p.startTime}
            onChange={(e) => updatePeriod(idx, "startTime", e.target.value)}
            className="h-8 text-sm"
          />
          <Input
            type="time"
            value={p.endTime}
            onChange={(e) => updatePeriod(idx, "endTime", e.target.value)}
            className="h-8 text-sm"
          />
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="h-8 w-8"
            onClick={() => removePeriod(idx)}
          >
            <X className="w-4 h-4" />
          </Button>
        </div>
      ))}
      <Button
        type="button"
        variant="outline"
        size="sm"
        onClick={addPeriod}
        className="w-full"
      >
        <Plus className="w-4 h-4 mr-1" />
        Add period
      </Button>
    </div>
  );
}
