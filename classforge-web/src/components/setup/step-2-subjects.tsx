"use client";

import { useWizardStore } from "@/lib/stores/wizard-store";
import { Button } from "@/components/ui/button";

export function Step2Subjects() {
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">Subjects</h2>
      <p className="text-muted-foreground mb-4">Configure subjects in the Academic Structure section. Come back here when done.</p>
      <Button onClick={() => { markStepCompleted(2); setCurrentStep(3); }}>Continue</Button>
    </div>
  );
}
