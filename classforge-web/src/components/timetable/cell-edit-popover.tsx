"use client";

import { useState } from "react";
import { useTranslations } from "next-intl";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";
import { useUpdateTimetableEntry } from "@/lib/api/hooks/use-timetables";
import type { components } from "@/lib/api/schema";

type TimetableViewEntry = components["schemas"]["TimetableViewEntry"];

interface Option { id: string; name: string; }

interface CellEditPopoverProps {
  timetableId: string;
  entry: TimetableViewEntry | null;
  children: React.ReactNode;
  subjects?: Option[];
  teachers?: Option[];
  rooms?: Option[];
  onSuccess?: () => void;
}

export function CellEditPopover({
  timetableId,
  entry,
  children,
  subjects = [],
  teachers = [],
  rooms = [],
  onSuccess,
}: CellEditPopoverProps) {
  const t = useTranslations("timetable");
  const [open, setOpen] = useState(false);
  const [subjectId, setSubjectId] = useState("");
  const [teacherId, setTeacherId] = useState("");
  const [roomId, setRoomId] = useState("");
  const updateMutation = useUpdateTimetableEntry();

  if (!entry?.entryId) return <>{children}</>;

  const handleSave = async () => {
    try {
      await updateMutation.mutateAsync({
        timetableId,
        entryId: entry.entryId!,
        body: {
          subjectId: subjectId || undefined,
          teacherId: teacherId || undefined,
          roomId: roomId || null,
        },
      });
      toast.success("Entry updated");
      setOpen(false);
      onSuccess?.();
    } catch (err: unknown) {
      const status = (err as { status?: number }).status;
      if (status === 409) {
        toast.error(t("conflictError", { message: "Schedule conflict detected" }));
      } else {
        toast.error("Failed to update entry");
      }
    }
  };

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>{children}</PopoverTrigger>
      <PopoverContent className="w-72 space-y-3">
        <h3 className="font-semibold text-sm">{t("editEntry")}</h3>
        <div className="space-y-1">
          <Label className="text-xs">{t("subject")}</Label>
          <Select value={subjectId} onValueChange={setSubjectId}>
            <SelectTrigger className="h-8 text-xs">
              <SelectValue placeholder="Select subject..." />
            </SelectTrigger>
            <SelectContent>
              {subjects.map((s) => (
                <SelectItem key={s.id} value={s.id} className="text-xs">{s.name}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="space-y-1">
          <Label className="text-xs">{t("teacher")}</Label>
          <Select value={teacherId} onValueChange={setTeacherId}>
            <SelectTrigger className="h-8 text-xs">
              <SelectValue placeholder="Select teacher..." />
            </SelectTrigger>
            <SelectContent>
              {teachers.map((teacher) => (
                <SelectItem key={teacher.id} value={teacher.id} className="text-xs">{teacher.name}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="space-y-1">
          <Label className="text-xs">{t("room")}</Label>
          <Select value={roomId} onValueChange={setRoomId}>
            <SelectTrigger className="h-8 text-xs">
              <SelectValue placeholder="Select room..." />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="" className="text-xs">None</SelectItem>
              {rooms.map((r) => (
                <SelectItem key={r.id} value={r.id} className="text-xs">{r.name}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="flex gap-2 justify-end pt-1">
          <Button variant="outline" size="sm" onClick={() => setOpen(false)}>
            Cancel
          </Button>
          <Button size="sm" onClick={handleSave} disabled={updateMutation.isPending}>
            Save
          </Button>
        </div>
      </PopoverContent>
    </Popover>
  );
}
