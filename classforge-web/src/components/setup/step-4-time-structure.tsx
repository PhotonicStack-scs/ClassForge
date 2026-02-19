"use client";

import { useWizardStore } from "@/lib/stores/wizard-store";
import { Button } from "@/components/ui/button";

export function Step4TimeStructure() {
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">Time Structure</h2>
      <p className="text-muted-foreground mb-4">Configure teaching days and time slots. Come back here when done.</p>
      <Button onClick={() => { markStepCompleted(4); setCurrentStep(5); }}>Continue</Button>
    </div>
  );
}
