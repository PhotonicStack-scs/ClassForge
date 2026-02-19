export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-[#0f2830] via-[#014751] to-[#0f2830]">
      {children}
    </div>
  );
}
