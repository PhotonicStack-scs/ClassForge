"use client";

import { useTimetables, useCreateTimetable, useDeleteTimetable, usePublishTimetable, usePreflight } from "@/lib/api/hooks/use-timetables";
import Link from "next/link";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function TimetablesPage() {
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

  if (isLoading) return <div className="p-8">Loading...</div>;

  return (
    <div className="container mx-auto p-8 max-w-4xl">
      <h1 className="text-2xl font-bold mb-6">Timetables</h1>
      <div className="flex gap-2 mb-4">
        <Button variant="outline" onClick={() => preflight.mutate()} disabled={preflight.isPending}>
          {preflight.isPending ? "Checking..." : "Run Preflight"}
        </Button>
        {preflight.data && (
          <span className="text-sm text-green-600">Preflight: {preflight.data?.isValid ? "Valid" : "Issues found"}</span>
        )}
      </div>
      <Card className="mb-6">
        <CardHeader><CardTitle>Generate Timetable</CardTitle></CardHeader>
        <CardContent>
          <div className="flex gap-2">
            <Input placeholder="Timetable name" value={name} onChange={(e) => setName(e.target.value)} />
            <Button onClick={handleCreate} disabled={createTimetable.isPending}>
              {createTimetable.isPending ? "Generating..." : "Generate"}
            </Button>
          </div>
        </CardContent>
      </Card>
      <div className="space-y-3">
        {timetables?.map((t) => (
          <Card key={t.id!}>
            <CardContent className="pt-4">
              <div className="flex items-center justify-between">
                <div>
                  <span className="font-medium">{t.name}</span>
                  <span className="ml-2 text-sm text-muted-foreground">{t.status}</span>
                  {t.qualityScore != null && (
                    <span className="ml-2 text-sm">Score: {t.qualityScore}</span>
                  )}
                </div>
                <div className="flex gap-2">
                  <Link href={"/timetables/"+t.id}>
                    <Button variant="outline" size="sm">View</Button>
                  </Link>
                  {t.status === "Draft" && (
                    <Button size="sm" onClick={() => publishTimetable.mutate(t.id!)}>Publish</Button>
                  )}
                  <Button variant="destructive" size="sm" onClick={() => deleteTimetable.mutate(t.id!)}>Delete</Button>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
