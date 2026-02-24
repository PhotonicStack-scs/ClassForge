"use client";

import { useSubjects, useCreateSubject, useUpdateSubject, useDeleteSubject } from "@/lib/api/hooks/use-subjects";
import { useRooms } from "@/lib/api/hooks/use-rooms";
import { useState, useEffect } from "react";
import { Pencil, Trash2, Check, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { SUBJECT_COLORS, getNextUnusedColor } from "@/lib/utils/color";
import { SubjectColorPicker } from "@/components/ui/subject-color-picker";

export default function SubjectsPage() {
  const { data: subjects, isLoading } = useSubjects();
  const { data: rooms = [] } = useRooms();
  const createSubject = useCreateSubject();
  const updateSubject = useUpdateSubject();
  const deleteSubject = useDeleteSubject();
  const [name, setName] = useState("");
  const [requiresSpecialRoom, setRequiresSpecialRoom] = useState(false);
  const [specialRoomId, setSpecialRoomId] = useState<string | null>(null);
  const [color, setColor] = useState<string>(SUBJECT_COLORS[0]);

  const [editingId, setEditingId] = useState<string | null>(null);
  const [editName, setEditName] = useState("");
  const [editColor, setEditColor] = useState<string>(SUBJECT_COLORS[0]);
  const [editRequiresSpecialRoom, setEditRequiresSpecialRoom] = useState(false);
  const [editSpecialRoomId, setEditSpecialRoomId] = useState<string | null>(null);

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

  function startEdit(s: { id: string; name?: string | null; color?: string | null; requiresSpecialRoom?: boolean | null; specialRoomId?: string | null }) {
    setEditingId(s.id);
    setEditName(s.name ?? "");
    setEditColor(s.color ?? SUBJECT_COLORS[0]);
    setEditRequiresSpecialRoom(s.requiresSpecialRoom ?? false);
    setEditSpecialRoomId(s.specialRoomId ?? null);
  }

  function handleSave(id: string) {
    if (!editName.trim()) return;
    updateSubject.mutate(
      { id, body: { name: editName, color: editColor, requiresSpecialRoom: editRequiresSpecialRoom, specialRoomId: editRequiresSpecialRoom ? editSpecialRoomId : null } },
      { onSuccess: () => setEditingId(null) }
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
          if (editingId === s.id) {
            return (
              <div key={s.id!} className="flex items-stretch rounded-xl border bg-card shadow-sm overflow-hidden">
                <div className="w-3 shrink-0" style={{ backgroundColor: editColor }} />
                <div className="flex flex-1 items-start gap-2 px-4 py-2">
                  <div className="flex-1 space-y-2">
                    <div className="flex gap-2">
                      <Input
                        value={editName}
                        onChange={(e) => setEditName(e.target.value)}
                        className="flex-1 h-8 text-sm"
                      />
                      <SubjectColorPicker value={editColor} onChange={setEditColor} />
                    </div>
                    <div className="flex items-center gap-2">
                      <Checkbox
                        checked={editRequiresSpecialRoom}
                        onCheckedChange={(v) => {
                          setEditRequiresSpecialRoom(!!v);
                          if (!v) setEditSpecialRoomId(null);
                        }}
                      />
                      <span className="text-sm shrink-0">Requires special room</span>
                      {editRequiresSpecialRoom && (
                        <Select value={editSpecialRoomId ?? ""} onValueChange={(v) => setEditSpecialRoomId(v || null)}>
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
                  <div className="flex gap-1 mt-0.5">
                    <Button
                      size="icon"
                      variant="ghost"
                      className="h-8 w-8 text-green-600 hover:text-green-700 hover:bg-green-50"
                      onClick={() => handleSave(s.id!)}
                      disabled={updateSubject.isPending}
                    >
                      <Check className="w-4 h-4" />
                    </Button>
                    <Button
                      size="icon"
                      variant="ghost"
                      className="h-8 w-8 text-muted-foreground hover:text-foreground"
                      onClick={() => setEditingId(null)}
                    >
                      <X className="w-4 h-4" />
                    </Button>
                  </div>
                </div>
              </div>
            );
          }
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
                <div className="flex gap-1">
                  <Button
                    variant="outline"
                    size="icon"
                    className="text-muted-foreground hover:text-foreground"
                    onClick={() => startEdit({ id: s.id!, name: s.name, color: s.color, requiresSpecialRoom: s.requiresSpecialRoom, specialRoomId: s.specialRoomId })}
                  >
                    <Pencil className="w-4 h-4" />
                  </Button>
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
            </div>
          );
        })}
      </div>
    </div>
  );
}
