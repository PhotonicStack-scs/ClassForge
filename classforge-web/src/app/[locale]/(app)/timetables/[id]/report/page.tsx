"use client";

import { use } from "react";
import Link from "next/link";
import { useTranslations } from "next-intl";
import { useTimetableReport } from "@/lib/api/hooks/use-timetable-views";
import { Button } from "@/components/ui/button";
import { ArrowLeft } from "lucide-react";
import { ReportSummary } from "@/components/report/report-summary";
import { ReportItemList } from "@/components/report/report-item-list";

export default function TimetableReportPage({
  params,
}: {
  params: Promise<{ id: string; locale: string }>;
}) {
  const { id, locale } = use(params);
  const t = useTranslations("timetable");
  const { data: items = [], isLoading } = useTimetableReport(id);

  const errors = items.filter((i) => i.type === "Error");
  const warnings = items.filter((i) => i.type === "Warning");
  const infos = items.filter((i) => i.type === "Info");

  if (isLoading) {
    return <div className="p-8 text-muted-foreground">Laster rapport...</div>;
  }

  return (
    <div className="space-y-6 max-w-3xl">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" asChild>
          <Link href={`/${locale}/timetables/${id}`}>
            <ArrowLeft className="w-4 h-4" />
          </Link>
        </Button>
        <h1 className="text-2xl font-bold">{t("reportTitle")}</h1>
      </div>

      <ReportSummary
        errorCount={errors.length}
        warningCount={warnings.length}
        infoCount={infos.length}
      />

      {errors.length > 0 && <ReportItemList severity="Error" items={errors} />}
      {warnings.length > 0 && <ReportItemList severity="Warning" items={warnings} />}
      {infos.length > 0 && <ReportItemList severity="Info" items={infos} />}

      {items.length === 0 && (
        <div className="text-center py-16 text-muted-foreground">
          Ingen problemer funnet
        </div>
      )}
    </div>
  );
}
