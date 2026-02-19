"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useTranslations } from "next-intl";
import { CalendarDays, LayoutDashboard, Settings, UserCircle } from "lucide-react";
import { cn } from "@/lib/utils";

interface MobileNavProps {
  locale: string;
}

export function MobileNav({ locale }: MobileNavProps) {
  const t = useTranslations("common");
  const pathname = usePathname();

  const items = [
    { href: "/dashboard", icon: LayoutDashboard, labelKey: "dashboard" },
    { href: "/timetables", icon: CalendarDays, labelKey: "timetables" },
    { href: "/my-schedule", icon: UserCircle, labelKey: "mySchedule" },
    { href: "/settings", icon: Settings, labelKey: "settings" },
  ];

  return (
    <nav className="fixed bottom-0 left-0 right-0 z-50 flex items-center justify-around bg-card border-t border-border h-16 md:hidden">
      {items.map((item) => {
        const href = `/${locale}${item.href}`;
        const active = pathname.startsWith(href);
        return (
          <Link
            key={item.href}
            href={href}
            className={cn(
              "flex flex-col items-center gap-1 text-xs font-medium px-3 py-2 rounded-lg transition-colors",
              active
                ? "text-primary"
                : "text-muted-foreground hover:text-foreground"
            )}
          >
            <item.icon className="w-5 h-5" />
            <span>{t(item.labelKey as never)}</span>
          </Link>
        );
      })}
    </nav>
  );
}
