import { getRequestConfig } from "next-intl/server";
import { routing } from "./routing";

export default getRequestConfig(async ({ requestLocale }) => {
  let locale = await requestLocale;

  if (!locale || !routing.locales.includes(locale as "nb" | "nn" | "en")) {
    locale = routing.defaultLocale;
  }

  const [
    common,
    auth,
    setup,
    grades,
    subjects,
    rooms,
    teachers,
    timeStructure,
    timetable,
    users,
    settings,
  ] = await Promise.all([
    import(`../../messages/${locale}/common.json`),
    import(`../../messages/${locale}/auth.json`),
    import(`../../messages/${locale}/setup.json`),
    import(`../../messages/${locale}/grades.json`),
    import(`../../messages/${locale}/subjects.json`),
    import(`../../messages/${locale}/rooms.json`),
    import(`../../messages/${locale}/teachers.json`),
    import(`../../messages/${locale}/timeStructure.json`),
    import(`../../messages/${locale}/timetable.json`),
    import(`../../messages/${locale}/users.json`),
    import(`../../messages/${locale}/settings.json`),
  ]);

  return {
    locale,
    messages: {
      common: common.default,
      auth: auth.default,
      setup: setup.default,
      grades: grades.default,
      subjects: subjects.default,
      rooms: rooms.default,
      teachers: teachers.default,
      timeStructure: timeStructure.default,
      timetable: timetable.default,
      users: users.default,
      settings: settings.default,
    },
  };
});
