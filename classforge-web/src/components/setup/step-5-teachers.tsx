"use client";

import { useWizardStore } from "@/lib/stores/wizard-store";
import { Button } from "@/components/ui/button";

export function Step5Teachers() {
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">Teachers</h2>
      <p className="text-muted-foreground mb-4">Configure teachers and their qualifications. Come back here when done.</p>
      <Button onClick={() => { markStepCompleted(5); setCurrentStep(6); }}>Continue</Button>
    </div>
  );
}
