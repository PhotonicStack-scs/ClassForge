"use client";

import { useState } from "react";
import { useWizardStore } from "@/lib/stores/wizard-store";
import {
  useGrades,
  useCreateGrade,
  useDeleteGrade,
  useGroups,
  useCreateGroup,
  useDeleteGroup,
} from "@/lib/api/hooks/use-grades";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Trash2, Plus, GraduationCap } from "lucide-react";
import { toast } from "sonner";
import type { components } from "@/lib/api/schema";

type GradeResponse = components["schemas"]["GradeResponse"];

function nextGroupLetter(existing: string[]): string {
  const letters = existing
    .map((n) => n.trim().toUpperCase())
    .filter((n) => n.length === 1 && n >= "A" && n <= "Z");
  if (letters.length === 0) return "A";
  const last = letters.sort().at(-1)!;
  return last < "Z" ? String.fromCharCode(last.charCodeAt(0) + 1) : "";
}

interface GradeItemProps {
  grade: GradeResponse;
  onDelete: () => void;
}

function GradeItem({ grade, onDelete }: GradeItemProps) {
  const gradeId = grade.id!;
  const { data: groups = [] } = useGroups(gradeId);
  const createGroup = useCreateGroup(gradeId);
  const deleteGroup = useDeleteGroup();

  const next = nextGroupLetter(groups.map((g) => g.name ?? ""));

  function handleAddGroup() {
    if (!next) return;
    createGroup.mutate({ name: next });
  }

  return (
    <li className="py-1.5 px-3 rounded-md bg-muted/50 space-y-1.5">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium">{grade.name}</span>
        <Button
          variant="ghost"
          size="icon"
          className="h-7 w-7 text-muted-foreground hover:text-destructive"
          onClick={onDelete}
        >
          <Trash2 className="w-3.5 h-3.5" />
        </Button>
      </div>

      {/* Groups row */}
      <div className="flex flex-wrap items-center gap-1.5 pl-0.5">
        {groups.map((group) => (
          <Badge key={group.id} variant="secondary" className="gap-1 pr-1 text-xs">
            {group.name}
            <button
              className="text-muted-foreground hover:text-destructive leading-none ml-0.5"
              onClick={() => deleteGroup.mutate({ gradeId, id: group.id! })}
              aria-label={`Remove group ${group.name}`}
            >
              ×
            </button>
          </Badge>
        ))}
        {next && (
          <Button
            variant="ghost"
            size="icon"
            className="h-5 w-5 text-muted-foreground hover:text-foreground"
            onClick={handleAddGroup}
            disabled={createGroup.isPending}
            title={`Add group ${next}`}
          >
            <Plus className="w-3 h-3" />
          </Button>
        )}
      </div>
    </li>
  );
}

export function Step1Grades() {
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  const { data: grades = [], isLoading } = useGrades();
  const createGrade = useCreateGrade();
  const deleteGrade = useDeleteGrade();

  const [name, setName] = useState("");

  async function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;
    try {
      await createGrade.mutateAsync({ name: name.trim(), sortOrder: grades.length });
      setName("");
    } catch {
      toast.error("Failed to add grade");
    }
  }

  function handleContinue() {
    markStepCompleted(1);
    setCurrentStep(2);
  }

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold flex items-center gap-2">
          <GraduationCap className="w-5 h-5" />
          Grades
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          Add the grade levels at your school (e.g. Grade 1, Grade 2, … Grade 10).
        </p>
      </div>

      <form onSubmit={handleAdd} className="flex gap-2">
        <Input
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Grade name (e.g. Grade 1)"
          className="max-w-xs"
        />
        <Button type="submit" size="sm" disabled={createGrade.isPending || !name.trim()}>
          <Plus className="w-4 h-4 mr-1" />
          Add
        </Button>
      </form>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">Loading…</p>
      ) : grades.length === 0 ? (
        <p className="text-sm text-muted-foreground">No grades added yet.</p>
      ) : (
        <ul className="space-y-1">
          {grades.map((g) => (
            <GradeItem
              key={g.id}
              grade={g}
              onDelete={() => deleteGrade.mutate(g.id!)}
            />
          ))}
        </ul>
      )}

      <div className="pt-2 space-y-1">
        <Button onClick={handleContinue} disabled={grades.length === 0}>
          Continue{grades.length > 0 && <Badge variant="secondary" className="ml-2">{grades.length} grade{grades.length !== 1 ? "s" : ""}</Badge>}
        </Button>
        {grades.length === 0 && (
          <p className="text-xs text-muted-foreground">Add at least one grade to continue</p>
        )}
      </div>
    </div>
  );
}
