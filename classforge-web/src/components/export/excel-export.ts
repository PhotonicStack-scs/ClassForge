import type { components } from "@/lib/api/schema";

type TimetableViewEntry = components["schemas"]["TimetableViewEntry"];

export async function exportTimetableToExcel(
  entries: TimetableViewEntry[],
  filename = "timetable.xlsx"
) {
  const xlsx = await import("xlsx");

  const rows = entries.map((e) => ({
    Day: e.dayOfWeek ?? "",
    Slot: e.slotNumber ?? "",
    Subject: e.subjectName ?? "",
    Teacher: e.teacherName ?? "",
    Room: e.roomName ?? "",
    Classes: (e.classNames ?? []).join(", "),
  }));

  const ws = xlsx.utils.json_to_sheet(rows);
  const wb = xlsx.utils.book_new();
  xlsx.utils.book_append_sheet(wb, ws, "Timetable");
  xlsx.writeFile(wb, filename);
}
