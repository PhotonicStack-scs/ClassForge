"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { Loader2 } from "lucide-react";
import { useWizardStore } from "@/lib/stores/wizard-store";
import { useBulkCreateYears, useYears } from "@/lib/api/hooks/use-years";
import { useBulkCreateSubjects, useSubjects } from "@/lib/api/hooks/use-subjects";
import { SUBJECT_COLORS } from "@/lib/utils/color";
import { Card, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

const TEMPLATES = [
  { id: "barneskole", key: "barneskole" },
  { id: "ungdomsskole", key: "ungdomsskole" },
  { id: "combined", key: "combined" },
  { id: "videregaende", key: "videregaende" },
  { id: "custom", key: "customSetup" },
] as const;

type TemplateId = (typeof TEMPLATES)[number]["id"];

const TEMPLATE_YEARS: Record<TemplateId, { name: string; sortOrder: number }[]> = {
  barneskole: Array.from({ length: 7 }, (_, i) => ({ name: `${i + 1}. klasse`, sortOrder: i })),
  ungdomsskole: Array.from({ length: 3 }, (_, i) => ({ name: `${i + 8}. klasse`, sortOrder: i })),
  combined: Array.from({ length: 10 }, (_, i) => ({ name: `${i + 1}. klasse`, sortOrder: i })),
  videregaende: Array.from({ length: 3 }, (_, i) => ({ name: `${i + 1}. klasse`, sortOrder: i })),
  custom: [],
};

const SUBJECT_NAMES = [
  "Norsk", "Matematikk", "Engelsk", "Naturfag", "Samfunnsfag",
  "KRLE", "Kunst og håndverk", "Musikk", "Kroppsøving", "Mat og helse",
];

const TEMPLATE_SUBJECTS: Record<TemplateId, { name: string; color: string }[]> = {
  barneskole: SUBJECT_NAMES.map((name, i) => ({ name, color: SUBJECT_COLORS[i] as string })),
  ungdomsskole: SUBJECT_NAMES.map((name, i) => ({ name, color: SUBJECT_COLORS[i] as string })),
  combined: SUBJECT_NAMES.map((name, i) => ({ name, color: SUBJECT_COLORS[i] as string })),
  videregaende: [],
  custom: [],
};

export function Step0Template() {
  const t = useTranslations("setup");
  const { template, setTemplate, markStepCompleted, setCurrentStep } = useWizardStore();
  const [seeding, setSeeding] = useState(false);

  const { data: existingYears = [] } = useYears();
  const { data: existingSubjects = [] } = useSubjects();
  const bulkCreateYears = useBulkCreateYears();
  const bulkCreateSubjects = useBulkCreateSubjects();

  async function handleSelect(id: TemplateId) {
    setTemplate(id);
    markStepCompleted(0);

    if (id === "custom") {
      setCurrentStep(1);
      return;
    }

    setSeeding(true);
    try {
      const years = TEMPLATE_YEARS[id];
      if (years.length > 0 && existingYears.length === 0) {
        await bulkCreateYears.mutateAsync({ items: years });
      }

      const subjects = TEMPLATE_SUBJECTS[id];
      if (subjects.length > 0 && existingSubjects.length === 0) {
        await bulkCreateSubjects.mutateAsync({ items: subjects });
      }
    } finally {
      setSeeding(false);
    }

    markStepCompleted(1);
    if (TEMPLATE_SUBJECTS[id].length > 0) {
      markStepCompleted(3);
    }
    setCurrentStep(2);
  }

  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">
        {t("selectTemplate")}
        {seeding && <Loader2 className="inline-block ml-2 h-4 w-4 animate-spin" />}
      </h2>
      <div className={"grid grid-cols-1 sm:grid-cols-2 gap-4" + (seeding ? " pointer-events-none opacity-50" : "")}>
        {TEMPLATES.map((tmpl) => (
          <Card
            key={tmpl.id}
            className={"cursor-pointer border-2 transition-colors " + (template === tmpl.id ? "border-primary" : "border-border hover:border-primary/50")}
            onClick={() => handleSelect(tmpl.id)}
          >
            <CardHeader>
              <CardTitle>{t(tmpl.key)}</CardTitle>
              <CardDescription>{t("templateDescription")}</CardDescription>
            </CardHeader>
          </Card>
        ))}
      </div>
    </div>
  );
}
