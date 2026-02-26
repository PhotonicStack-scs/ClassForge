"use client";

import { use, useState } from "react";
import Link from "next/link";
import { useTranslations } from "next-intl";
import { useTimetable, usePublishTimetable } from "@/lib/api/hooks/use-timetables";
import { useTeachers } from "@/lib/api/hooks/use-teachers";
import { useTimetableByClass, useTimetableByTeacher } from "@/lib/api/hooks/use-timetable-views";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { QualityGauge } from "@/components/timetable/quality-gauge";
import { TimetableGrid } from "@/components/timetable/timetable-grid";
import { Progress } from "@/components/ui/progress";
import { toast } from "sonner";
import { ArrowLeft, FileText } from "lucide-react";

type ViewMode = "class" | "teacher";
const DAY_NAMES = ["Mandag", "Tirsdag", "Onsdag", "Torsdag", "Fredag"];
const DEFAULT_SLOTS = [1, 2, 3, 4, 5, 6, 7, 8].map((n) => ({ slotNumber: n, label: `Time ${n}` }));
const STATUS_VARIANT: Record<string, "default" | "secondary" | "destructive" | "outline"> = {
  Draft: "secondary", Generating: "outline", Generated: "default", Published: "default", Failed: "destructive",
};

export default function TimetableDetailPage({ params }: { params: Promise<{ id: string; locale: string }> }) {
  const { id, locale } = use(params);
  const t = useTranslations("timetable");
  const [viewMode, setViewMode] = useState<ViewMode>("class");
  const [selectedClassId, setSelectedClassId] = useState<string>("");
  const [selectedTeacherId, setSelectedTeacherId] = useState<string>("");
  const { data: timetable, isLoading } = useTimetable(id);
  const { data: teachersData } = useTeachers();
  const publishMutation = usePublishTimetable();

  const teachers = teachersData?.map((teacher) => ({
    id: teacher.id ?? "",
    label: teacher.name ?? "",
  })) ?? [];

  const { data: classView } = useTimetableByClass(id, viewMode === "class" && selectedClassId ? selectedClassId : null);
  const { data: teacherView } = useTimetableByTeacher(id, viewMode === "teacher" && selectedTeacherId ? selectedTeacherId : null);
  const entries = viewMode === "class" ? classView?.entries ?? [] : teacherView?.entries ?? [];
  const hasSelection = viewMode === "class" ? !!selectedClassId : !!selectedTeacherId;

  if (isLoading) return <div className="p-8 text-muted-foreground">Laster...</div>;
  if (!timetable) return <div className="p-8">Timeplan ikke funnet.</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between flex-wrap gap-4">
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="icon" asChild>
            <Link href={`/${locale}/timetables`}><ArrowLeft className="w-4 h-4" /></Link>
          </Button>
          <div>
            <h1 className="text-2xl font-bold">{timetable.name}</h1>
            <div className="flex items-center gap-2 mt-1">
              <Badge variant={STATUS_VARIANT[timetable.status ?? ""] ?? "secondary"}>
                {t(`status.${timetable.status}` as never)}
              </Badge>
              {timetable.qualityScore != null && (
                <div className="flex items-center gap-2">
                  <QualityGauge score={timetable.qualityScore * 100} size="sm" />
                  <span className="text-sm text-muted-foreground">{t("qualityScore")}</span>
                </div>
              )}
            </div>
          </div>
        </div>
        <div className="flex items-center gap-2">
          {(timetable.status === "Generated" || timetable.status === "Draft") && (
            <Button onClick={async () => { try { await publishMutation.mutateAsync(id); toast.success("Timeplan publisert"); } catch { toast.error("Publisering mislyktes"); } }} disabled={publishMutation.isPending}>
              {t("publish")}
            </Button>
          )}
          <Button variant="outline" asChild>
            <Link href={`/${locale}/timetables/${id}/report`}>
              <FileText className="w-4 h-4 mr-2" />{t("reportTitle")}
            </Link>
          </Button>
        </div>
      </div>

      {timetable.status === "Generating" && (
        <div className="space-y-2">
          <p className="text-sm text-muted-foreground">{t("generationProgress")}</p>
          <Progress value={timetable.progressPercentage ?? 0} className="h-2" />
        </div>
      )}

      {timetable.status !== "Generating" && timetable.status !== "Failed" && (
        <div className="flex items-center gap-4 flex-wrap">
          <Tabs value={viewMode} onValueChange={(v) => setViewMode(v as ViewMode)}>
            <TabsList>
              <TabsTrigger value="class">{t("viewByClass")}</TabsTrigger>
              <TabsTrigger value="teacher">{t("viewByTeacher")}</TabsTrigger>
            </TabsList>
          </Tabs>
          {viewMode === "class" && (
            <Input
              className="w-72"
              placeholder="Klasse-ID (lim inn fra klasser-siden)"
              value={selectedClassId}
              onChange={(e) => setSelectedClassId(e.target.value)}
            />
          )}
          {viewMode === "teacher" && (
            <Select value={selectedTeacherId} onValueChange={setSelectedTeacherId}>
              <SelectTrigger className="w-56"><SelectValue placeholder="Velg lærer..." /></SelectTrigger>
              <SelectContent>
                {teachers.map((teacher) => (
                  <SelectItem key={teacher.id} value={teacher.id}>{teacher.label}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        </div>
      )}

      {hasSelection && <TimetableGrid entries={entries} days={DAY_NAMES} slots={DEFAULT_SLOTS} />}
      {!hasSelection && timetable.status !== "Generating" && (
        <div className="text-center py-16 text-muted-foreground">
          Velg en {viewMode === "class" ? "klasse" : "lærer"} for å vise timeplanen.
        </div>
      )}
    </div>
  );
}
