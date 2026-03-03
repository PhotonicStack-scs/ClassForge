"use client";

import { useTranslations } from "next-intl";
import { useAuthStore } from "@/lib/stores/auth-store";
import { useUsers, useDeleteUser } from "@/lib/api/hooks/use-users";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Trash2 } from "lucide-react";
import { toast } from "sonner";

const ROLE_VARIANT: Record<string, "default" | "secondary" | "outline"> = {
  OrgAdmin: "default",
  ScheduleManager: "secondary",
  Viewer: "outline",
};

export default function UsersPage() {
  const t = useTranslations("users");
  const tc = useTranslations("common");
  const user = useAuthStore((s) => s.user);
  const { data: users = [], isLoading } = useUsers();
  const deleteMutation = useDeleteUser();
  const isOrgAdmin = user?.role === "OrgAdmin";

  const handleDelete = async (id: string | undefined, name: string | null | undefined) => {
    if (id == null) return;
    const ok = window.confirm(t("confirmDelete"));
    if (ok === false) return;
    try {
      await deleteMutation.mutateAsync(id);
      toast.success(t("deleteSuccess", { name: name ?? "User" }));
    } catch {
      toast.error(t("deleteFailed"));
    }
  };

  if (isLoading) {
    return <div className="p-8 text-muted-foreground">{tc("loading")}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t("title")}</h1>
      </div>
      {users.length === 0 ? (
        <div className="text-center py-16 text-muted-foreground">{t("noUsers")}</div>
      ) : (
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t("displayName")}</TableHead>
                <TableHead>{t("email")}</TableHead>
                <TableHead>{t("role")}</TableHead>
                {isOrgAdmin && <TableHead className="w-16">{tc("actions")}</TableHead>}
              </TableRow>
            </TableHeader>
            <TableBody>
              {users.map((u) => (
                <TableRow key={u.id}>
                  <TableCell className="font-medium">{u.displayName ?? "-"}</TableCell>
                  <TableCell className="text-muted-foreground">{u.email ?? "-"}</TableCell>
                  <TableCell>
                    <Badge variant={ROLE_VARIANT[u.role ?? ""] ?? "outline"}>
                      {u.role ? t(("roles." + u.role) as never) : "-"}
                    </Badge>
                  </TableCell>
                  {isOrgAdmin && (
                    <TableCell>
                      {u.id !== user?.id && (
                        <Button
                          variant="ghost"
                          size="icon"
                          className="text-destructive hover:text-destructive"
                          onClick={() => handleDelete(u.id, u.displayName)}
                          disabled={deleteMutation.isPending}
                        >
                          <Trash2 className="w-4 h-4" />
                        </Button>
                      )}
                    </TableCell>
                  )}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  );
}
