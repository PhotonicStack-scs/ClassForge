"use client";

import { useWizardStore } from "@/lib/stores/wizard-store";
import { Button } from "@/components/ui/button";

export function Step6LessonConfig() {
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">Lesson Configuration</h2>
      <p className="text-muted-foreground mb-4">Configure lesson requirements per grade and subject. Come back here when done.</p>
      <Button onClick={() => { markStepCompleted(6); setCurrentStep(7); }}>Continue</Button>
    </div>
  );
}
