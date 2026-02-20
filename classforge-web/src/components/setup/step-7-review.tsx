"use client";

import { useWizardStore } from "@/lib/stores/wizard-store";
import { useUpdateSetupProgress } from "@/lib/api/hooks/use-setup";
import { Button } from "@/components/ui/button";

export function Step7Review() {
  const { completedSteps, markStepCompleted } = useWizardStore();
  const { mutate, isPending } = useUpdateSetupProgress();

  function handleComplete() {
    const progress: Record<string, boolean> = {};
    for (let i = 0; i <= 6; i++) {
      progress["step" + i] = completedSteps.has(i as 0);
    }
    mutate({ setupCompleted: true, setupProgress: progress });
    markStepCompleted(7);
  }

  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">Review &amp; Complete</h2>
      <p className="text-muted-foreground mb-6">Review your setup and mark it as complete.</p>
      <div className="space-y-2 mb-6">
        {["Template","Grades","Subjects","Rooms","Time Structure","Teachers","Lesson Config"].map((label, i) => (
          <div key={i} className="flex items-center gap-2">
            <span className={completedSteps.has(i as 0) ? "text-green-600" : "text-muted-foreground"}>
              {completedSteps.has(i as 0) ? "[x]" : "[ ]"}
            </span>
            <span>{label}</span>
          </div>
        ))}
      </div>
      <Button onClick={handleComplete} disabled={isPending}>
        {isPending ? "Saving..." : "Complete Setup"}
      </Button>
    </div>
  );
}
