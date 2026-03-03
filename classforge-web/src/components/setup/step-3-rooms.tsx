"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { useWizardStore } from "@/lib/stores/wizard-store";
import { useRooms, useCreateRoom, useDeleteRoom } from "@/lib/api/hooks/use-rooms";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Trash2, Plus, DoorOpen } from "lucide-react";
import { toast } from "sonner";

export function Step3Rooms() {
  const t = useTranslations("setup");
  const tr = useTranslations("rooms");
  const tc = useTranslations("common");
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  const { data: rooms = [], isLoading } = useRooms();
  const createRoom = useCreateRoom();
  const deleteRoom = useDeleteRoom();

  const [name, setName] = useState("");
  const [capacity, setCapacity] = useState("30");

  async function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;
    try {
      await createRoom.mutateAsync({ name: name.trim(), capacity: parseInt(capacity) || 30 });
      setName("");
    } catch {
      toast.error(tc("error"));
    }
  }

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold flex items-center gap-2">
          <DoorOpen className="w-5 h-5" />
          {tr("title")}
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          {t("step3Description")}
        </p>
      </div>

      <form onSubmit={handleAdd} className="flex gap-2">
        <Input
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder={tr("roomNamePlaceholder")}
          className="max-w-xs"
        />
        <Input
          value={capacity}
          onChange={(e) => setCapacity(e.target.value)}
          placeholder={tr("capacity")}
          type="number"
          min="1"
          className="w-24"
        />
        <Button type="submit" size="sm" disabled={createRoom.isPending || !name.trim()}>
          <Plus className="w-4 h-4 mr-1" />
          {tc("add")}
        </Button>
      </form>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">{tc("loading")}</p>
      ) : rooms.length === 0 ? (
        <p className="text-sm text-muted-foreground">{tr("noRooms")}</p>
      ) : (
        <ul className="space-y-1">
          {rooms.map((r) => (
            <li key={r.id} className="flex items-center justify-between py-1.5 px-3 rounded-md bg-muted/50">
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium">{r.name}</span>
                <span className="text-xs text-muted-foreground">{tr("capacityPrefix")}{r.capacity}</span>
              </div>
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7 text-muted-foreground hover:text-destructive"
                onClick={() => deleteRoom.mutate(r.id!)}
              >
                <Trash2 className="w-3.5 h-3.5" />
              </Button>
            </li>
          ))}
        </ul>
      )}

      <div className="pt-2 space-y-1">
        <Button onClick={() => { markStepCompleted(2); setCurrentStep(3); }} disabled={rooms.length === 0}>
          {tc("continue")}{rooms.length > 0 && <Badge variant="secondary" className="ml-2">{rooms.length}</Badge>}
        </Button>
        {rooms.length === 0 && (
          <p className="text-xs text-muted-foreground">{t("addRoomFirst")}</p>
        )}
      </div>
    </div>
  );
}
