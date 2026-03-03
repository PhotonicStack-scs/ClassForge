"use client";

import { useTranslations } from "next-intl";
import { useRooms, useCreateRoom, useUpdateRoom, useDeleteRoom } from "@/lib/api/hooks/use-rooms";
import { useState } from "react";
import { Pencil, Trash2, Check, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function RoomsPage() {
  const t = useTranslations("rooms");
  const tc = useTranslations("common");
  const { data: rooms, isLoading } = useRooms();
  const createRoom = useCreateRoom();
  const updateRoom = useUpdateRoom();
  const deleteRoom = useDeleteRoom();
  const [name, setName] = useState("");
  const [capacity, setCapacity] = useState(30);

  const [editingId, setEditingId] = useState<string | null>(null);
  const [editName, setEditName] = useState("");
  const [editCapacity, setEditCapacity] = useState(30);

  function handleCreate() {
    if (!name.trim()) return;
    createRoom.mutate({ name, capacity }, { onSuccess: () => { setName(""); setCapacity(30); } });
  }

  function startEdit(room: { id: string; name?: string | null; capacity?: number | null }) {
    setEditingId(room.id);
    setEditName(room.name ?? "");
    setEditCapacity(room.capacity ?? 30);
  }

  function handleSave(id: string) {
    if (!editName.trim()) return;
    updateRoom.mutate(
      { id, body: { name: editName, capacity: editCapacity } },
      { onSuccess: () => setEditingId(null) }
    );
  }

  if (isLoading) return <div className="p-8">{tc("loading")}</div>;

  return (
    <div className="container mx-auto p-8 max-w-3xl">
      <h1 className="text-2xl font-bold mb-6">{t("title")}</h1>
      <Card className="mb-6">
        <CardHeader><CardTitle>{t("addRoom")}</CardTitle></CardHeader>
        <CardContent>
          <div className="flex gap-3 items-end">
            <div className="flex-1 space-y-1">
              <Label htmlFor="roomName">{t("roomName")}</Label>
              <Input id="roomName" placeholder={t("roomNamePlaceholder")} value={name} onChange={(e) => setName(e.target.value)} />
            </div>
            <div className="w-28 space-y-1">
              <Label htmlFor="capacity">{t("capacity")}</Label>
              <Input id="capacity" type="number" value={capacity} onChange={(e) => setCapacity(Number(e.target.value))} />
            </div>
            <Button onClick={handleCreate} disabled={createRoom.isPending}>{tc("add")}</Button>
          </div>
        </CardContent>
      </Card>
      <div className="space-y-2">
        {rooms?.map((room) =>
          editingId === room.id ? (
            <div key={room.id!} className="flex items-center gap-2 px-4 py-2 rounded-xl border bg-card shadow-sm">
              <Input
                value={editName}
                onChange={(e) => setEditName(e.target.value)}
                className="flex-1 h-8 text-sm"
              />
              <Input
                type="number"
                value={editCapacity}
                onChange={(e) => setEditCapacity(Number(e.target.value))}
                className="w-24 h-8 text-sm"
              />
              <Button
                size="icon"
                variant="ghost"
                className="h-8 w-8 text-green-600 hover:text-green-700 hover:bg-green-50"
                onClick={() => handleSave(room.id!)}
                disabled={updateRoom.isPending}
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
          ) : (
            <div key={room.id!} className="flex items-center justify-between px-4 py-3 rounded-xl border bg-card shadow-sm">
              <div>
                <span className="font-medium">{room.name}</span>
                <p className="text-xs text-muted-foreground">{t("capacityPrefix")}{room.capacity}</p>
              </div>
              <div className="flex gap-1">
                <Button
                  variant="outline"
                  size="icon"
                  className="text-muted-foreground hover:text-foreground"
                  onClick={() => startEdit({ id: room.id!, name: room.name, capacity: room.capacity })}
                >
                  <Pencil className="w-4 h-4" />
                </Button>
                <Button
                  variant="outline"
                  size="icon"
                  className="border-destructive/30 text-destructive/60 hover:bg-destructive/10 hover:text-destructive hover:border-destructive/50"
                  onClick={() => deleteRoom.mutate(room.id!)}
                >
                  <Trash2 className="w-4 h-4" />
                </Button>
              </div>
            </div>
          )
        )}
        {rooms?.length === 0 && (
          <p className="text-sm text-muted-foreground py-4 text-center">{t("noRooms")}</p>
        )}
      </div>
    </div>
  );
}
