"use client";

import { useState } from "react";
import { useWizardStore } from "@/lib/stores/wizard-store";
import { useYears, useCreateYear, useDeleteYear } from "@/lib/api/hooks/use-years";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Trash2, Plus, GraduationCap } from "lucide-react";
import { toast } from "sonner";

export function Step1Years() {
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  const { data: years = [], isLoading } = useYears();
  const createYear = useCreateYear();
  const deleteYear = useDeleteYear();

  const [name, setName] = useState("");

  async function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;
    try {
      await createYear.mutateAsync({ name: name.trim(), sortOrder: years.length });
      setName("");
    } catch {
      toast.error("Failed to add year");
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
          Years
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          Add the year levels at your school (e.g. Year 1, Year 2, … Year 10).
        </p>
      </div>

      <form onSubmit={handleAdd} className="flex gap-2">
        <Input
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Year name (e.g. Year 1)"
          className="max-w-xs"
        />
        <Button type="submit" size="sm" disabled={createYear.isPending || !name.trim()}>
          <Plus className="w-4 h-4 mr-1" />
          Add
        </Button>
      </form>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">Loading…</p>
      ) : years.length === 0 ? (
        <p className="text-sm text-muted-foreground">No years added yet.</p>
      ) : (
        <ul className="space-y-1">
          {years.map((y) => (
            <li key={y.id} className="flex items-center justify-between py-1.5 px-3 rounded-md bg-muted/50">
              <span className="text-sm font-medium">{y.name}</span>
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7 text-muted-foreground hover:text-destructive"
                onClick={() => deleteYear.mutate(y.id!)}
              >
                <Trash2 className="w-3.5 h-3.5" />
              </Button>
            </li>
          ))}
        </ul>
      )}

      <div className="pt-2 space-y-1">
        <Button onClick={handleContinue} disabled={years.length === 0}>
          Continue{years.length > 0 && <Badge variant="secondary" className="ml-2">{years.length} year{years.length !== 1 ? "s" : ""}</Badge>}
        </Button>
        {years.length === 0 && (
          <p className="text-xs text-muted-foreground">Add at least one year to continue</p>
        )}
      </div>
    </div>
  );
}
