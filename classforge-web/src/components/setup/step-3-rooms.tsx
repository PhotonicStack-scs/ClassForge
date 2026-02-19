"use client";

import { useWizardStore } from "@/lib/stores/wizard-store";
import { Button } from "@/components/ui/button";

export function Step3Rooms() {
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">Rooms</h2>
      <p className="text-muted-foreground mb-4">Configure rooms in the Academic Structure section. Come back here when done.</p>
      <Button onClick={() => { markStepCompleted(3); setCurrentStep(4); }}>Continue</Button>
    </div>
  );
}
