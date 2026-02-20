import { getTranslations } from "next-intl/server";
import Link from "next/link";
import { School } from "lucide-react";
import { RegisterForm } from "@/components/auth/register-form";

interface RegisterPageProps {
  params: Promise<{ locale: string }>;
}

export default async function RegisterPage({ params }: RegisterPageProps) {
  const { locale } = await params;
  const t = await getTranslations("auth");

  return (
    <div className="w-full max-w-md">
      <div className="bg-card rounded-2xl shadow-2xl p-8">
        {/* Logo */}
        <div className="flex flex-col items-center mb-8">
          <div className="flex items-center gap-2 mb-2">
            <School className="w-8 h-8 text-primary" />
            <span className="text-2xl font-extrabold tracking-tight">ClassForge</span>
          </div>
          <p className="text-sm text-muted-foreground">{t("registerSubtitle")}</p>
        </div>

        <h1 className="text-xl font-bold mb-6 text-center">{t("registerTitle")}</h1>

        <RegisterForm locale={locale} />

        <p className="mt-6 text-center text-sm text-muted-foreground">
          {t("hasAccount")}{" "}
          <Link
            href={`/${locale}/login`}
            className="text-primary font-medium hover:underline"
          >
            {t("login")}
          </Link>
        </p>
      </div>
    </div>
  );
}
