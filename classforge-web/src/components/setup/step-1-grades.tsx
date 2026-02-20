"use client";

import { useState } from "react";
import { useWizardStore } from "@/lib/stores/wizard-store";
import { useGrades, useCreateGrade, useDeleteGrade } from "@/lib/api/hooks/use-grades";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Trash2, Plus, GraduationCap } from "lucide-react";
import { toast } from "sonner";

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
            <li key={g.id} className="flex items-center justify-between py-1.5 px-3 rounded-md bg-muted/50">
              <span className="text-sm font-medium">{g.name}</span>
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7 text-muted-foreground hover:text-destructive"
                onClick={() => deleteGrade.mutate(g.id!)}
              >
                <Trash2 className="w-3.5 h-3.5" />
              </Button>
            </li>
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
