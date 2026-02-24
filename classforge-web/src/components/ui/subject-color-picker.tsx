"use client";

import { useState } from "react";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { SUBJECT_COLORS, getContrastColor } from "@/lib/utils/color";
import { cn } from "@/lib/utils";
import { Check } from "lucide-react";

interface SubjectColorPickerProps {
  value: string;
  onChange: (color: string) => void;
}

export function SubjectColorPicker({ value, onChange }: SubjectColorPickerProps) {
  const [open, setOpen] = useState(false);

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <button
          type="button"
          className="h-9 w-9 rounded-md border border-input shrink-0 transition-opacity hover:opacity-80"
          style={{ backgroundColor: value }}
          aria-label="Pick a color"
        />
      </PopoverTrigger>
      <PopoverContent className="w-auto p-2" align="start">
        <div className="grid grid-cols-5 gap-1.5">
          {SUBJECT_COLORS.map((c) => (
            <button
              key={c}
              type="button"
              onClick={() => { onChange(c); setOpen(false); }}
              className={cn(
                "w-7 h-7 rounded-full border-2 flex items-center justify-center transition-transform hover:scale-110",
                value === c ? "border-foreground/60" : "border-transparent"
              )}
              style={{ backgroundColor: c }}
              aria-label={c}
            >
              {value === c && (
                <Check
                  className="w-3.5 h-3.5"
                  style={{ color: getContrastColor(c) === "black" ? "rgba(0,0,0,0.5)" : "rgba(255,255,255,0.8)" }}
                />
              )}
            </button>
          ))}
        </div>
      </PopoverContent>
    </Popover>
  );
}
