"use client";

import { useState } from "react";
import { useWizardStore } from "@/lib/stores/wizard-store";
import { useRooms, useCreateRoom, useDeleteRoom } from "@/lib/api/hooks/use-rooms";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Trash2, Plus, DoorOpen } from "lucide-react";
import { toast } from "sonner";

export function Step3Rooms() {
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
      toast.error("Failed to add room");
    }
  }

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold flex items-center gap-2">
          <DoorOpen className="w-5 h-5" />
          Rooms
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          Add the classrooms and special rooms at your school.
        </p>
      </div>

      <form onSubmit={handleAdd} className="flex gap-2">
        <Input
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Room name (e.g. Room 101)"
          className="max-w-xs"
        />
        <Input
          value={capacity}
          onChange={(e) => setCapacity(e.target.value)}
          placeholder="Capacity"
          type="number"
          min="1"
          className="w-24"
        />
        <Button type="submit" size="sm" disabled={createRoom.isPending || !name.trim()}>
          <Plus className="w-4 h-4 mr-1" />
          Add
        </Button>
      </form>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">Loading…</p>
      ) : rooms.length === 0 ? (
        <p className="text-sm text-muted-foreground">No rooms added yet.</p>
      ) : (
        <ul className="space-y-1">
          {rooms.map((r) => (
            <li key={r.id} className="flex items-center justify-between py-1.5 px-3 rounded-md bg-muted/50">
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium">{r.name}</span>
                <span className="text-xs text-muted-foreground">cap. {r.capacity}</span>
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
        <Button onClick={() => { markStepCompleted(3); setCurrentStep(4); }} disabled={rooms.length === 0}>
          Continue{rooms.length > 0 && <Badge variant="secondary" className="ml-2">{rooms.length} room{rooms.length !== 1 ? "s" : ""}</Badge>}
        </Button>
        {rooms.length === 0 && (
          <p className="text-xs text-muted-foreground">Add at least one room to continue</p>
        )}
      </div>
    </div>
  );
}
