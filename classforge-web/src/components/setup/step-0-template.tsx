"use client";

import { useWizardStore } from "@/lib/stores/wizard-store";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

const TEMPLATES = [
  { id: "barneskole", label: "Barneskole", description: "Primary school (1-7)" },
  { id: "ungdomsskole", label: "Ungdomsskole", description: "Lower secondary (8-10)" },
  { id: "combined", label: "Combined", description: "K-10 school" },
  { id: "videregaende", label: "Videregaende", description: "Upper secondary" },
  { id: "custom", label: "Custom", description: "Start from scratch" },
] as const;

export function Step0Template() {
  const { template, setTemplate, markStepCompleted, setCurrentStep } = useWizardStore();

  function handleSelect(t: typeof TEMPLATES[number]["id"]) {
    setTemplate(t);
    markStepCompleted(0);
    setCurrentStep(1);
  }

  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">Choose a Template</h2>
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        {TEMPLATES.map((t) => (
          <Card
            key={t.id}
            className={"cursor-pointer border-2 transition-colors " + (template === t.id ? "border-primary" : "border-border hover:border-primary/50")}
            onClick={() => handleSelect(t.id)}
          >
            <CardHeader>
              <CardTitle>{t.label}</CardTitle>
              <CardDescription>{t.description}</CardDescription>
            </CardHeader>
          </Card>
        ))}
      </div>
    </div>
  );
}
