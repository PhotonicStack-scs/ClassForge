"use client";

import { useState, useEffect } from "react";
import { useRouter, useParams } from "next/navigation";
import { useTranslations } from "next-intl";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Separator } from "@/components/ui/separator";
import { Badge } from "@/components/ui/badge";
import { useAuthStore } from "@/lib/stores/auth-store";
import { apiClient } from "@/lib/api/client";
import { toast } from "sonner";
import { Settings, School, User, Wand2 } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import type { components } from "@/lib/api/schema";

type TenantResponse = components["schemas"]["TenantResponse"];

function useSchool() {
  return useQuery<TenantResponse>({
    queryKey: ["school"],
    queryFn: async () => {
      const { data, error } = await apiClient.GET("/api/v1/school");
      if (error) throw error;
      return data as TenantResponse;
    },
  });
}

export default function SettingsPage() {
  const router = useRouter();
  const params = useParams();
  const locale = (params?.locale as string) ?? "nb";
  const user = useAuthStore((s) => s.user);
  const t = useTranslations("settings");
  const tc = useTranslations("common");

  const { data: school } = useSchool();

  const [schoolName, setSchoolName] = useState("");
  const [language, setLanguage] = useState("nb");
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (school) {
      setSchoolName(school.name ?? "");
      setLanguage(school.defaultLanguage ?? "nb");
    }
  }, [school]);

  async function handleSaveSchool(e: React.FormEvent) {
    e.preventDefault();
    if (!schoolName.trim()) return;
    setIsSaving(true);
    try {
      const { error } = await apiClient.PUT("/api/v1/school", {
        body: { name: schoolName, defaultLanguage: language },
      });
      if (error) throw error;
      toast.success(t("settingsSaved"));
    } catch {
      toast.error(tc("error"));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div className="max-w-2xl space-y-6">
      <div>
        <h1 className="text-2xl font-bold flex items-center gap-2">
          <Settings className="w-6 h-6" />
          {t("title")}
        </h1>
        <p className="text-muted-foreground mt-1">{t("subtitle")}</p>
      </div>

      {/* School Settings — OrgAdmin only */}
      {user?.role === "OrgAdmin" && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <School className="w-4 h-4" />
              {t("schoolSettings")}
            </CardTitle>
            <CardDescription>{t("schoolSettingsDesc")}</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSaveSchool} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="schoolName">{t("schoolName")}</Label>
                <Input
                  id="schoolName"
                  value={schoolName}
                  onChange={(e) => setSchoolName(e.target.value)}
                  placeholder={t("enterSchoolName")}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="language">{t("defaultLanguage")}</Label>
                <Select value={language} onValueChange={setLanguage}>
                  <SelectTrigger id="language">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="nb">{t("bokmaal")}</SelectItem>
                    <SelectItem value="nn">{t("nynorsk")}</SelectItem>
                    <SelectItem value="en">{t("english")}</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <Button type="submit" disabled={isSaving}>
                {isSaving ? t("saving") : t("saveChanges")}
              </Button>
            </form>
          </CardContent>
        </Card>
      )}

      {/* Setup Wizard — OrgAdmin only */}
      {user?.role === "OrgAdmin" && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <Wand2 className="w-4 h-4" />
              {t("setupWizardSection")}
            </CardTitle>
            <CardDescription>{t("setupWizardDesc")}</CardDescription>
          </CardHeader>
          <CardContent>
            <Button
              variant="outline"
              onClick={() => router.push(`/${locale}/setup`)}
            >
              {t("openSetupWizard")}
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Account Info */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <User className="w-4 h-4" />
            {t("account")}
          </CardTitle>
          <CardDescription>{t("accountDesc")}</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">{tc("name")}</span>
            <span className="text-sm font-medium">{user?.displayName}</span>
          </div>
          <Separator />
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Email</span>
            <span className="text-sm font-medium">{user?.email}</span>
          </div>
          <Separator />
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">{t("role")}</span>
            <Badge variant="secondary">{user?.role}</Badge>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
