"use client";

import { useTranslations } from "next-intl";
import { AlertCircle, AlertTriangle, Info } from "lucide-react";
import type { components } from "@/lib/api/schema";

type TimetableReportResponse = components["schemas"]["TimetableReportResponse"];

interface ReportItemListProps {
  severity: "Error" | "Warning" | "Info";
  items: TimetableReportResponse[];
}

const ICONS = {
  Error: AlertCircle,
  Warning: AlertTriangle,
  Info: Info,
};

const COLORS = {
  Error: "text-destructive",
  Warning: "text-yellow-500",
  Info: "text-blue-500",
};

const BG_COLORS = {
  Error: "bg-destructive/5 border-destructive/20",
  Warning: "bg-yellow-50 border-yellow-200 dark:bg-yellow-950/20 dark:border-yellow-800",
  Info: "bg-blue-50 border-blue-200 dark:bg-blue-950/20 dark:border-blue-800",
};

export function ReportItemList({ severity, items }: ReportItemListProps) {
  const t = useTranslations("timetable");
  const Icon = ICONS[severity];
  const color = COLORS[severity];
  const bg = BG_COLORS[severity];

  return (
    <div className="space-y-2">
      <h2 className={"text-lg font-semibold flex items-center gap-2 " + color}>
        <Icon className="w-5 h-5" />
        {t("severity." + severity as never)} ({items.length})
      </h2>
      <ul className="space-y-2">
        {items.map((item) => (
          <li
            key={item.id}
            className={"rounded-md border p-3 text-sm " + bg}
          >
            <div className="font-medium">
              {item.category ? t("issueCategories." + item.category as never) : item.category}
            </div>
            <div className="text-muted-foreground mt-0.5">{item.message}</div>
          </li>
        ))}
      </ul>
    </div>
  );
}
