"use client";

import { useSubjects, useCreateSubject, useDeleteSubject } from "@/lib/api/hooks/use-subjects";
import { useRooms } from "@/lib/api/hooks/use-rooms";
import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Trash2 } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { SUBJECT_COLORS, getNextUnusedColor } from "@/lib/utils/color";
import { SubjectColorPicker } from "@/components/ui/subject-color-picker";

export default function SubjectsPage() {
  const { data: subjects, isLoading } = useSubjects();
  const { data: rooms = [] } = useRooms();
  const createSubject = useCreateSubject();
  const deleteSubject = useDeleteSubject();
  const [name, setName] = useState("");
  const [requiresSpecialRoom, setRequiresSpecialRoom] = useState(false);
  const [specialRoomId, setSpecialRoomId] = useState<string | null>(null);
  const [color, setColor] = useState<string>(SUBJECT_COLORS[0]);

  useEffect(() => {
    if (subjects) setColor(getNextUnusedColor(subjects.map((s) => s.color ?? "")));
  }, [subjects]);

  function handleCreate() {
    if (!name.trim()) return;
    createSubject.mutate(
      { name, requiresSpecialRoom, specialRoomId: requiresSpecialRoom ? specialRoomId : null, color },
      {
        onSuccess: () => {
          setName("");
          setRequiresSpecialRoom(false);
          setSpecialRoomId(null);
        },
      }
    );
  }

  if (isLoading) return <div className="p-8">Loading...</div>;

  return (
    <div className="container mx-auto p-8 max-w-3xl">
      <h1 className="text-2xl font-bold mb-6">Subjects</h1>
      <Card className="mb-6">
        <CardHeader><CardTitle>Add Subject</CardTitle></CardHeader>
        <CardContent>
          <div className="flex gap-2 items-start">
            <div className="flex-1 space-y-2">
              <div className="flex gap-2">
                <Input placeholder="Subject name" value={name} onChange={(e) => setName(e.target.value)} className="flex-1" />
                <SubjectColorPicker value={color} onChange={setColor} />
              </div>
              <div className="flex items-center gap-2">
                <Checkbox
                  id="specialRoom"
                  checked={requiresSpecialRoom}
                  onCheckedChange={(v) => {
                    setRequiresSpecialRoom(!!v);
                    if (!v) setSpecialRoomId(null);
                  }}
                />
                <label htmlFor="specialRoom" className="text-sm shrink-0">Requires special room</label>
                {requiresSpecialRoom && (
                  <Select value={specialRoomId ?? ""} onValueChange={(v) => setSpecialRoomId(v || null)}>
                    <SelectTrigger className="flex-1">
                      <SelectValue placeholder="Select a room" />
                    </SelectTrigger>
                    <SelectContent>
                      {rooms.map((r) => (
                        <SelectItem key={r.id} value={r.id!}>{r.name}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              </div>
            </div>
            <Button onClick={handleCreate} disabled={createSubject.isPending}>Add</Button>
          </div>
        </CardContent>
      </Card>
      <div className="space-y-2">
        {subjects?.map((s) => {
          const room = s.specialRoomId ? rooms.find((r) => r.id === s.specialRoomId) : null;
          return (
            <div key={s.id!} className="flex items-stretch rounded-xl border bg-card shadow-sm overflow-hidden">
              <div className="w-3 shrink-0" style={{ backgroundColor: s.color ?? undefined }} />
              <div className="flex flex-1 items-center justify-between px-4 py-3">
                <div>
                  <span className="font-medium">{s.name}</span>
                  {s.requiresSpecialRoom && (
                    <p className="text-xs text-muted-foreground">
                      Requires special room{room ? `: ${room.name}` : ""}
                    </p>
                  )}
                </div>
                <Button
                  variant="outline"
                  size="icon"
                  className="border-destructive/30 text-destructive/60 hover:bg-destructive/10 hover:text-destructive hover:border-destructive/50"
                  onClick={() => deleteSubject.mutate(s.id!)}
                >
                  <Trash2 className="w-4 h-4" />
                </Button>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
