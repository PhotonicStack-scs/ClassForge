"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useTranslations } from "next-intl";
import {
  LayoutDashboard,
  GraduationCap,
  BookOpen,
  DoorOpen,
  Users,
  Clock,
  CalendarDays,
  BarChart3,
  Settings,
  ChevronLeft,
  ChevronRight,
  School,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { useUiStore } from "@/lib/stores/ui-store";
import { useAuthStore } from "@/lib/stores/auth-store";
import { Button } from "@/components/ui/button";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";

interface NavItem {
  href: string;
  icon: React.ComponentType<{ className?: string }>;
  labelKey: string;
  roles?: ("OrgAdmin" | "ScheduleManager" | "Viewer")[];
}

const NAV_ITEMS: NavItem[] = [
  { href: "/dashboard", icon: LayoutDashboard, labelKey: "dashboard" },
  { href: "/grades", icon: GraduationCap, labelKey: "grades", roles: ["OrgAdmin", "ScheduleManager"] },
  { href: "/subjects", icon: BookOpen, labelKey: "subjects", roles: ["OrgAdmin", "ScheduleManager"] },
  { href: "/rooms", icon: DoorOpen, labelKey: "rooms", roles: ["OrgAdmin", "ScheduleManager"] },
  { href: "/teachers", icon: Users, labelKey: "teachers", roles: ["OrgAdmin", "ScheduleManager"] },
  { href: "/time-structure", icon: Clock, labelKey: "timeStructure", roles: ["OrgAdmin", "ScheduleManager"] },
  { href: "/timetables", icon: CalendarDays, labelKey: "timetables" },
  { href: "/users", icon: BarChart3, labelKey: "users", roles: ["OrgAdmin"] },
  { href: "/settings", icon: Settings, labelKey: "settings" },
];

interface SidebarProps {
  locale: string;
}

export function Sidebar({ locale }: SidebarProps) {
  const t = useTranslations("common");
  const pathname = usePathname();
  const { sidebarCollapsed, toggleSidebar } = useUiStore();
  const { user, isLoading } = useAuthStore((s) => ({ user: s.user, isLoading: s.isLoading }));

  // While auth is loading or role is unknown, show all items to avoid flicker
  const filteredNav = NAV_ITEMS.filter(
    (item) => !item.roles || isLoading || !user || item.roles.includes(user.role)
  );

  return (
    <aside
      className={cn(
        "flex flex-col h-full bg-sidebar text-sidebar-foreground border-r border-sidebar-border transition-all duration-300",
        sidebarCollapsed ? "w-16" : "w-64"
      )}
    >
      {/* Logo */}
      <div className="flex items-center gap-3 px-4 py-5 border-b border-sidebar-border">
        <School className="shrink-0 text-sidebar-primary w-7 h-7" />
        {!sidebarCollapsed && (
          <span className="font-extrabold text-lg tracking-tight text-sidebar-foreground">
            ClassForge
          </span>
        )}
      </div>

      {/* Nav */}
      <nav className="flex-1 py-4 px-2 space-y-1">
        {filteredNav.map((item) => {
          const href = `/${locale}${item.href}`;
          const active = pathname.startsWith(`/${locale}${item.href}`);

          if (sidebarCollapsed) {
            return (
              <Tooltip key={item.href}>
                <TooltipTrigger asChild>
                  <Link
                    href={href}
                    className={cn(
                      "flex items-center justify-center w-10 h-10 rounded-lg transition-colors mx-auto",
                      active
                        ? "bg-sidebar-primary text-sidebar-primary-foreground"
                        : "text-sidebar-foreground/70 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
                    )}
                  >
                    <item.icon className="w-5 h-5" />
                    <span className="sr-only">{t(item.labelKey as never)}</span>
                  </Link>
                </TooltipTrigger>
                <TooltipContent side="right">
                  {t(item.labelKey as never)}
                </TooltipContent>
              </Tooltip>
            );
          }

          return (
            <Link
              key={item.href}
              href={href}
              className={cn(
                "flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors",
                active
                  ? "bg-sidebar-primary text-sidebar-primary-foreground"
                  : "text-sidebar-foreground/70 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
              )}
            >
              <item.icon className="w-5 h-5 shrink-0" />
              {t(item.labelKey as never)}
            </Link>
          );
        })}
      </nav>

      {/* Collapse button */}
      <div className="p-2 border-t border-sidebar-border">
        <Button
          variant="ghost"
          size="icon"
          onClick={toggleSidebar}
          className="w-full text-sidebar-foreground/70 hover:text-sidebar-foreground hover:bg-sidebar-accent"
          aria-label={sidebarCollapsed ? "Expand sidebar" : "Collapse sidebar"}
        >
          {sidebarCollapsed ? (
            <ChevronRight className="w-4 h-4" />
          ) : (
            <ChevronLeft className="w-4 h-4" />
          )}
        </Button>
      </div>
    </aside>
  );
}
