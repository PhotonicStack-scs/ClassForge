"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useWizardStore } from "@/lib/stores/wizard-store";
import { useYears } from "@/lib/api/hooks/use-years";
import { useSubjects } from "@/lib/api/hooks/use-subjects";
import { apiClient } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { Trash2, Plus, LayoutGrid } from "lucide-react";
import { toast } from "sonner";

type YearCurriculumResponse = components["schemas"]["YearCurriculumResponse"];

function useYearCurriculum(yearId: string) {
  return useQuery<YearCurriculumResponse[]>({
    queryKey: ["years", yearId, "curriculum"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET(
        "/api/v1/years/{yearId}/curriculum",
        { params: { path: { yearId } } }
      );
      if (error) throw error;
      return (data ?? []) as YearCurriculumResponse[];
    },
    enabled: !!yearId,
  });
}

function useCreateCurriculumEntry(yearId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: components["schemas"]["CreateYearCurriculumRequest"]) => {
      const { data, error } = await apiClient.POST(
        "/api/v1/years/{yearId}/curriculum",
        { params: { path: { yearId } }, body }
      );
      if (error) throw error;
      return data!;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["years", yearId, "curriculum"] }),
  });
}

function useDeleteCurriculumEntry(yearId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await apiClient.DELETE(
        "/api/v1/years/{yearId}/curriculum/{id}",
        { params: { path: { yearId, id } } }
      );
      if (error) throw error;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["years", yearId, "curriculum"] }),
  });
}

function RequirementsForYear({ yearId, yearName }: { yearId: string; yearName: string }) {
  const { data: requirements = [] } = useYearCurriculum(yearId);
  const { data: subjects = [] } = useSubjects();
  const createReq = useCreateCurriculumEntry(yearId);
  const deleteReq = useDeleteCurriculumEntry(yearId);

  const [subjectId, setSubjectId] = useState("");
  const [periods, setPeriods] = useState("3");

  const availableSubjects = subjects.filter(
    (s) => !requirements.some((r) => r.subjectId === s.id)
  );

  async function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    if (!subjectId) return;
    try {
      await createReq.mutateAsync({
        subjectId,
        periodsPerWeek: parseInt(periods) || 3,
        preferDoublePeriods: false,
      });
      setSubjectId("");
    } catch {
      toast.error("Failed to add requirement");
    }
  }

  return (
    <div className="border rounded-lg p-4 space-y-3">
      <h3 className="font-medium text-sm">{yearName}</h3>

      {requirements.length > 0 && (
        <ul className="space-y-1">
          {requirements.map((r) => (
            <li key={r.id} className="flex items-center justify-between text-sm">
              <span>{r.subjectName ?? subjects.find((s) => s.id === r.subjectId)?.name ?? "—"}</span>
              <div className="flex items-center gap-2">
                <span className="text-muted-foreground">{r.periodsPerWeek} periods/week</span>
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-6 w-6 text-muted-foreground hover:text-destructive"
                  onClick={() => deleteReq.mutate(r.id!)}
                >
                  <Trash2 className="w-3 h-3" />
                </Button>
              </div>
            </li>
          ))}
        </ul>
      )}

      {availableSubjects.length > 0 && (
        <form onSubmit={handleAdd} className="flex items-end gap-2">
          <div className="space-y-1">
            <Label className="text-xs">Subject</Label>
            <Select value={subjectId} onValueChange={setSubjectId}>
              <SelectTrigger className="w-44 h-8 text-xs">
                <SelectValue placeholder="Select subject" />
              </SelectTrigger>
              <SelectContent>
                {availableSubjects.map((s) => (
                  <SelectItem key={s.id} value={s.id!}>{s.name}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-1">
            <Label className="text-xs">Periods/week</Label>
            <Input
              value={periods}
              onChange={(e) => setPeriods(e.target.value)}
              type="number"
              min="1"
              max="10"
              className="w-20 h-8 text-xs"
            />
          </div>
          <Button type="submit" size="sm" className="h-8" disabled={!subjectId || createReq.isPending}>
            <Plus className="w-3.5 h-3.5" />
          </Button>
        </form>
      )}

      {availableSubjects.length === 0 && requirements.length === 0 && (
        <p className="text-xs text-muted-foreground">No subjects available — add subjects in step 2 first.</p>
      )}
    </div>
  );
}

export function Step6LessonConfig() {
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  const { data: years = [], isLoading } = useYears();

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold flex items-center gap-2">
          <LayoutGrid className="w-5 h-5" />
          Lesson Requirements
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          Set how many periods per week each subject should be taught per year.
        </p>
      </div>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">Loading…</p>
      ) : years.length === 0 ? (
        <p className="text-sm text-muted-foreground">No years found — go back and add years first.</p>
      ) : (
        <div className="space-y-3 max-h-96 overflow-y-auto pr-1">
          {years.map((y) => (
            <RequirementsForYear key={y.id} yearId={y.id!} yearName={y.name ?? "—"} />
          ))}
        </div>
      )}

      <div className="pt-2">
        <Button onClick={() => { markStepCompleted(6); setCurrentStep(7); }}>
          Continue to Review
          {years.length > 0 && <Badge variant="secondary" className="ml-2">{years.length} year{years.length !== 1 ? "s" : ""}</Badge>}
        </Button>
      </div>
    </div>
  );
}
