"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useTranslations } from "next-intl";
import { useAuthStore } from "@/lib/stores/auth-store";
import { useYears } from "@/lib/api/hooks/use-years";
import { useSubjects } from "@/lib/api/hooks/use-subjects";
import { useRooms } from "@/lib/api/hooks/use-rooms";
import { useTeachers } from "@/lib/api/hooks/use-teachers";
import { useTimetables } from "@/lib/api/hooks/use-timetables";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  GraduationCap,
  BookOpen,
  DoorOpen,
  Users,
  CalendarDays,
  ArrowRight,
  Wand2,
} from "lucide-react";

interface StatCardProps {
  label: string;
  count: number | undefined;
  href: string;
  icon: React.ReactNode;
  manage: string;
}

function StatCard({ label, count, href, icon, manage }: StatCardProps) {
  return (
    <Card className="hover:shadow-md transition-shadow">
      <CardHeader className="pb-2 flex-row items-center justify-between space-y-0">
        <CardTitle className="text-sm font-medium text-muted-foreground">{label}</CardTitle>
        <span className="text-muted-foreground">{icon}</span>
      </CardHeader>
      <CardContent>
        <div className="text-2xl font-bold">{count ?? "—"}</div>
        <Link href={href} className="text-xs text-primary flex items-center gap-1 mt-1 hover:underline">
          {manage} <ArrowRight className="w-3 h-3" />
        </Link>
      </CardContent>
    </Card>
  );
}

export default function DashboardPage() {
  const params = useParams();
  const locale = (params?.locale as string) ?? "nb";
  const user = useAuthStore((s) => s.user);
  const t = useTranslations("dashboard");
  const tc = useTranslations("common");

  const { data: years } = useYears();
  const { data: subjects } = useSubjects();
  const { data: rooms } = useRooms();
  const { data: teachers } = useTeachers();
  const { data: timetables } = useTimetables();

  const publishedTimetable = timetables?.find((tt) => tt.status === "Published");
  const latestTimetable = timetables?.[0];

  const isAdmin = user?.role === "OrgAdmin" || user?.role === "ScheduleManager";

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">{tc("dashboard")}</h1>
        <p className="text-muted-foreground mt-1">
          {t("welcomeBack", { name: user?.displayName ?? "there" })}
        </p>
      </div>

      {/* Quick stats — admin only */}
      {isAdmin && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <StatCard
            label={tc("years")}
            count={years?.length}
            href={`/${locale}/years`}
            icon={<GraduationCap className="w-4 h-4" />}
            manage={t("manage")}
          />
          <StatCard
            label={tc("subjects")}
            count={subjects?.length}
            href={`/${locale}/subjects`}
            icon={<BookOpen className="w-4 h-4" />}
            manage={t("manage")}
          />
          <StatCard
            label={tc("rooms")}
            count={rooms?.length}
            href={`/${locale}/rooms`}
            icon={<DoorOpen className="w-4 h-4" />}
            manage={t("manage")}
          />
          <StatCard
            label={tc("teachers")}
            count={teachers?.length}
            href={`/${locale}/teachers`}
            icon={<Users className="w-4 h-4" />}
            manage={t("manage")}
          />
        </div>
      )}

      {/* Timetables section */}
      <div className="grid md:grid-cols-2 gap-4">
        {publishedTimetable && (
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">{t("publishedTimetable")}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              <div className="flex items-center justify-between">
                <span className="font-semibold">{publishedTimetable.name}</span>
                <Badge className="bg-green-100 text-green-700 border-green-200">{t("published")}</Badge>
              </div>
              <Link href={`/${locale}/timetables/${publishedTimetable.id}`}>
                <Button variant="outline" size="sm" className="w-full">
                  <CalendarDays className="w-4 h-4 mr-2" />
                  {t("viewTimetable")}
                </Button>
              </Link>
            </CardContent>
          </Card>
        )}

        {isAdmin && (
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {timetables && timetables.length > 0 ? t("latestTimetable") : t("noTimetablesYet")}
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              {latestTimetable ? (
                <>
                  <div className="flex items-center justify-between">
                    <span className="font-semibold">{latestTimetable.name}</span>
                    <Badge variant="secondary">{latestTimetable.status}</Badge>
                  </div>
                  <Link href={`/${locale}/timetables`}>
                    <Button variant="outline" size="sm" className="w-full">{t("viewAllTimetables")}</Button>
                  </Link>
                </>
              ) : (
                <>
                  <p className="text-sm text-muted-foreground">{t("generateFirst")}</p>
                  <Link href={`/${locale}/timetables`}>
                    <Button size="sm" className="w-full">
                      <CalendarDays className="w-4 h-4 mr-2" />
                      {t("generateTimetable")}
                    </Button>
                  </Link>
                </>
              )}
            </CardContent>
          </Card>
        )}
      </div>

      {/* Setup prompt if school data is missing */}
      {isAdmin && years?.length === 0 && (
        <Card className="border-amber-200 bg-amber-50 dark:bg-amber-950/20 dark:border-amber-800">
          <CardContent className="py-4 flex items-center justify-between gap-4">
            <div>
              <p className="font-medium">{t("completeSetup")}</p>
              <p className="text-sm text-muted-foreground mt-0.5">
                {t("setupDescription")}
              </p>
            </div>
            <Link href={`/${locale}/setup`}>
              <Button size="sm">
                <Wand2 className="w-4 h-4 mr-2" />
                {t("openSetupWizard")}
              </Button>
            </Link>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
