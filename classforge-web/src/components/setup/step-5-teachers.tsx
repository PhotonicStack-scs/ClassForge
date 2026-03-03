"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { useWizardStore } from "@/lib/stores/wizard-store";
import { useTeachers, useCreateTeacher, useDeleteTeacher } from "@/lib/api/hooks/use-teachers";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Trash2, Plus, Users } from "lucide-react";
import { toast } from "sonner";

export function Step5Teachers() {
  const t = useTranslations("setup");
  const tt = useTranslations("teachers");
  const tc = useTranslations("common");
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
      toast.error(tc("error"));
    }
  }

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold flex items-center gap-2">
          <Users className="w-5 h-5" />
          {tt("title")}
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          {t("step5Description")}
        </p>
      </div>

      <form onSubmit={handleAdd} className="space-y-2">
        <div className="flex gap-2">
          <div className="space-y-1">
            <Label htmlFor="tName" className="text-xs">{tc("name")}</Label>
            <Input
              id="tName"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder={tt("fullName")}
              className="max-w-xs"
            />
          </div>
          <div className="space-y-1">
            <Label htmlFor="tEmail" className="text-xs">{tt("email")}</Label>
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
              {tc("add")}
            </Button>
          </div>
        </div>
      </form>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">{tc("loading")}</p>
      ) : teachers.length === 0 ? (
        <p className="text-sm text-muted-foreground">{tt("noTeachers")}</p>
      ) : (
        <ul className="space-y-1">
          {teachers.map((teacher) => (
            <li key={teacher.id} className="flex items-center justify-between py-1.5 px-3 rounded-md bg-muted/50">
              <div>
                <span className="text-sm font-medium">{teacher.name}</span>
                <span className="text-xs text-muted-foreground ml-2">{teacher.email}</span>
              </div>
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7 text-muted-foreground hover:text-destructive"
                onClick={() => deleteTeacher.mutate(teacher.id!)}
              >
                <Trash2 className="w-3.5 h-3.5" />
              </Button>
            </li>
          ))}
        </ul>
      )}

      <div className="pt-2 space-y-1">
        <Button onClick={() => { markStepCompleted(5); setCurrentStep(6); }} disabled={teachers.length === 0}>
          {tc("continue")}{teachers.length > 0 && <Badge variant="secondary" className="ml-2">{teachers.length}</Badge>}
        </Button>
        {teachers.length === 0 && (
          <p className="text-xs text-muted-foreground">{t("addTeacherFirst")}</p>
        )}
      </div>
    </div>
  );
}
