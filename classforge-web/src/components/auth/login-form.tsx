"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useTranslations } from "next-intl";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useLogin } from "@/lib/api/hooks/use-auth";
import { Loader2 } from "lucide-react";

interface LoginFormValues {
  email: string;
  password: string;
}

interface LoginFormProps {
  locale: string;
  redirectTo?: string;
}

export function LoginForm({ locale, redirectTo }: LoginFormProps) {
  const t = useTranslations("auth");
  const ct = useTranslations("common");
  const router = useRouter();
  const login = useLogin();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>();

  const onSubmit = handleSubmit(async (data) => {
    try {
      await login.mutateAsync(data);
      router.push(redirectTo || `/${locale}/dashboard`);
    } catch {
      toast.error(t("loginError"));
    }
  });

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      <div className="space-y-1">
        <Label htmlFor="email">{t("email")}</Label>
        <Input
          id="email"
          type="email"
          autoComplete="email"
          {...register("email", { required: ct("required") })}
          aria-invalid={!!errors.email}
        />
        {errors.email && (
          <p className="text-sm text-destructive">{errors.email.message}</p>
        )}
      </div>

      <div className="space-y-1">
        <Label htmlFor="password">{t("password")}</Label>
        <Input
          id="password"
          type="password"
          autoComplete="current-password"
          {...register("password", { required: ct("required") })}
          aria-invalid={!!errors.password}
        />
        {errors.password && (
          <p className="text-sm text-destructive">{errors.password.message}</p>
        )}
      </div>

      <Button
        type="submit"
        className="w-full"
        disabled={login.isPending}
      >
        {login.isPending && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
        {t("login")}
      </Button>
    </form>
  );
}
