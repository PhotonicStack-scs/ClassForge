"use client";

import { useState, useEffect } from "react";
import { useRouter, useParams } from "next/navigation";
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
      toast.success("School settings saved");
    } catch {
      toast.error("Failed to save school settings");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div className="max-w-2xl space-y-6">
      <div>
        <h1 className="text-2xl font-bold flex items-center gap-2">
          <Settings className="w-6 h-6" />
          Settings
        </h1>
        <p className="text-muted-foreground mt-1">Manage your school and account settings</p>
      </div>

      {/* School Settings — OrgAdmin only */}
      {user?.role === "OrgAdmin" && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <School className="w-4 h-4" />
              School Settings
            </CardTitle>
            <CardDescription>Update your school name and default language</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSaveSchool} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="schoolName">School Name</Label>
                <Input
                  id="schoolName"
                  value={schoolName}
                  onChange={(e) => setSchoolName(e.target.value)}
                  placeholder="Enter school name"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="language">Default Language</Label>
                <Select value={language} onValueChange={setLanguage}>
                  <SelectTrigger id="language">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="nb">Norwegian Bokmål</SelectItem>
                    <SelectItem value="nn">Norwegian Nynorsk</SelectItem>
                    <SelectItem value="en">English</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <Button type="submit" disabled={isSaving}>
                {isSaving ? "Saving..." : "Save Changes"}
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
              Setup Wizard
            </CardTitle>
            <CardDescription>Re-run the school setup wizard to review or update your configuration</CardDescription>
          </CardHeader>
          <CardContent>
            <Button
              variant="outline"
              onClick={() => router.push(`/${locale}/setup`)}
            >
              Open Setup Wizard
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Account Info */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <User className="w-4 h-4" />
            Account
          </CardTitle>
          <CardDescription>Your account information</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Name</span>
            <span className="text-sm font-medium">{user?.displayName}</span>
          </div>
          <Separator />
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Email</span>
            <span className="text-sm font-medium">{user?.email}</span>
          </div>
          <Separator />
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Role</span>
            <Badge variant="secondary">{user?.role}</Badge>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
