"use client";

import { useEffect } from "react";
import { toast } from "sonner";
import { useTranslations } from "next-intl";

export function ForbiddenToastListener() {
  const t = useTranslations("common");

  useEffect(() => {
    const handler = () => {
      toast.error(t("insufficientPermissions"));
    };
    window.addEventListener("classforge:forbidden", handler);
    return () => window.removeEventListener("classforge:forbidden", handler);
  }, [t]);

  return null;
}
