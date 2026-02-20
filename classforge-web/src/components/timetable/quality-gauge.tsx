"use client";

import { cn } from "@/lib/utils";

interface QualityGaugeProps {
  score: number | null | undefined;
  size?: "sm" | "md" | "lg";
}

export function QualityGauge({ score, size = "md" }: QualityGaugeProps) {
  const value = score ?? 0;
  const pct = Math.min(100, Math.max(0, value));

  const color =
    pct >= 80 ? "text-green-500" : pct >= 50 ? "text-yellow-500" : "text-red-500";
  const ringColor =
    pct >= 80 ? "stroke-green-500" : pct >= 50 ? "stroke-yellow-500" : "stroke-red-500";

  const sizes = { sm: 48, md: 72, lg: 96 };
  const px = sizes[size];
  const r = (px - 8) / 2;
  const circ = 2 * Math.PI * r;
  const dash = (pct / 100) * circ;

  return (
    <div className="relative inline-flex items-center justify-center">
      <svg width={px} height={px} className="-rotate-90">
        <circle
          cx={px / 2}
          cy={px / 2}
          r={r}
          fill="none"
          stroke="currentColor"
          strokeWidth={6}
          className="text-muted/30"
        />
        <circle
          cx={px / 2}
          cy={px / 2}
          r={r}
          fill="none"
          strokeWidth={6}
          strokeDasharray={`${dash} ${circ}`}
          strokeLinecap="round"
          className={cn("transition-all duration-500", ringColor)}
        />
      </svg>
      <span
        className={cn(
          "absolute font-bold",
          color,
          size === "sm" ? "text-xs" : size === "md" ? "text-sm" : "text-base"
        )}
      >
        {Math.round(pct)}
      </span>
    </div>
  );
}
