"use client";

import { useGrades, useCreateGrade, useUpdateGrade, useDeleteGrade } from "@/lib/api/hooks/use-grades";
import { useState } from "react";
import { Pencil, Trash2, Check, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function GradesPage() {
  const { data: grades, isLoading } = useGrades();
  const createGrade = useCreateGrade();
  const updateGrade = useUpdateGrade();
  const deleteGrade = useDeleteGrade();
  const [name, setName] = useState("");
  const [sortOrder, setSortOrder] = useState(1);

  const [editingId, setEditingId] = useState<string | null>(null);
  const [editName, setEditName] = useState("");
  const [editSortOrder, setEditSortOrder] = useState(1);

  function handleCreate() {
    if (!name.trim()) return;
    createGrade.mutate({ name, sortOrder }, { onSuccess: () => { setName(""); setSortOrder(1); } });
  }

  function startEdit(grade: { id: string; name?: string | null; sortOrder?: number | null }) {
    setEditingId(grade.id);
    setEditName(grade.name ?? "");
    setEditSortOrder(grade.sortOrder ?? 1);
  }

  function handleSave(id: string) {
    if (!editName.trim()) return;
    updateGrade.mutate(
      { id, body: { name: editName, sortOrder: editSortOrder } },
      { onSuccess: () => setEditingId(null) }
    );
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
        {grades?.map((grade) =>
          editingId === grade.id ? (
            <div key={grade.id!} className="flex items-center gap-2 px-4 py-2 rounded-xl border bg-card shadow-sm">
              <Input
                value={editName}
                onChange={(e) => setEditName(e.target.value)}
                className="flex-1 h-8 text-sm"
              />
              <Input
                type="number"
                value={editSortOrder}
                onChange={(e) => setEditSortOrder(Number(e.target.value))}
                className="w-24 h-8 text-sm"
              />
              <Button
                size="icon"
                variant="ghost"
                className="h-8 w-8 text-green-600 hover:text-green-700 hover:bg-green-50"
                onClick={() => handleSave(grade.id!)}
                disabled={updateGrade.isPending}
              >
                <Check className="w-4 h-4" />
              </Button>
              <Button
                size="icon"
                variant="ghost"
                className="h-8 w-8 text-muted-foreground hover:text-foreground"
                onClick={() => setEditingId(null)}
              >
                <X className="w-4 h-4" />
              </Button>
            </div>
          ) : (
            <div key={grade.id!} className="flex items-center justify-between px-4 py-3 rounded-xl border bg-card shadow-sm">
              <span className="font-medium">{grade.name}</span>
              <div className="flex gap-1">
                <Button
                  variant="outline"
                  size="icon"
                  className="text-muted-foreground hover:text-foreground"
                  onClick={() => startEdit({ id: grade.id!, name: grade.name, sortOrder: grade.sortOrder })}
                >
                  <Pencil className="w-4 h-4" />
                </Button>
                <Button
                  variant="outline"
                  size="icon"
                  className="border-destructive/30 text-destructive/60 hover:bg-destructive/10 hover:text-destructive hover:border-destructive/50"
                  onClick={() => deleteGrade.mutate(grade.id!)}
                >
                  <Trash2 className="w-4 h-4" />
                </Button>
              </div>
            </div>
          )
        )}
      </div>
    </div>
  );
}
