"use client";

import { useTranslations } from "next-intl";
import { AlertCircle, AlertTriangle, Info } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";

interface ReportSummaryProps {
  errorCount: number;
  warningCount: number;
  infoCount: number;
}

export function ReportSummary({ errorCount, warningCount, infoCount }: ReportSummaryProps) {
  const t = useTranslations("timetable");
  const total = errorCount + warningCount + infoCount;

  return (
    <Card>
      <CardContent className="pt-6">
        <div className="grid grid-cols-3 gap-4 text-center">
          <div className="flex flex-col items-center gap-1">
            <AlertCircle className="w-6 h-6 text-destructive" />
            <span className="text-2xl font-bold text-destructive">{errorCount}</span>
            <span className="text-sm text-muted-foreground">{t("severity.Error")}</span>
          </div>
          <div className="flex flex-col items-center gap-1">
            <AlertTriangle className="w-6 h-6 text-yellow-500" />
            <span className="text-2xl font-bold text-yellow-500">{warningCount}</span>
            <span className="text-sm text-muted-foreground">{t("severity.Warning")}</span>
          </div>
          <div className="flex flex-col items-center gap-1">
            <Info className="w-6 h-6 text-blue-500" />
            <span className="text-2xl font-bold text-blue-500">{infoCount}</span>
            <span className="text-sm text-muted-foreground">{t("severity.Info")}</span>
          </div>
        </div>
        {total === 0 && (
          <p className="text-center text-sm text-muted-foreground mt-4">
            No issues found
          </p>
        )}
      </CardContent>
    </Card>
  );
}
