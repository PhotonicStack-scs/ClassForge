"use client";

import { useWizardStore } from "@/lib/stores/wizard-store";
import { Step0Template } from "./step-0-template";
import { Step1Grades } from "./step-1-grades";
import { Step2Subjects } from "./step-2-subjects";
import { Step3Rooms } from "./step-3-rooms";
import { Step4TimeStructure } from "./step-4-time-structure";
import { Step5Teachers } from "./step-5-teachers";
import { Step6LessonConfig } from "./step-6-lesson-config";
import { Step7Review } from "./step-7-review";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";

const STEPS = [
  "Template",
  "Grades",
  "Subjects",
  "Rooms",
  "Time Structure",
  "Teachers",
  "Lesson Config",
  "Review",
];

export function WizardShell({ locale }: { locale: string }) {
  const { currentStep, setCurrentStep, completedSteps } = useWizardStore();

  const stepComponents = [
    <Step0Template key={0} />,
    <Step1Grades key={1} />,
    <Step2Subjects key={2} />,
    <Step3Rooms key={3} />,
    <Step4TimeStructure key={4} />,
    <Step5Teachers key={5} />,
    <Step6LessonConfig key={6} />,
    <Step7Review key={7} />,
  ];

  const progress = (completedSteps.size / (STEPS.length - 1)) * 100;

  return (
    <div className="min-h-screen bg-background">
      <div className="container mx-auto px-4 py-8 max-w-4xl">
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-2">School Setup</h1>
          <Progress value={progress} className="h-2" />
          <div className="flex justify-between mt-2">
            {STEPS.map((step, i) => (
              <button
                key={i}
                onClick={() => setCurrentStep(i as 0)}
                className={
                  "text-xs px-1 " +
                  (currentStep === i
                    ? "text-primary font-semibold"
                    : completedSteps.has(i as 0)
                    ? "text-green-600"
                    : "text-muted-foreground")
                }
              >
                {i + 1}. {step}
              </button>
            ))}
          </div>
        </div>
        <div className="bg-card rounded-lg border p-6">
          {stepComponents[currentStep]}
        </div>
        <div className="flex justify-between mt-4">
          <Button
            variant="outline"
            onClick={() => setCurrentStep((currentStep - 1) as 0)}
            disabled={currentStep === 0}
          >
            Previous
          </Button>
          <Button
            onClick={() => setCurrentStep((currentStep + 1) as 0)}
            disabled={currentStep === 7}
          >
            Next
          </Button>
        </div>
      </div>
    </div>
  );
}
