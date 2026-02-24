"use client";

import { useGrades, useCreateGrade, useDeleteGrade } from "@/lib/api/hooks/use-grades";
import { useState } from "react";
import { Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function GradesPage() {
  const { data: grades, isLoading } = useGrades();
  const createGrade = useCreateGrade();
  const deleteGrade = useDeleteGrade();
  const [name, setName] = useState("");
  const [sortOrder, setSortOrder] = useState(1);

  function handleCreate() {
    if (!name.trim()) return;
    createGrade.mutate({ name, sortOrder }, { onSuccess: () => { setName(""); setSortOrder(1); } });
  }

  if (isLoading) return <div className="p-8">Loading...</div>;

  return (
    <div className="container mx-auto p-8 max-w-3xl">
      <h1 className="text-2xl font-bold mb-6">Grades</h1>
      <Card className="mb-6">
        <CardHeader><CardTitle>Add Grade</CardTitle></CardHeader>
        <CardContent>
          <div className="flex gap-3 items-end">
            <div className="flex-1 space-y-1">
              <Label htmlFor="gradeName">Grade name</Label>
              <Input id="gradeName" placeholder="e.g. 1. klasse" value={name} onChange={(e) => setName(e.target.value)} />
            </div>
            <div className="w-28 space-y-1">
              <Label htmlFor="sortOrder">Sort order</Label>
              <Input id="sortOrder" type="number" value={sortOrder} onChange={(e) => setSortOrder(Number(e.target.value))} />
            </div>
            <Button onClick={handleCreate} disabled={createGrade.isPending}>Add</Button>
          </div>
        </CardContent>
      </Card>
      <div className="space-y-2">
        {grades?.map((grade) => (
          <div key={grade.id!} className="flex items-center justify-between px-4 py-3 rounded-xl border bg-card shadow-sm">
            <span className="font-medium">{grade.name}</span>
            <Button
              variant="outline"
              size="icon"
              className="border-destructive/30 text-destructive/60 hover:bg-destructive/10 hover:text-destructive hover:border-destructive/50"
              onClick={() => deleteGrade.mutate(grade.id!)}
            >
              <Trash2 className="w-4 h-4" />
            </Button>
          </div>
        ))}
      </div>
    </div>
  );
}
