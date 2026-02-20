"use client";

import type { components } from "@/lib/api/schema";
import { getContrastColor } from "@/lib/utils/color";

type TimetableViewEntry = components["schemas"]["TimetableViewEntry"];

interface TimetableCellProps {
  entry: TimetableViewEntry | null;
  subjectColor?: string;
  onClick?: () => void;
  editable?: boolean;
}

export function TimetableCell({
  entry,
  subjectColor,
  onClick,
  editable = false,
}: TimetableCellProps) {
  if (!entry) {
    return (
      <div
        onClick={editable ? onClick : undefined}
        className="h-full min-h-[60px] rounded border border-dashed border-border bg-transparent flex items-center justify-center text-xs text-muted-foreground/50 cursor-pointer hover:bg-muted/20 transition-colors"
        role={editable ? "button" : undefined}
      >
        {editable ? "+" : ""}
      </div>
    );
  }

  const bg = subjectColor ?? "#e5e7eb";
  const fg = getContrastColor(bg);

  return (
    <div
      onClick={editable ? onClick : undefined}
      style={{ backgroundColor: bg, color: fg }}
      className="h-full min-h-[60px] rounded p-1.5 text-xs font-medium overflow-hidden cursor-pointer hover:brightness-95 transition-all"
      role={editable ? "button" : undefined}
    >
      <div className="font-semibold truncate">{entry.subjectName}</div>
      {entry.teacherName && (
        <div className="opacity-80 truncate">{entry.teacherName}</div>
      )}
      {entry.roomName && (
        <div className="opacity-70 truncate">{entry.roomName}</div>
      )}
      {entry.isDoublePeriod && (
        <div className="mt-0.5 text-[10px] opacity-60">2×</div>
      )}
    </div>
  );
}
