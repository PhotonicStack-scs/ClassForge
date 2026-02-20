import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "ClassForge",
  description: "Smart timetable planner for Norwegian schools",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html>
      <body>{children}</body>
    </html>
  );
}
