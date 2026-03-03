"use client";

import { useState, useEffect } from "react";
import { useTranslations } from "next-intl";
import { useWizardStore } from "@/lib/stores/wizard-store";
import { useSubjects, useCreateSubject, useDeleteSubject } from "@/lib/api/hooks/use-subjects";
import { useRooms } from "@/lib/api/hooks/use-rooms";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Trash2, Plus, BookOpen } from "lucide-react";
import { toast } from "sonner";
import { SUBJECT_COLORS, getNextUnusedColor } from "@/lib/utils/color";
import { SubjectColorPicker } from "@/components/ui/subject-color-picker";

export function Step2Subjects() {
  const t = useTranslations("setup");
  const ts = useTranslations("subjects");
  const tc = useTranslations("common");
  const { markStepCompleted, setCurrentStep } = useWizardStore();
  const { data: subjects = [], isLoading } = useSubjects();
  const { data: rooms = [] } = useRooms();
  const createSubject = useCreateSubject();
  const deleteSubject = useDeleteSubject();

  const [name, setName] = useState("");
  const [color, setColor] = useState<string>(SUBJECT_COLORS[0]);
  const [requiresSpecialRoom, setRequiresSpecialRoom] = useState(false);
  const [specialRoomId, setSpecialRoomId] = useState<string | null>(null);

  useEffect(() => {
    setColor(getNextUnusedColor(subjects.map((s) => s.color ?? "")));
  }, [subjects]);

  async function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;
    try {
      await createSubject.mutateAsync({
        name: name.trim(),
        color,
        requiresSpecialRoom,
        specialRoomId: requiresSpecialRoom ? specialRoomId : null,
      });
      setName("");
      setRequiresSpecialRoom(false);
      setSpecialRoomId(null);
    } catch {
      toast.error(tc("error"));
    }
  }

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold flex items-center gap-2">
          <BookOpen className="w-5 h-5" />
          {ts("title")}
        </h2>
        <p className="text-sm text-muted-foreground mt-1">
          {t("step2Description")}
        </p>
      </div>

      <form onSubmit={handleAdd} className="space-y-3">
        <div className="flex gap-2 items-start">
          <div className="flex-1 max-w-xs space-y-2">
            <div className="flex gap-2">
              <Input
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder={ts("subjectName")}
                className="flex-1"
              />
              <SubjectColorPicker value={color} onChange={setColor} />
            </div>
            <div className="flex items-center gap-2">
          <Checkbox
            id="wizard-specialRoom"
            checked={requiresSpecialRoom}
            onCheckedChange={(v) => {
              setRequiresSpecialRoom(!!v);
              if (!v) setSpecialRoomId(null);
            }}
          />
          <label htmlFor="wizard-specialRoom" className="text-sm shrink-0">{ts("requiresSpecialRoom")}</label>
          {requiresSpecialRoom && (
            <Select value={specialRoomId ?? ""} onValueChange={(v) => setSpecialRoomId(v || null)}>
              <SelectTrigger className="flex-1 h-8 text-xs">
                <SelectValue placeholder={ts("selectRoom")} />
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
          <Button type="submit" size="sm" disabled={createSubject.isPending || !name.trim()}>
            <Plus className="w-4 h-4 mr-1" />
            {tc("add")}
          </Button>
        </div>
      </form>

      {isLoading ? (
        <p className="text-sm text-muted-foreground">{tc("loading")}</p>
      ) : subjects.length === 0 ? (
        <p className="text-sm text-muted-foreground">{ts("noSubjects")}</p>
      ) : (
        <ul className="space-y-1">
          {subjects.map((s) => (
            <li key={s.id} className="flex items-center justify-between py-1.5 px-3 rounded-md bg-muted/50">
              <div className="flex items-center gap-2">
                <span className="w-3 h-3 rounded-full shrink-0" style={{ backgroundColor: s.color ?? "#888" }} />
                <span className="text-sm font-medium">{s.name}</span>
              </div>
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7 text-muted-foreground hover:text-destructive"
                onClick={() => deleteSubject.mutate(s.id!)}
              >
                <Trash2 className="w-3.5 h-3.5" />
              </Button>
            </li>
          ))}
        </ul>
      )}

      <div className="pt-2 space-y-1">
        <Button onClick={() => { markStepCompleted(3); setCurrentStep(4); }} disabled={subjects.length === 0}>
          {tc("continue")}{subjects.length > 0 && <Badge variant="secondary" className="ml-2">{subjects.length}</Badge>}
        </Button>
        {subjects.length === 0 && (
          <p className="text-xs text-muted-foreground">{t("addSubjectFirst")}</p>
        )}
      </div>
    </div>
  );
}
