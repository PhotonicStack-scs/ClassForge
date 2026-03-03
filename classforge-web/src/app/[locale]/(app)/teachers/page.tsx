"use client";

import { useTranslations } from "next-intl";
import { useTeachers, useCreateTeacher, useDeleteTeacher } from "@/lib/api/hooks/use-teachers";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function TeachersPage() {
  const t = useTranslations("teachers");
  const tc = useTranslations("common");
  const params = useParams();
  const locale = (params?.locale as string) ?? "nb";
  const { data: teachers, isLoading } = useTeachers();
  const createTeacher = useCreateTeacher();
  const deleteTeacher = useDeleteTeacher();
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  function handleCreate() {
    if (!name.trim()) return;
    createTeacher.mutate({ name, email }, { onSuccess: () => { setName(""); setEmail(""); } });
  }
  if (isLoading) return <div className="p-8">{tc("loading")}</div>;
  return (
    <div className="container mx-auto p-8 max-w-3xl">
      <h1 className="text-2xl font-bold mb-6">{t("title")}</h1>
      <Card className="mb-6">
        <CardHeader><CardTitle>{t("addTeacher")}</CardTitle></CardHeader>
        <CardContent>
          <div className="flex gap-2">
            <Input placeholder={t("fullName")} value={name} onChange={(e) => setName(e.target.value)} />
            <Input placeholder={t("email")} value={email} onChange={(e) => setEmail(e.target.value)} />
            <Button onClick={handleCreate} disabled={createTeacher.isPending}>{tc("add")}</Button>
          </div>
        </CardContent>
      </Card>
      <div className="space-y-2">
        {teachers?.map((teacher) => (
          <Card key={teacher.id!}>
            <CardContent className="flex items-center justify-between pt-4">
              <div>
                <span className="font-medium">{teacher.name}</span>
                <span className="text-muted-foreground ml-2 text-sm">{teacher.email}</span>
              </div>
              <div className="flex gap-2">
                <Link href={`/${locale}/teachers/${teacher.id}`}>
                  <Button variant="outline" size="sm">{tc("edit")}</Button>
                </Link>
                <Button variant="destructive" size="sm" onClick={() => deleteTeacher.mutate(teacher.id!)}>{tc("delete")}</Button>
              </div>
            </CardContent>
          </Card>
        ))}
        {teachers?.length === 0 && (
          <p className="text-sm text-muted-foreground py-4 text-center">{t("noTeachers")}</p>
        )}
      </div>
    </div>
  );
}
