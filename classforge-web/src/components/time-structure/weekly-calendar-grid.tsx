"use client";

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Checkbox } from "@/components/ui/checkbox";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { PeriodDef } from "./period-template-builder";
import type { components } from "@/lib/api/schema";

// Format "HH:mm" using the browser's locale (respects 12h/24h system preference).
// Falls back to the raw string if the value is missing or unparseable.
function formatTime(hhmm: string): string {
  const [h, m] = hhmm.split(":").map(Number);
  if (!hhmm || isNaN(h) || isNaN(m)) return hhmm;
  return new Intl.DateTimeFormat(undefined, {
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(1970, 0, 1, h, m));
}

type TeachingDayResponse = components["schemas"]["TeachingDayResponse"];
type TimeSlotResponse = components["schemas"]["TimeSlotResponse"];

type Props = {
  template: PeriodDef[];
  activeDays: TeachingDayResponse[];
  slotsByDay: Map<string, TimeSlotResponse[]>;
  onCellToggle: (dayId: string, slot: PeriodDef, currentlyEnabled: boolean) => Promise<void>;
  toggling: Set<string>;
};

export function WeeklyCalendarGrid({
  template,
  activeDays,
  slotsByDay,
  onCellToggle,
  toggling,
}: Props) {
  function lessonNumber(upToIdx: number) {
    return template.slice(0, upToIdx + 1).filter((e) => !e.isBreak).length;
  }

  if (template.length === 0 || activeDays.length === 0) return null;

  return (
    <div className="rounded-lg border overflow-x-auto">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-52">Slot</TableHead>
            {activeDays.map((day) => (
              <TableHead key={day.id} className="text-center w-20">
                {day.name ?? ""}
              </TableHead>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
          {template.map((slot, idx) => (
            <TableRow
              key={slot.slotNumber}
              className={cn(slot.isBreak && "bg-muted/50")}
            >
              <TableCell className="font-medium text-sm">
                {slot.isBreak ? (
                  <span className="flex items-center gap-2">
                    <Badge
                      variant="outline"
                      className="text-xs px-1.5 py-0 font-normal text-muted-foreground"
                    >
                      Break
                    </Badge>
                    <span className="text-muted-foreground text-xs">
                      {formatTime(slot.startTime)}–{formatTime(slot.endTime)}
                    </span>
                  </span>
                ) : (
                  <span className="flex items-center gap-2">
                    <span>Period {lessonNumber(idx)}</span>
                    <span className="text-muted-foreground text-xs">
                      {formatTime(slot.startTime)}–{formatTime(slot.endTime)}
                    </span>
                  </span>
                )}
              </TableCell>
              {activeDays.map((day) => {
                const daySlots = slotsByDay.get(day.id!) ?? [];
                const enabled = daySlots.some((s) => s.slotNumber === slot.slotNumber);
                const cellKey = `${day.id!}:${slot.slotNumber}`;
                const isToggling = toggling.has(cellKey);
                return (
                  <TableCell key={day.id} className="text-center">
                    {!slot.isBreak && (
                      <Checkbox
                        checked={enabled}
                        disabled={isToggling}
                        onCheckedChange={() => onCellToggle(day.id!, slot, enabled)}
                        aria-label={`Period ${lessonNumber(idx)} on ${day.name ?? ""}`}
                      />
                    )}
                  </TableCell>
                );
              })}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
