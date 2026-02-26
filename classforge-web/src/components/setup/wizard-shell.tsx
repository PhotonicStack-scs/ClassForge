"use client";

import { useWizardStore } from "@/lib/stores/wizard-store";
import { Step0Template } from "./step-0-template";
import { Step1Years } from "./step-1-years";
import { Step2Subjects } from "./step-2-subjects";
import { Step3Rooms } from "./step-3-rooms";
import { Step4TimeStructure } from "./step-4-time-structure";
import { Step5Teachers } from "./step-5-teachers";
import { Step6LessonConfig } from "./step-6-lesson-config";
import { Step7Review } from "./step-7-review";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { cn } from "@/lib/utils";
import { Check } from "lucide-react";

const STEPS = [
  "Template",
  "Years",
  "Rooms",
  "Subjects",
  "Time",
  "Teachers",
  "Curriculum",
  "Review",
];

export function WizardShell({ locale }: { locale: string }) {
  const { currentStep, setCurrentStep, completedSteps } = useWizardStore();

  const stepComponents = [
    <Step0Template key={0} />,
    <Step1Years key={1} />,
    <Step3Rooms key={2} />,
    <Step2Subjects key={3} />,
    <Step4TimeStructure key={4} />,
    <Step5Teachers key={5} />,
    <Step6LessonConfig key={6} />,
    <Step7Review key={7} locale={locale} />,
  ];

  const progress = (completedSteps.size / STEPS.length) * 100;

  return (
    <div className="min-h-screen bg-background">
      <div className="container mx-auto px-4 py-8 max-w-3xl">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-extrabold mb-1">School Setup</h1>
          <p className="text-muted-foreground text-sm">Complete each step to configure your school for timetable generation</p>
          <Progress value={progress} className="h-1.5 mt-4" />
        </div>

        {/* Step tabs */}
        <div className="flex gap-1 mb-6 overflow-x-auto pb-1">
          {STEPS.map((step, i) => {
            const isDone = completedSteps.has(i as 0);
            const isCurrent = currentStep === i;
            return (
              <button
                key={i}
                onClick={() => setCurrentStep(i as 0)}
                className={cn(
                  "flex items-center gap-1 px-3 py-1.5 rounded-full text-xs font-medium whitespace-nowrap transition-colors",
                  isCurrent
                    ? "bg-primary text-primary-foreground"
                    : isDone
                    ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400"
                    : "bg-muted text-muted-foreground hover:bg-muted/80"
                )}
              >
                {isDone && !isCurrent && <Check className="w-3 h-3" />}
                {i + 1}. {step}
              </button>
            );
          })}
        </div>

        {/* Step content */}
        <div className="bg-card rounded-xl border shadow-sm p-6">
          {stepComponents[currentStep]}
        </div>

        {/* Navigation */}
        <div className="flex justify-between mt-4">
          <Button
            variant="outline"
            onClick={() => setCurrentStep((currentStep - 1) as 0)}
            disabled={currentStep === 0}
          >
            Previous
          </Button>
          <Button
            variant="ghost"
            onClick={() => setCurrentStep((currentStep + 1) as 0)}
            disabled={currentStep === 7}
          >
            Skip
          </Button>
        </div>
      </div>
    </div>
  );
}
