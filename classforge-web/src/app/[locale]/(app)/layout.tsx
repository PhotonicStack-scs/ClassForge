import { TooltipProvider } from "@/components/ui/tooltip";
import { Sidebar } from "@/components/layout/sidebar";
import { Header } from "@/components/layout/header";
import { MobileNav } from "@/components/layout/mobile-nav";

export default async function AppLayout({
  children,
  params,
}: {
  children: React.ReactNode;
  params: Promise<{ locale: string }>;
}) {
  const { locale } = await params;

  return (
    <TooltipProvider>
      <div className="flex h-screen overflow-hidden">
        {/* Sidebar — hidden on mobile */}
        <div className="hidden md:flex md:shrink-0">
          <Sidebar locale={locale} />
        </div>

        {/* Main area */}
        <div className="flex flex-col flex-1 min-w-0 overflow-hidden">
          <Header locale={locale} />
          <main className="flex-1 overflow-y-auto p-6 pb-20 md:pb-6">
            {children}
          </main>
        </div>
      </div>

      {/* Mobile bottom nav */}
      <MobileNav locale={locale} />
    </TooltipProvider>
  );
}
