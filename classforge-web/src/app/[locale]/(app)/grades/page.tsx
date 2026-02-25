"use client";

import {
  useGrades,
  useCreateGrade,
  useUpdateGrade,
  useDeleteGrade,
  useGroups,
  useCreateGroup,
  useUpdateGroup,
  useDeleteGroup,
} from "@/lib/api/hooks/use-grades";
import { useState, useEffect } from "react";
import { Pencil, Trash2, Check, X, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { components } from "@/lib/api/schema";

type GradeResponse = components["schemas"]["GradeResponse"];
type GroupResponse = components["schemas"]["GroupResponse"];

function nextGroupLetter(existing: string[]): string {
  const letters = existing
    .map((n) => n.trim().toUpperCase())
    .filter((n) => n.length === 1 && n >= "A" && n <= "Z");
  if (letters.length === 0) return "A";
  const last = letters.sort().at(-1)!;
  return last < "Z" ? String.fromCharCode(last.charCodeAt(0) + 1) : "";
}

interface GradeRowProps {
  grade: GradeResponse;
  isEditing: boolean;
  editName: string;
  editSortOrder: number;
  onStartEdit: () => void;
  onSave: () => void;
  onCancelEdit: () => void;
  onDelete: () => void;
  setEditName: (v: string) => void;
  setEditSortOrder: (v: number) => void;
  updateGradePending: boolean;
}

function GradeRow({
  grade,
  isEditing,
  editName,
  editSortOrder,
  onStartEdit,
  onSave,
  onCancelEdit,
  onDelete,
  setEditName,
  setEditSortOrder,
  updateGradePending,
}: GradeRowProps) {
  const gradeId = grade.id!;
  const { data: groups = [] } = useGroups(gradeId);
  const createGroup = useCreateGroup(gradeId);
  const updateGroup = useUpdateGroup();
  const deleteGroup = useDeleteGroup();

  const [addInput, setAddInput] = useState("");
  const [renamingId, setRenamingId] = useState<string | null>(null);
  const [renameValue, setRenameValue] = useState("");

  const suggested = nextGroupLetter(groups.map((g) => g.name ?? ""));

  useEffect(() => { setAddInput(suggested); }, [suggested]);

  function handleAddGroup() {
    const val = addInput.trim();
    if (!val) return;
    const duplicate = groups.some((g) => (g.name ?? "").trim().toUpperCase() === val.toUpperCase());
    if (duplicate) return;
    createGroup.mutate({ name: val });
  }

  function startRename(group: GroupResponse) {
    setRenamingId(group.id!);
    setRenameValue(group.name ?? "");
  }

  function handleRename(group: GroupResponse) {
    if (!renameValue.trim()) return;
    updateGroup.mutate(
      { gradeId, id: group.id!, body: { name: renameValue.trim() } },
      { onSuccess: () => setRenamingId(null) }
    );
  }

  return (
    <div className="rounded-xl border bg-card shadow-sm overflow-hidden">
      {/* Grade row */}
      {isEditing ? (
        <div className="flex items-center gap-2 px-4 py-2">
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
            onClick={onSave}
            disabled={updateGradePending}
          >
            <Check className="w-4 h-4" />
          </Button>
          <Button
            size="icon"
            variant="ghost"
            className="h-8 w-8 text-muted-foreground hover:text-foreground"
            onClick={onCancelEdit}
          >
            <X className="w-4 h-4" />
          </Button>
        </div>
      ) : (
        <div className="flex items-center justify-between px-4 py-3">
          <span className="font-medium">{grade.name}</span>
          <div className="flex gap-1">
            <Button
              variant="outline"
              size="icon"
              className="text-muted-foreground hover:text-foreground"
              onClick={onStartEdit}
            >
              <Pencil className="w-4 h-4" />
            </Button>
            <Button
              variant="outline"
              size="icon"
              className="border-destructive/30 text-destructive/60 hover:bg-destructive/10 hover:text-destructive hover:border-destructive/50"
              onClick={onDelete}
            >
              <Trash2 className="w-4 h-4" />
            </Button>
          </div>
        </div>
      )}

      {/* Groups subsection */}
      <div className="border-t px-4 py-2 pl-6 flex flex-wrap items-center gap-2">
        {groups.map((group) =>
          renamingId === group.id ? (
            <div key={group.id} className="flex items-center gap-1">
              <Input
                value={renameValue}
                onChange={(e) => setRenameValue(e.target.value)}
                className="w-14 h-6 text-xs text-center px-1"
                onKeyDown={(e) => {
                  if (e.key === "Enter") handleRename(group);
                  if (e.key === "Escape") setRenamingId(null);
                }}
                autoFocus
              />
              <Button
                size="icon"
                variant="ghost"
                className="h-6 w-6 text-green-600 hover:text-green-700 hover:bg-green-50"
                onClick={() => handleRename(group)}
                disabled={updateGroup.isPending}
              >
                <Check className="w-3 h-3" />
              </Button>
              <Button
                size="icon"
                variant="ghost"
                className="h-6 w-6 text-muted-foreground"
                onClick={() => setRenamingId(null)}
              >
                <X className="w-3 h-3" />
              </Button>
            </div>
          ) : (
            <span
              key={group.id}
              className="inline-flex items-center gap-1 text-xs font-medium px-2 py-0.5 rounded border bg-muted/50"
            >
              <button
                className="hover:underline cursor-pointer"
                onClick={() => startRename(group)}
              >
                {group.name}
              </button>
              <button
                className="text-muted-foreground hover:text-destructive leading-none"
                onClick={() => deleteGroup.mutate({ gradeId, id: group.id! })}
                aria-label={`Remove group ${group.name}`}
              >
                ×
              </button>
            </span>
          )
        )}

        {/* Add group */}
        <div className="flex items-center gap-1 ml-auto">
          <Input
            value={addInput}
            onChange={(e) => setAddInput(e.target.value)}
            placeholder={suggested}
            className="w-10 h-6 text-xs text-center px-1"
            onKeyDown={(e) => { if (e.key === "Enter") handleAddGroup(); }}
          />
          <Button
            size="icon"
            variant="outline"
            className="h-6 w-6 text-muted-foreground hover:text-foreground"
            onClick={handleAddGroup}
            disabled={createGroup.isPending || !addInput.trim() || groups.some((g) => (g.name ?? "").trim().toUpperCase() === addInput.trim().toUpperCase())}
          >
            <Plus className="w-3 h-3" />
          </Button>
        </div>
      </div>
    </div>
  );
}

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
        {grades?.map((grade) => (
          <GradeRow
            key={grade.id!}
            grade={grade}
            isEditing={editingId === grade.id}
            editName={editName}
            editSortOrder={editSortOrder}
            onStartEdit={() => startEdit({ id: grade.id!, name: grade.name, sortOrder: grade.sortOrder })}
            onSave={() => handleSave(grade.id!)}
            onCancelEdit={() => setEditingId(null)}
            onDelete={() => deleteGrade.mutate(grade.id!)}
            setEditName={setEditName}
            setEditSortOrder={setEditSortOrder}
            updateGradePending={updateGrade.isPending}
          />
        ))}
      </div>
    </div>
  );
}
