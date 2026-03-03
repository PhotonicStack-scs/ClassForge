"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { useWizardStore } from "@/lib/stores/wizard-store";
import {
  useYears,
  useCreateYear,
  useDeleteYear,
  useClasses,
  useCreateClass,
  useDeleteClass,
} from "@/lib/api/hooks/use-years";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Trash2, Plus, GraduationCap, X } from "lucide-react";
import { toast } from "sonner";
import type { components } from "@/lib/api/schema";

type YearResponse = components["schemas"]["YearResponse"];

function WizardYearRow({ year }: { year: YearResponse }) {
  const ty = useTranslations("years");
  const tc = useTranslations("common");
  const yearId = year.id!;
  const [newClassName, setNewClassName] = useState("");

  const { data: classes = [] } = useClasses(yearId);
  const deleteYear = useDeleteYear();
  const createClass = useCreateClass(yearId);
  const deleteClass = useDeleteClass(yearId);

  async function handleAddClass(e: React.FormEvent) {
    e.preventDefault();
    if (!newClassName.trim()) return;
    try {
      await createClass.mutateAsync({ name: newClassName.trim(), sortOrder: classes.length });
      setNewClassName("");
    } catch {
      toast.error(tc("error"));
    }
  }

  return (
    <li className="rounded-md border bg-card px-3 py-2 space-y-1.5">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium">{year.name}</span>
        <Button
          variant="ghost"
          size="icon"
          className="h-7 w-7 text-muted-foreground hover:text-destructive"
          onClick={() => deleteYear.mutate(yearId)}
          disabled={deleteYear.isPending}
        >
          <Trash2 className="w-3.5 h-3.5" />
        </Button>
      </div>
      <div className="flex items-center gap-2 flex-wrap">
        <div className="flex flex-wrap gap-1.5 flex-1 items-center min-h-[24px]">
          {classes.length === 0 && (
            <span className="text-xs text-muted-foreground">{ty("noClasses")}</span>
          )}
          {classes.map((cls) => (
            <span
              key={cls.id}
              className="inline-flex items-center gap-1 bg-muted rounded-md px-2 py-0.5 text-xs font-medium"
            >
              {cls.name}
              <button
                onClick={() => deleteClass.mutate(cls.id!)}
                disabled={deleteClass.isPending}
                className="text-muted-foreground hover:text-foreground leading-none"
              >
                <X className="w-2.5 h-2.5" />
              </button>
            </span>
          ))}
        </div>
        <form onSubmit={handleAddClass} className="flex items-center gap-1 shrink-0">
          <Input
            value={newClassName}
            onChange={(e) => setNewClassName(e.target.value)}
            placeholder={ty("classNamePlaceholder")}
            className="h-6 w-14 text-xs"
          />
          <Button
            type="submit"
            size="icon"
            variant="outline"
            className="h-6 w-6"
            disabled={createClass.isPending || !newClassName.trim()}
          >
            <Plus className="w-3 h-3" />
          </Button>
        </form>
      </div>
    </li>
  );
}

export function Step1Years() {
  const t = useTranslations("setup");
  const ty = useTranslations("years");
  const tc = useTranslations("common");
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  const { data: years = [], isLoading } = useYears();
  const createYear = useCreateYear();

  const [name, setName] = useState("");

  async function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;
    try {
      await createYear.mutateAsync({ name: name.trim(), sortOrder: years.length });
      setName("");
    } catch {
      toast.error(tc("error"));
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
          {ty("title")}
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          {t("step1Description")}
        </p>
      </div>

      <form onSubmit={handleAdd} className="flex gap-2">
        <Input
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder={ty("yearNamePlaceholder")}
          className="max-w-xs"
        />
        <Button type="submit" size="sm" disabled={createYear.isPending || !name.trim()}>
          <Plus className="w-4 h-4 mr-1" />
          {tc("add")}
        </Button>
      </form>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">{tc("loading")}</p>
      ) : years.length === 0 ? (
        <p className="text-sm text-muted-foreground">{ty("noYears")}</p>
      ) : (
        <ul className="space-y-1.5">
          {years.map((y) => (
            <WizardYearRow key={y.id} year={y} />
          ))}
        </ul>
      )}

      <div className="pt-2 space-y-1">
        <Button onClick={handleContinue} disabled={years.length === 0}>
          {tc("continue")}{years.length > 0 && <Badge variant="secondary" className="ml-2">{years.length}</Badge>}
        </Button>
        {years.length === 0 && (
          <p className="text-xs text-muted-foreground">{t("addYearFirst")}</p>
        )}
      </div>
    </div>
  );
}
