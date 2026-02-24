export const SUBJECT_COLORS = [
  "#FF6B6B", // 01 - red
  "#FF8E53", // 02 - orange
  "#FFCA3A", // 03 - yellow
  "#8AC926", // 04 - lime
  "#1982C4", // 05 - blue
  "#6A4C93", // 06 - purple
  "#FF595E", // 07 - coral
  "#FFEE93", // 08 - pale yellow
  "#C7EFCF", // 09 - pale green
  "#B5EAD7", // 10 - mint
  "#ADE8F4", // 11 - sky
  "#CDB4DB", // 12 - lavender
  "#FFC8DD", // 13 - pink
  "#FFAFCC", // 14 - rose
  "#BDE0FE", // 15 - periwinkle
  "#A2D2FF", // 16 - light blue
  "#E0BBE4", // 17 - orchid
  "#FED9B7", // 18 - peach
  "#D0F4DE", // 19 - sage
  "#A8DADC", // 20 - teal
] as const;

export function getSubjectColor(index: number): string {
  return SUBJECT_COLORS[index % SUBJECT_COLORS.length];
}

export function getNextUnusedColor(usedColors: string[]): string {
  const unused = SUBJECT_COLORS.find((c) => !usedColors.includes(c));
  return unused ?? SUBJECT_COLORS[usedColors.length % SUBJECT_COLORS.length];
}

export function getContrastColor(hex: string): "black" | "white" {
  const r = parseInt(hex.slice(1, 3), 16);
  const g = parseInt(hex.slice(3, 5), 16);
  const b = parseInt(hex.slice(5, 7), 16);
  // Luminance
  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
  return luminance > 0.5 ? "black" : "white";
}
