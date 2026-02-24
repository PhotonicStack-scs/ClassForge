import { NextIntlClientProvider } from "next-intl";
import { getMessages } from "next-intl/server";
import { notFound } from "next/navigation";
import { QueryProvider } from "@/components/providers/query-provider";
import { Toaster } from "@/components/ui/sonner";
import { ForbiddenToastListener } from "@/components/providers/forbidden-toast-listener";
import { AuthInitializer } from "@/components/providers/auth-initializer";
import { locales } from "@/i18n/config";
import type { Locale } from "@/i18n/config";

export default async function LocaleLayout({
  children,
  params,
}: {
  children: React.ReactNode;
  params: Promise<{ locale: string }>;
}) {
  const { locale } = await params;

  if (!locales.includes(locale as Locale)) {
    notFound();
  }

  const messages = await getMessages();

  return (
    <NextIntlClientProvider messages={messages}>
      <QueryProvider>
        <AuthInitializer />
        <ForbiddenToastListener />
        {children}
        <Toaster position="top-right" />
      </QueryProvider>
    </NextIntlClientProvider>
  );
}
