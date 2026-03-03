"use client";

import { useTranslations } from "next-intl";
import { useTimetables, useCreateTimetable, useDeleteTimetable, usePublishTimetable, usePreflight } from "@/lib/api/hooks/use-timetables";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function TimetablesPage() {
  const t = useTranslations("timetable");
  const tc = useTranslations("common");
  const params = useParams();
  const locale = (params?.locale as string) ?? "nb";
  const { data: timetables, isLoading } = useTimetables();
  const createTimetable = useCreateTimetable();
  const deleteTimetable = useDeleteTimetable();
  const publishTimetable = usePublishTimetable();
  const preflight = usePreflight();
  const [name, setName] = useState("");

  function handleCreate() {
    if (!name.trim()) return;
    createTimetable.mutate({ name }, { onSuccess: () => setName("") });
  }

  if (isLoading) return <div className="p-8">{tc("loading")}</div>;

  return (
    <div className="container mx-auto p-8 max-w-4xl">
      <h1 className="text-2xl font-bold mb-6">{t("title")}</h1>
      <div className="flex gap-2 mb-4">
        <Button variant="outline" onClick={() => preflight.mutate()} disabled={preflight.isPending}>
          {preflight.isPending ? t("checking") : t("runPreflight")}
        </Button>
        {preflight.data && (
          <span className="text-sm text-green-600">{t("preflightLabel")}{preflight.data?.isValid ? t("preflightValid") : t("preflightIssues")}</span>
        )}
      </div>
      <Card className="mb-6">
        <CardHeader><CardTitle>{t("generate")}</CardTitle></CardHeader>
        <CardContent>
          <div className="flex gap-2">
            <Input placeholder={t("timetableName")} value={name} onChange={(e) => setName(e.target.value)} />
            <Button onClick={handleCreate} disabled={createTimetable.isPending}>
              {createTimetable.isPending ? t("generating") : t("generate")}
            </Button>
          </div>
        </CardContent>
      </Card>
      <div className="space-y-3">
        {timetables?.map((tt) => (
          <Card key={tt.id!}>
            <CardContent className="pt-4">
              <div className="flex items-center justify-between">
                <div>
                  <span className="font-medium">{tt.name}</span>
                  <span className="ml-2 text-sm text-muted-foreground">{tt.status}</span>
                  {tt.qualityScore != null && (
                    <span className="ml-2 text-sm">{t("score")}{tt.qualityScore}</span>
                  )}
                </div>
                <div className="flex gap-2">
                  <Link href={`/${locale}/timetables/${tt.id}`}>
                    <Button variant="outline" size="sm">{t("view")}</Button>
                  </Link>
                  {tt.status === "Draft" && (
                    <Button size="sm" onClick={() => publishTimetable.mutate(tt.id!)}>{t("publish")}</Button>
                  )}
                  <Button variant="destructive" size="sm" onClick={() => deleteTimetable.mutate(tt.id!)}>{tc("delete")}</Button>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
        {timetables?.length === 0 && (
          <p className="text-sm text-muted-foreground py-4 text-center">{t("noTimetables")}</p>
        )}
      </div>
    </div>
  );
}
