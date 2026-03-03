"use client";

import { useRouter } from "next/navigation";
import { useTranslations } from "next-intl";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useRegister } from "@/lib/api/hooks/use-auth";
import { Loader2 } from "lucide-react";

interface RegisterFormValues {
  schoolName: string;
  displayName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

interface RegisterFormProps {
  locale: string;
}

export function RegisterForm({ locale }: RegisterFormProps) {
  const t = useTranslations("auth");
  const ct = useTranslations("common");
  const router = useRouter();
  const register_ = useRegister();

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<RegisterFormValues>();

  const password = watch("password");

  const onSubmit = handleSubmit(async (data) => {
    try {
      await register_.mutateAsync({
        schoolName: data.schoolName,
        displayName: data.displayName,
        email: data.email,
        password: data.password,
      });
      router.push(`/${locale}/setup`);
    } catch {
      toast.error(t("registerError"));
    }
  });

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      <div className="space-y-1">
        <Label htmlFor="schoolName">{t("schoolName")}</Label>
        <Input
          id="schoolName"
          {...register("schoolName", { required: ct("required") })}
          aria-invalid={!!errors.schoolName}
        />
        {errors.schoolName && (
          <p className="text-sm text-destructive">{errors.schoolName.message}</p>
        )}
      </div>

      <div className="space-y-1">
        <Label htmlFor="displayName">{t("adminName")}</Label>
        <Input
          id="displayName"
          {...register("displayName", { required: ct("required") })}
          aria-invalid={!!errors.displayName}
        />
        {errors.displayName && (
          <p className="text-sm text-destructive">{errors.displayName.message}</p>
        )}
      </div>

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
          autoComplete="new-password"
          {...register("password", { required: ct("required"), minLength: { value: 8, message: t("passwordMinLength") } })}
          aria-invalid={!!errors.password}
        />
        {errors.password && (
          <p className="text-sm text-destructive">{errors.password.message}</p>
        )}
      </div>

      <div className="space-y-1">
        <Label htmlFor="confirmPassword">{t("confirmPassword")}</Label>
        <Input
          id="confirmPassword"
          type="password"
          autoComplete="new-password"
          {...register("confirmPassword", {
            required: ct("required"),
            validate: (v) => v === password || t("passwordMismatch"),
          })}
          aria-invalid={!!errors.confirmPassword}
        />
        {errors.confirmPassword && (
          <p className="text-sm text-destructive">{errors.confirmPassword.message}</p>
        )}
      </div>

      <Button
        type="submit"
        className="w-full"
        disabled={register_.isPending}
      >
        {register_.isPending && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
        {t("register")}
      </Button>
    </form>
  );
}
