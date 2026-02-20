export const locales = ["nb", "nn", "en"] as const;
export type Locale = (typeof locales)[number];
export const defaultLocale: Locale = "nb";

export const localeNames: Record<Locale, string> = {
  nb: "Bokmål",
  nn: "Nynorsk",
  en: "English",
};
