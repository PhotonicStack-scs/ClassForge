"use client";

import { useRef } from "react";
import { useReactToPrint } from "react-to-print";
import { Button } from "@/components/ui/button";
import { Printer } from "lucide-react";

interface PrintLayoutProps {
  contentRef: React.RefObject<HTMLElement | null>;
  label?: string;
}

export function PrintButton({ contentRef, label = "Print" }: PrintLayoutProps) {
  const handlePrint = useReactToPrint({ contentRef });

  return (
    <Button variant="outline" size="sm" onClick={handlePrint}>
      <Printer className="w-4 h-4 mr-2" />
      {label}
    </Button>
  );
}
