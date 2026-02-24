import { WizardShell } from "@/components/setup/wizard-shell";

export default async function SetupPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale } = await params;
  return <WizardShell locale={locale} />;
}
