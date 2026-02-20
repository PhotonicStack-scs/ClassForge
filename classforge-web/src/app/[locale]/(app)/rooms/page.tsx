"use client";

import { useRooms, useCreateRoom, useDeleteRoom } from "@/lib/api/hooks/use-rooms";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function RoomsPage() {
  const { data: rooms, isLoading } = useRooms();
  const createRoom = useCreateRoom();
  const deleteRoom = useDeleteRoom();
  const [name, setName] = useState("");
  const [capacity, setCapacity] = useState(30);

  function handleCreate() {
    if (!name.trim()) return;
    createRoom.mutate({ name, capacity }, { onSuccess: () => { setName(""); setCapacity(30); } });
  }

  if (isLoading) return <div className="p-8">Loading...</div>;

  return (
    <div className="container mx-auto p-8 max-w-3xl">
      <h1 className="text-2xl font-bold mb-6">Rooms</h1>
      <Card className="mb-6">
        <CardHeader><CardTitle>Add Room</CardTitle></CardHeader>
        <CardContent>
          <div className="flex gap-2">
            <Input placeholder="Room name" value={name} onChange={(e) => setName(e.target.value)} />
            <Input type="number" placeholder="Capacity" value={capacity} onChange={(e) => setCapacity(Number(e.target.value))} className="w-28" />
            <Button onClick={handleCreate} disabled={createRoom.isPending}>Add</Button>
          </div>
        </CardContent>
      </Card>
      <div className="space-y-2">
        {rooms?.map((room) => (
          <Card key={room.id!}>
            <CardContent className="flex items-center justify-between pt-4">
              <div>
                <span className="font-medium">{room.name}</span>
                <span className="text-muted-foreground ml-2 text-sm">Capacity: {room.capacity}</span>
              </div>
              <Button variant="destructive" size="sm" onClick={() => deleteRoom.mutate(room.id!)}>Delete</Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
