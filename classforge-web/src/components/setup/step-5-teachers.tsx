"use client";

import { useState } from "react";
import { useWizardStore } from "@/lib/stores/wizard-store";
import { useTeachers, useCreateTeacher, useDeleteTeacher } from "@/lib/api/hooks/use-teachers";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Trash2, Plus, Users } from "lucide-react";
import { toast } from "sonner";

export function Step5Teachers() {
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  const { data: teachers = [], isLoading } = useTeachers();
  const createTeacher = useCreateTeacher();
  const deleteTeacher = useDeleteTeacher();

  const [name, setName] = useState("");
  const [email, setEmail] = useState("");

  async function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim() || !email.trim()) return;
    try {
      await createTeacher.mutateAsync({ name: name.trim(), email: email.trim() });
      setName("");
      setEmail("");
    } catch {
      toast.error("Failed to add teacher");
    }
  }

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold flex items-center gap-2">
          <Users className="w-5 h-5" />
          Teachers
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          Add teachers who will be assigned to lessons. You can set their qualifications and availability later.
        </p>
      </div>

      <form onSubmit={handleAdd} className="space-y-2">
        <div className="flex gap-2">
          <div className="space-y-1">
            <Label htmlFor="tName" className="text-xs">Name</Label>
            <Input
              id="tName"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Full name"
              className="max-w-xs"
            />
          </div>
          <div className="space-y-1">
            <Label htmlFor="tEmail" className="text-xs">Email</Label>
            <Input
              id="tEmail"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="teacher@school.no"
              type="email"
              className="max-w-xs"
            />
          </div>
          <div className="self-end">
            <Button type="submit" size="sm" disabled={createTeacher.isPending || !name.trim() || !email.trim()}>
              <Plus className="w-4 h-4 mr-1" />
              Add
            </Button>
          </div>
        </div>
      </form>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">Loading…</p>
      ) : teachers.length === 0 ? (
        <p className="text-sm text-muted-foreground">No teachers added yet.</p>
      ) : (
        <ul className="space-y-1">
          {teachers.map((t) => (
            <li key={t.id} className="flex items-center justify-between py-1.5 px-3 rounded-md bg-muted/50">
              <div>
                <span className="text-sm font-medium">{t.name}</span>
                <span className="text-xs text-muted-foreground ml-2">{t.email}</span>
              </div>
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7 text-muted-foreground hover:text-destructive"
                onClick={() => deleteTeacher.mutate(t.id!)}
              >
                <Trash2 className="w-3.5 h-3.5" />
              </Button>
            </li>
          ))}
        </ul>
      )}

      <div className="pt-2 space-y-1">
        <Button onClick={() => { markStepCompleted(5); setCurrentStep(6); }} disabled={teachers.length === 0}>
          Continue{teachers.length > 0 && <Badge variant="secondary" className="ml-2">{teachers.length} teacher{teachers.length !== 1 ? "s" : ""}</Badge>}
        </Button>
        {teachers.length === 0 && (
          <p className="text-xs text-muted-foreground">Add at least one teacher to continue</p>
        )}
      </div>
    </div>
  );
}
