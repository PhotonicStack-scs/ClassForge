"use client";

import { useRouter } from "next/navigation";
import { useWizardStore } from "@/lib/stores/wizard-store";
import { useUpdateSetupProgress } from "@/lib/api/hooks/use-setup";
import { useYears } from "@/lib/api/hooks/use-years";
import { useSubjects } from "@/lib/api/hooks/use-subjects";
import { useRooms } from "@/lib/api/hooks/use-rooms";
import { useTeachers } from "@/lib/api/hooks/use-teachers";
import { useSchoolDays } from "@/lib/api/hooks/use-school-days";
import { Button } from "@/components/ui/button";
import { CheckCircle2, Circle, PartyPopper } from "lucide-react";
import { toast } from "sonner";

interface StepSummaryRow {
  label: string;
  count: number;
  unit: string;
}

function SummaryRow({ label, count, unit }: StepSummaryRow) {
  const ok = count > 0;
  return (
    <div className="flex items-center gap-3 py-2">
      {ok ? (
        <CheckCircle2 className="w-5 h-5 text-green-500 shrink-0" />
      ) : (
        <Circle className="w-5 h-5 text-muted-foreground shrink-0" />
      )}
      <span className={ok ? "text-sm" : "text-sm text-muted-foreground"}>{label}</span>
      {ok && (
        <span className="ml-auto text-sm text-muted-foreground">{count} {unit}{count !== 1 ? "s" : ""}</span>
      )}
      {!ok && <span className="ml-auto text-xs text-destructive">Not configured</span>}
    </div>
  );
}

export function Step7Review({ locale }: { locale: string }) {
  const router = useRouter();
  const { completedSteps, markStepCompleted } = useWizardStore();
  const { mutate, isPending } = useUpdateSetupProgress();

  const { data: years = [] } = useYears();
  const { data: subjects = [] } = useSubjects();
  const { data: rooms = [] } = useRooms();
  const { data: teachers = [] } = useTeachers();
  const { data: days = [] } = useSchoolDays();

  const activeDays = days.filter((d) => d.isActive);

  function handleComplete() {
    const progress: Record<string, boolean> = {};
    for (let i = 0; i <= 6; i++) {
      progress[`step${i}`] = completedSteps.has(i as 0);
    }
    mutate(
      { setupCompleted: true, setupProgress: progress },
      {
        onSuccess: () => {
          markStepCompleted(7);
          toast.success("Setup complete! Welcome to ClassForge.");
          router.push(`/${locale}/dashboard`);
        },
        onError: () => {
          toast.error("Failed to save setup progress");
        },
      }
    );
  }

  const allConfigured = years.length > 0 && subjects.length > 0 && rooms.length > 0 && teachers.length > 0 && activeDays.length > 0;

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold flex items-center gap-2">
          <PartyPopper className="w-5 h-5" />
          Review & Complete
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          Review your school configuration before completing setup.
        </p>
      </div>

      <div className="border rounded-lg divide-y">
        <SummaryRow label="Years" count={years.length} unit="year" />
        <SummaryRow label="Subjects" count={subjects.length} unit="subject" />
        <SummaryRow label="Rooms" count={rooms.length} unit="room" />
        <SummaryRow label="Teachers" count={teachers.length} unit="teacher" />
        <SummaryRow label="School days" count={activeDays.length} unit="day" />
      </div>

      {!allConfigured && (
        <p className="text-sm text-amber-600 dark:text-amber-400">
          Some sections are not configured yet. You can still complete setup and configure them later from the sidebar.
        </p>
      )}

      <Button onClick={handleComplete} disabled={isPending} size="lg">
        {isPending ? "Saving…" : "Complete Setup"}
      </Button>
    </div>
  );
}
