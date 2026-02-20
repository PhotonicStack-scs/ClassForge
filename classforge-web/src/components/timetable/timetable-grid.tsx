"use client";

import type { components } from "@/lib/api/schema";
import { TimetableCell } from "./timetable-cell";

type TimetableViewEntry = components["schemas"]["TimetableViewEntry"];

interface TimetableGridProps {
  entries: TimetableViewEntry[];
  days: string[];
  slots: { slotNumber: number; label: string; isBreak?: boolean }[];
  subjectColors?: Record<string, string>;
  editable?: boolean;
  onCellClick?: (entry: TimetableViewEntry | null, day: number, slot: number) => void;
}

export function TimetableGrid({
  entries,
  days,
  slots,
  subjectColors = {},
  editable = false,
  onCellClick,
}: TimetableGridProps) {
  // Build lookup: day+slot → entry
  const entryMap = new Map<string, TimetableViewEntry>();
  for (const e of entries) {
    entryMap.set(`${e.dayOfWeek}-${e.slotNumber}`, e);
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr>
            <th className="w-20 p-2 text-left text-muted-foreground font-medium border-b border-border">
              Tid
            </th>
            {days.map((day, i) => (
              <th
                key={i}
                className="p-2 text-center text-foreground font-semibold border-b border-border min-w-[120px]"
              >
                {day}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {slots.map((slot) => {
            if (slot.isBreak) {
              return (
                <tr key={`break-${slot.slotNumber}`} className="bg-muted/30">
                  <td
                    colSpan={days.length + 1}
                    className="py-1 px-2 text-xs text-muted-foreground italic border-y border-border"
                  >
                    {slot.label}
                  </td>
                </tr>
              );
            }

            return (
              <tr key={slot.slotNumber} className="hover:bg-muted/10">
                <td className="p-1 text-xs text-muted-foreground font-medium border-b border-border w-20">
                  {slot.label}
                </td>
                {days.map((_, dayIdx) => {
                  const dayOfWeek = dayIdx + 1;
                  const entry = entryMap.get(`${dayOfWeek}-${slot.slotNumber}`) ?? null;
                  const color = entry?.subjectName
                    ? subjectColors[entry.subjectName]
                    : undefined;

                  return (
                    <td
                      key={dayIdx}
                      className="p-1 border-b border-l border-border align-top"
                    >
                      <TimetableCell
                        entry={entry}
                        subjectColor={color}
                        editable={editable}
                        onClick={() => onCellClick?.(entry, dayOfWeek, slot.slotNumber)}
                      />
                    </td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
