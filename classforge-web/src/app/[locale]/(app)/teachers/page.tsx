"use client";

import { useTeachers, useCreateTeacher, useDeleteTeacher } from "@/lib/api/hooks/use-teachers";
import Link from "next/link";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function TeachersPage() {
  const { data: teachers, isLoading } = useTeachers();
  const createTeacher = useCreateTeacher();
  const deleteTeacher = useDeleteTeacher();
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  function handleCreate() {
    if (!name.trim()) return;
    createTeacher.mutate({ name, email }, { onSuccess: () => { setName(""); setEmail(""); } });
  }
  if (isLoading) return <div className="p-8">Loading...</div>;
  return (
    <div className="container mx-auto p-8 max-w-3xl">
      <h1 className="text-2xl font-bold mb-6">Teachers</h1>
      <Card className="mb-6">
        <CardHeader><CardTitle>Add Teacher</CardTitle></CardHeader>
        <CardContent>
          <div className="flex gap-2">
            <Input placeholder="Name" value={name} onChange={(e) => setName(e.target.value)} />
            <Input placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
            <Button onClick={handleCreate} disabled={createTeacher.isPending}>Add</Button>
          </div>
        </CardContent>
      </Card>
      <div className="space-y-2">
        {teachers?.map((t) => (          <Card key={t.id!}>
            <CardContent className="flex items-center justify-between pt-4">
              <div>
                <span className="font-medium">{t.name}</span>
                <span className="text-muted-foreground ml-2 text-sm">{t.email}</span>
              </div>
              <div className="flex gap-2">
                <Link href={"/teachers/"+t.id}>
                  <Button variant="outline" size="sm">Edit</Button>
                </Link>
                <Button variant="destructive" size="sm" onClick={() => deleteTeacher.mutate(t.id!)}>Delete</Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
