"use client";

import { useTimetable } from "@/lib/api/hooks/use-timetables";
import { use } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function TimetableDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { data: timetable, isLoading } = useTimetable(id);

  if (isLoading) return <div className="p-8">Loading...</div>;
  if (!timetable) return <div className="p-8">Not found</div>;

  return (
    <div className="container mx-auto p-8 max-w-4xl">
      <h1 className="text-2xl font-bold mb-2">{timetable.name}</h1>
      <div className="flex items-center gap-4 mb-6">
        <span className="text-muted-foreground">Status: {timetable.status}</span>
        {timetable.qualityScore != null && (
          <span>Quality Score: {timetable.qualityScore}</span>
        )}
        {timetable.progressPercentage != null && timetable.status === "Generating" && (
          <span>Progress: {timetable.progressPercentage}%</span>
        )}
      </div>
      {timetable.errorMessage && (
        <Card className="mb-4 border-destructive">
          <CardContent className="pt-4 text-destructive">
            {timetable.errorMessage}
          </CardContent>
        </Card>
      )}
      <Card>
        <CardHeader><CardTitle>Timetable Details</CardTitle></CardHeader>
        <CardContent>
          <dl className="space-y-2">
            <div><dt className="text-sm text-muted-foreground">Generated At</dt><dd>{timetable.generatedAt ?? "N/A"}</dd></div>
            <div><dt className="text-sm text-muted-foreground">Created By</dt><dd>{timetable.createdBy}</dd></div>
          </dl>
        </CardContent>
      </Card>
    </div>
  );
}
