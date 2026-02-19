"use client";

import { useWizardStore } from "@/lib/stores/wizard-store";
import { Button } from "@/components/ui/button";

export function Step1Grades() {
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">Grades &amp; Groups</h2>
      <p className="text-muted-foreground mb-4">Configure grades in the Academic Structure section. Come back here when done.</p>
      <Button onClick={() => { markStepCompleted(1); setCurrentStep(2); }}>Continue</Button>
    </div>
  );
}
