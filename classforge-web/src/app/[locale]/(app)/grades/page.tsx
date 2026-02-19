"use client";

import { useGrades, useCreateGrade, useDeleteGrade } from "@/lib/api/hooks/use-grades";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
          <div className="flex gap-2">
            <Input placeholder="Grade name" value={name} onChange={(e) => setName(e.target.value)} />
            <Input type="number" placeholder="Sort" value={sortOrder} onChange={(e) => setSortOrder(Number(e.target.value))} className="w-24" />
            <Button onClick={handleCreate} disabled={createGrade.isPending}>Add</Button>
          </div>
        </CardContent>
      </Card>
      <div className="space-y-2">
        {grades?.map((grade) => (
          <Card key={grade.id!}>
            <CardContent className="flex items-center justify-between pt-4">
              <span className="font-medium">{grade.name}</span>
              <Button variant="destructive" size="sm" onClick={() => deleteGrade.mutate(grade.id!)}>Delete</Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
