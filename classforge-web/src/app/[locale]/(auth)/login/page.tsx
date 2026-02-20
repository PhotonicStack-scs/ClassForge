import { getTranslations } from "next-intl/server";
import Link from "next/link";
import { School } from "lucide-react";
import { LoginForm } from "@/components/auth/login-form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { locales, localeNames } from "@/i18n/config";

interface LoginPageProps {
  params: Promise<{ locale: string }>;
  searchParams: Promise<{ from?: string }>;
}

export default async function LoginPage({ params, searchParams }: LoginPageProps) {
  const { locale } = await params;
  const { from } = await searchParams;
  const t = await getTranslations("auth");

  return (
    <div className="w-full max-w-md">
      {/* Card */}
      <div className="bg-card rounded-2xl shadow-2xl p-8">
        {/* Logo */}
        <div className="flex flex-col items-center mb-8">
          <div className="flex items-center gap-2 mb-2">
            <School className="w-8 h-8 text-primary" />
            <span className="text-2xl font-extrabold tracking-tight">ClassForge</span>
          </div>
          <p className="text-sm text-muted-foreground">{t("loginSubtitle")}</p>
        </div>

        <h1 className="text-xl font-bold mb-6 text-center">{t("loginTitle")}</h1>

        <LoginForm locale={locale} redirectTo={from} />

        <p className="mt-6 text-center text-sm text-muted-foreground">
          {t("noAccount")}{" "}
          <Link
            href={`/${locale}/register`}
            className="text-primary font-medium hover:underline"
          >
            {t("register")}
          </Link>
        </p>
      </div>

      {/* Locale switcher */}
      <div className="mt-4 flex justify-center">
        <Select defaultValue={locale}>
          <SelectTrigger className="w-40 bg-white/10 border-white/20 text-white">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {locales.map((loc) => (
              <SelectItem key={loc} value={loc}>
                {localeNames[loc]}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </div>
  );
}
