"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
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
}

function StatCard({ label, count, href, icon }: StatCardProps) {
  return (
    <Card className="hover:shadow-md transition-shadow">
      <CardHeader className="pb-2 flex-row items-center justify-between space-y-0">
        <CardTitle className="text-sm font-medium text-muted-foreground">{label}</CardTitle>
        <span className="text-muted-foreground">{icon}</span>
      </CardHeader>
      <CardContent>
        <div className="text-2xl font-bold">{count ?? "—"}</div>
        <Link href={href} className="text-xs text-primary flex items-center gap-1 mt-1 hover:underline">
          Manage <ArrowRight className="w-3 h-3" />
        </Link>
      </CardContent>
    </Card>
  );
}

export default function DashboardPage() {
  const params = useParams();
  const locale = (params?.locale as string) ?? "nb";
  const user = useAuthStore((s) => s.user);

  const { data: years } = useYears();
  const { data: subjects } = useSubjects();
  const { data: rooms } = useRooms();
  const { data: teachers } = useTeachers();
  const { data: timetables } = useTimetables();

  const publishedTimetable = timetables?.find((t) => t.status === "Published");
  const latestTimetable = timetables?.[0];

  const isAdmin = user?.role === "OrgAdmin" || user?.role === "ScheduleManager";

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <p className="text-muted-foreground mt-1">
          Welcome back, {user?.displayName ?? "there"}
        </p>
      </div>

      {/* Quick stats — admin only */}
      {isAdmin && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <StatCard
            label="Years"
            count={years?.length}
            href={`/${locale}/years`}
            icon={<GraduationCap className="w-4 h-4" />}
          />
          <StatCard
            label="Subjects"
            count={subjects?.length}
            href={`/${locale}/subjects`}
            icon={<BookOpen className="w-4 h-4" />}
          />
          <StatCard
            label="Rooms"
            count={rooms?.length}
            href={`/${locale}/rooms`}
            icon={<DoorOpen className="w-4 h-4" />}
          />
          <StatCard
            label="Teachers"
            count={teachers?.length}
            href={`/${locale}/teachers`}
            icon={<Users className="w-4 h-4" />}
          />
        </div>
      )}

      {/* Timetables section */}
      <div className="grid md:grid-cols-2 gap-4">
        {publishedTimetable && (
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">Published Timetable</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              <div className="flex items-center justify-between">
                <span className="font-semibold">{publishedTimetable.name}</span>
                <Badge className="bg-green-100 text-green-700 border-green-200">Published</Badge>
              </div>
              <Link href={`/${locale}/timetables/${publishedTimetable.id}`}>
                <Button variant="outline" size="sm" className="w-full">
                  <CalendarDays className="w-4 h-4 mr-2" />
                  View Timetable
                </Button>
              </Link>
            </CardContent>
          </Card>
        )}

        {isAdmin && (
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {timetables && timetables.length > 0 ? "Latest Timetable" : "No Timetables Yet"}
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
                    <Button variant="outline" size="sm" className="w-full">View All Timetables</Button>
                  </Link>
                </>
              ) : (
                <>
                  <p className="text-sm text-muted-foreground">Generate your first timetable to get started.</p>
                  <Link href={`/${locale}/timetables`}>
                    <Button size="sm" className="w-full">
                      <CalendarDays className="w-4 h-4 mr-2" />
                      Generate Timetable
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
              <p className="font-medium">Complete your school setup</p>
              <p className="text-sm text-muted-foreground mt-0.5">
                Add years, subjects, rooms, and teachers before generating a timetable.
              </p>
            </div>
            <Link href={`/${locale}/setup`}>
              <Button size="sm">
                <Wand2 className="w-4 h-4 mr-2" />
                Setup Wizard
              </Button>
            </Link>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
