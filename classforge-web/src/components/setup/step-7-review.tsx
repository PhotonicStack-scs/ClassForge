"use client";

import { useTranslations } from "next-intl";
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

function SummaryRow({ label, count, notConfigured }: { label: string; count: number; notConfigured: string }) {
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
        <span className="ml-auto text-sm text-muted-foreground">{count}</span>
      )}
      {!ok && <span className="ml-auto text-xs text-destructive">{notConfigured}</span>}
    </div>
  );
}

export function Step7Review({ locale }: { locale: string }) {
  const t = useTranslations("setup");
  const tc = useTranslations("common");
  const router = useRouter();
  const { completedSteps, markStepCompleted } = useWizardStore();
  const { mutate, isPending } = useUpdateSetupProgress();

  const { data: years = [] } = useYears();
  const { data: subjects = [] } = useSubjects();
  const { data: rooms = [] } = useRooms();
  const { data: teachers = [] } = useTeachers();
  const { data: days = [] } = useSchoolDays();

  const activeDays = days.filter((d) => d.isActive);
  const notConfigured = t("notConfigured");

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
          toast.success(t("setupComplete"));
          router.push(`/${locale}/dashboard`);
        },
        onError: () => {
          toast.error(t("saveFailed"));
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
          {t("reviewTitle")}
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          {t("step7Description")}
        </p>
      </div>

      <div className="border rounded-lg divide-y">
        <SummaryRow label={tc("years")} count={years.length} notConfigured={notConfigured} />
        <SummaryRow label={tc("subjects")} count={subjects.length} notConfigured={notConfigured} />
        <SummaryRow label={tc("rooms")} count={rooms.length} notConfigured={notConfigured} />
        <SummaryRow label={tc("teachers")} count={teachers.length} notConfigured={notConfigured} />
        <SummaryRow label={t("schoolDaysLabel")} count={activeDays.length} notConfigured={notConfigured} />
      </div>

      {!allConfigured && (
        <p className="text-sm text-amber-600 dark:text-amber-400">
          {t("incompleteSetupNote")}
        </p>
      )}

      <Button onClick={handleComplete} disabled={isPending} size="lg">
        {isPending ? t("saving") : t("completeSetup")}
      </Button>
    </div>
  );
}
