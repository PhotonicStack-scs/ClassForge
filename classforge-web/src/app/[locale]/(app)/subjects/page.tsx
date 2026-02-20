"use client";

import { useSubjects, useCreateSubject, useDeleteSubject } from "@/lib/api/hooks/use-subjects";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";

export default function SubjectsPage() {
  const { data: subjects, isLoading } = useSubjects();
  const createSubject = useCreateSubject();
  const deleteSubject = useDeleteSubject();
  const [name, setName] = useState("");
  const [requiresSpecialRoom, setRequiresSpecialRoom] = useState(false);
  const [color, setColor] = useState("#3b82f6");

  function handleCreate() {
    if (!name.trim()) return;
    createSubject.mutate(
      { name, requiresSpecialRoom, color },
      { onSuccess: () => { setName(""); setRequiresSpecialRoom(false); setColor("#3b82f6"); } }
    );
  }

  if (isLoading) return <div className="p-8">Loading...</div>;

  return (
    <div className="container mx-auto p-8 max-w-3xl">
      <h1 className="text-2xl font-bold mb-6">Subjects</h1>
      <Card className="mb-6">
        <CardHeader><CardTitle>Add Subject</CardTitle></CardHeader>
        <CardContent>
          <div className="flex gap-2 mb-2">
            <Input placeholder="Subject name" value={name} onChange={(e) => setName(e.target.value)} />
            <input type="color" value={color} onChange={(e) => setColor(e.target.value)} className="h-10 w-10 rounded border" />
            <Button onClick={handleCreate} disabled={createSubject.isPending}>Add</Button>
          </div>
          <div className="flex items-center gap-2">
            <Checkbox id="specialRoom" checked={requiresSpecialRoom} onCheckedChange={(v) => setRequiresSpecialRoom(!!v)} />
            <label htmlFor="specialRoom" className="text-sm">Requires special room</label>
          </div>
        </CardContent>
      </Card>
      <div className="space-y-2">
        {subjects?.map((s) => (
          <Card key={s.id!}>
            <CardContent className="flex items-center justify-between pt-4">
              <div className="flex items-center gap-3">
                <div className="w-4 h-4 rounded-full" style={{ backgroundColor: s.color ?? undefined }} />
                <span className="font-medium">{s.name}</span>
              </div>
              <Button variant="destructive" size="sm" onClick={() => deleteSubject.mutate(s.id!)}>Delete</Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
