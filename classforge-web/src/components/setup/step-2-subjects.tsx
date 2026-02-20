"use client";

import { useState } from "react";
import { useWizardStore } from "@/lib/stores/wizard-store";
import { useSubjects, useCreateSubject, useDeleteSubject } from "@/lib/api/hooks/use-subjects";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Trash2, Plus, BookOpen } from "lucide-react";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

const SUBJECT_COLORS = [
  "#4CAF50","#2196F3","#FF5722","#9C27B0","#FF9800",
  "#00BCD4","#F44336","#3F51B5","#8BC34A","#FFC107",
  "#009688","#E91E63","#673AB7","#CDDC39","#795548",
  "#607D8B","#FF4081","#00E676","#40C4FF","#FFD740",
];

export function Step2Subjects() {
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  const { data: subjects = [], isLoading } = useSubjects();
  const createSubject = useCreateSubject();
  const deleteSubject = useDeleteSubject();

  const [name, setName] = useState("");
  const [color, setColor] = useState(SUBJECT_COLORS[0]);

  async function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;
    try {
      await createSubject.mutateAsync({ name: name.trim(), color, maxPeriodsPerDay: 2, allowDoublePeriods: false, requiresSpecialRoom: false });
      setName("");
      // advance color for next subject
      const nextIdx = (SUBJECT_COLORS.indexOf(color) + 1) % SUBJECT_COLORS.length;
      setColor(SUBJECT_COLORS[nextIdx]);
    } catch {
      toast.error("Failed to add subject");
    }
  }

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold flex items-center gap-2">
          <BookOpen className="w-5 h-5" />
          Subjects
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          Add the subjects taught at your school. Each subject gets a color for the timetable.
        </p>
      </div>

      <form onSubmit={handleAdd} className="space-y-3">
        <div className="flex gap-2">
          <Input
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Subject name (e.g. Mathematics)"
            className="max-w-xs"
          />
          <Button type="submit" size="sm" disabled={createSubject.isPending || !name.trim()}>
            <Plus className="w-4 h-4 mr-1" />
            Add
          </Button>
        </div>
        <div>
          <p className="text-xs text-muted-foreground mb-1.5">Pick a color</p>
          <div className="flex flex-wrap gap-1.5">
            {SUBJECT_COLORS.map((c) => (
              <button
                key={c}
                type="button"
                onClick={() => setColor(c)}
                className={cn(
                  "w-6 h-6 rounded-full border-2 transition-transform",
                  color === c ? "border-foreground scale-110" : "border-transparent"
                )}
                style={{ backgroundColor: c }}
                aria-label={c}
              />
            ))}
          </div>
        </div>
      </form>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">Loading…</p>
      ) : subjects.length === 0 ? (
        <p className="text-sm text-muted-foreground">No subjects added yet.</p>
      ) : (
        <ul className="space-y-1">
          {subjects.map((s) => (
            <li key={s.id} className="flex items-center justify-between py-1.5 px-3 rounded-md bg-muted/50">
              <div className="flex items-center gap-2">
                <span className="w-3 h-3 rounded-full shrink-0" style={{ backgroundColor: s.color ?? "#888" }} />
                <span className="text-sm font-medium">{s.name}</span>
              </div>
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7 text-muted-foreground hover:text-destructive"
                onClick={() => deleteSubject.mutate(s.id!)}
              >
                <Trash2 className="w-3.5 h-3.5" />
              </Button>
            </li>
          ))}
        </ul>
      )}

      <div className="pt-2 space-y-1">
        <Button onClick={() => { markStepCompleted(2); setCurrentStep(3); }} disabled={subjects.length === 0}>
          Continue{subjects.length > 0 && <Badge variant="secondary" className="ml-2">{subjects.length} subject{subjects.length !== 1 ? "s" : ""}</Badge>}
        </Button>
        {subjects.length === 0 && (
          <p className="text-xs text-muted-foreground">Add at least one subject to continue</p>
        )}
      </div>
    </div>
  );
}
