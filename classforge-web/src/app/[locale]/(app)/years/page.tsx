"use client";

import { useTranslations } from "next-intl";
import {
  useYears,
  useCreateYear,
  useUpdateYear,
  useDeleteYear,
  useClasses,
  useCreateClass,
  useDeleteClass,
} from "@/lib/api/hooks/use-years";
import { useState } from "react";
import { Pencil, Trash2, Check, X, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { components } from "@/lib/api/schema";

type YearResponse = components["schemas"]["YearResponse"];

function YearSection({ year }: { year: YearResponse }) {
  const t = useTranslations("years");
  const tc = useTranslations("common");
  const yearId = year.id!;

  const [editingYear, setEditingYear] = useState(false);
  const [editYearName, setEditYearName] = useState("");
  const [editYearSortOrder, setEditYearSortOrder] = useState(1);
  const [newClassName, setNewClassName] = useState("");

  const { data: classes = [] } = useClasses(yearId);
  const updateYear = useUpdateYear();
  const deleteYear = useDeleteYear();
  const createClass = useCreateClass(yearId);
  const deleteClass = useDeleteClass(yearId);

  function startEditYear() {
    setEditingYear(true);
    setEditYearName(year.name ?? "");
    setEditYearSortOrder(year.sortOrder ?? 1);
  }

  function handleSaveYear() {
    if (!editYearName.trim()) return;
    updateYear.mutate(
      { id: yearId, body: { name: editYearName, sortOrder: editYearSortOrder } },
      { onSuccess: () => setEditingYear(false) }
    );
  }

  function handleAddClass() {
    if (!newClassName.trim()) return;
    createClass.mutate(
      { name: newClassName.trim(), sortOrder: classes.length },
      { onSuccess: () => setNewClassName("") }
    );
  }

  return (
    <Card>
      <CardHeader className="pb-2">
        {editingYear ? (
          <div className="flex items-center gap-2">
            <Input
              value={editYearName}
              onChange={(e) => setEditYearName(e.target.value)}
              className="flex-1 h-8 text-sm"
              onKeyDown={(e) => { if (e.key === "Enter") handleSaveYear(); }}
              autoFocus
            />
            <Input
              type="number"
              value={editYearSortOrder}
              onChange={(e) => setEditYearSortOrder(Number(e.target.value))}
              className="w-20 h-8 text-sm"
            />
            <Button
              size="icon" variant="ghost"
              className="h-8 w-8 text-green-600 hover:text-green-700 hover:bg-green-50"
              onClick={handleSaveYear}
              disabled={updateYear.isPending}
            >
              <Check className="w-4 h-4" />
            </Button>
            <Button
              size="icon" variant="ghost"
              className="h-8 w-8 text-muted-foreground hover:text-foreground"
              onClick={() => setEditingYear(false)}
            >
              <X className="w-4 h-4" />
            </Button>
          </div>
        ) : (
          <div className="flex items-center justify-between">
            <CardTitle className="text-base">{year.name}</CardTitle>
            <div className="flex gap-1">
              <Button
                variant="outline" size="icon"
                className="h-8 w-8 text-muted-foreground hover:text-foreground"
                onClick={startEditYear}
              >
                <Pencil className="w-3.5 h-3.5" />
              </Button>
              <Button
                variant="outline" size="icon"
                className="h-8 w-8 border-destructive/30 text-destructive/60 hover:bg-destructive/10 hover:text-destructive hover:border-destructive/50"
                onClick={() => deleteYear.mutate(yearId)}
                disabled={deleteYear.isPending}
              >
                <Trash2 className="w-3.5 h-3.5" />
              </Button>
            </div>
          </div>
        )}
      </CardHeader>
      <CardContent className="pt-0">
        <div className="flex items-center gap-2 flex-wrap">
          <div className="flex flex-wrap gap-1.5 flex-1 items-center min-h-[28px]">
            {classes.length === 0 && (
              <span className="text-xs text-muted-foreground">{t("noClasses")}</span>
            )}
            {classes.map((cls) => (
              <span
                key={cls.id}
                className="inline-flex items-center gap-1 bg-muted rounded-md px-2 py-0.5 text-sm font-medium"
              >
                {cls.name}
                <button
                  onClick={() => deleteClass.mutate(cls.id!)}
                  disabled={deleteClass.isPending}
                  className="text-muted-foreground hover:text-foreground leading-none"
                >
                  <X className="w-3 h-3" />
                </button>
              </span>
            ))}
          </div>
          <div className="flex items-center gap-1.5 shrink-0">
            <Input
              value={newClassName}
              onChange={(e) => setNewClassName(e.target.value)}
              placeholder={t("classNamePlaceholder")}
              className="h-7 w-16 text-sm"
              onKeyDown={(e) => { if (e.key === "Enter") handleAddClass(); }}
            />
            <Button
              size="icon" variant="outline"
              className="h-7 w-7"
              onClick={handleAddClass}
              disabled={createClass.isPending || !newClassName.trim()}
            >
              <Plus className="w-3.5 h-3.5" />
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export default function YearsPage() {
  const t = useTranslations("years");
  const tc = useTranslations("common");
  const { data: years, isLoading } = useYears();
  const createYear = useCreateYear();
  const [name, setName] = useState("");
  const [sortOrder, setSortOrder] = useState(1);

  function handleCreate() {
    if (!name.trim()) return;
    createYear.mutate(
      { name, sortOrder },
      { onSuccess: () => { setName(""); setSortOrder((years?.length ?? 0) + 2); } }
    );
  }

  if (isLoading) return <div className="p-8">{tc("loading")}</div>;

  return (
    <div className="container mx-auto p-8 max-w-3xl">
      <h1 className="text-2xl font-bold mb-6">{t("title")}</h1>
      <Card className="mb-6">
        <CardHeader><CardTitle>{t("addYear")}</CardTitle></CardHeader>
        <CardContent>
          <div className="flex gap-3 items-end">
            <div className="flex-1 space-y-1">
              <Label htmlFor="yearName">{t("yearName")}</Label>
              <Input
                id="yearName"
                placeholder={t("yearNamePlaceholder")}
                value={name}
                onChange={(e) => setName(e.target.value)}
                onKeyDown={(e) => { if (e.key === "Enter") handleCreate(); }}
              />
            </div>
            <div className="w-28 space-y-1">
              <Label htmlFor="sortOrder">{t("sortOrder")}</Label>
              <Input
                id="sortOrder"
                type="number"
                value={sortOrder}
                onChange={(e) => setSortOrder(Number(e.target.value))}
              />
            </div>
            <Button onClick={handleCreate} disabled={createYear.isPending || !name.trim()}>
              {tc("add")}
            </Button>
          </div>
        </CardContent>
      </Card>
      <div className="space-y-3">
        {years?.map((year) => (
          <YearSection key={year.id} year={year} />
        ))}
        {years?.length === 0 && (
          <p className="text-sm text-muted-foreground py-4 text-center">{t("noYears")}</p>
        )}
      </div>
    </div>
  );
}
