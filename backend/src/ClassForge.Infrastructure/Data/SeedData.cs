using ClassForge.Domain.Entities;
using ClassForge.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace ClassForge.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedDemoSchoolAsync(AppDbContext db)
    {
        if (db.Tenants.Any()) return;

        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Demo School" };
        db.Tenants.Add(tenant);

        var hasher = new PasswordHasher<User>();
        var admin = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = "admin@demo.school",
            DisplayName = "Admin User",
            Role = UserRole.OrgAdmin
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
        db.Users.Add(admin);

        // Years (6-10)
        var years = Enumerable.Range(6, 5).Select(i => new Year
        {
            Id = Guid.NewGuid(), TenantId = tenant.Id, Name = $"Year {i}", SortOrder = i
        }).ToList();
        db.Years.AddRange(years);

        // Classes (A, B, C per year)
        var classes = new List<Class>();
        foreach (var year in years)
        {
            foreach (var (name, idx) in new[] { ("A", 0), ("B", 1), ("C", 2) })
            {
                classes.Add(new Class
                {
                    Id = Guid.NewGuid(), TenantId = tenant.Id,
                    YearId = year.Id, Name = name, SortOrder = idx
                });
            }
        }
        db.Classes.AddRange(classes);

        // Subjects
        var subjectDefs = new[]
        {
            ("Math", false, (Guid?)null),
            ("Norwegian", false, null),
            ("English", false, null),
            ("Science", false, null),
            ("Social Studies", false, null),
            ("PE", false, null),
            ("Music", false, null),
            ("Art", false, null),
            ("Food & Health", false, null),
            ("KRLE", false, null),
            ("Natural Science", false, null),
            ("Spanish", false, null),
            ("Technology & Design", false, null),
            ("Programming", false, null),
            ("Math Extra", false, null)
        };

        var subjects = subjectDefs.Select(s => new Subject
        {
            Id = Guid.NewGuid(), TenantId = tenant.Id,
            Name = s.Item1, RequiresSpecialRoom = s.Item2,
            SpecialRoomId = s.Item3
        }).ToList();
        db.Subjects.AddRange(subjects);

        // Rooms
        var rooms = new[]
        {
            new Room { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Gym", Capacity = 3 },
            new Room { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Music Room", Capacity = 2 },
            new Room { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Art Room", Capacity = 1 },
            new Room { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Computer Lab", Capacity = 1 },
            new Room { Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Kitchen", Capacity = 1 }
        };
        db.Rooms.AddRange(rooms);

        // School days (Mon-Fri)
        var days = Enumerable.Range(1, 5).Select(d => new SchoolDay
        {
            Id = Guid.NewGuid(), TenantId = tenant.Id,
            DayOfWeek = d, IsActive = true, SortOrder = d
        }).ToList();
        db.SchoolDays.AddRange(days);

        // Time slots: 7 non-break + 2 breaks per day
        var slotDefs = new[]
        {
            (1, "08:15", "09:00", false),
            (2, "09:05", "09:50", false),
            (3, "09:50", "10:05", true),   // break
            (4, "10:05", "10:50", false),
            (5, "10:55", "11:40", false),
            (6, "11:40", "12:10", true),   // lunch
            (7, "12:10", "12:55", false),
            (8, "13:00", "13:45", false),
            (9, "13:50", "14:35", false)
        };

        foreach (var day in days)
        {
            foreach (var (num, start, end, isBreak) in slotDefs)
            {
                db.TimeSlots.Add(new TimeSlot
                {
                    Id = Guid.NewGuid(), TenantId = tenant.Id,
                    SchoolDayId = day.Id, SlotNumber = num,
                    StartTime = TimeOnly.Parse(start), EndTime = TimeOnly.Parse(end),
                    IsBreak = isBreak
                });
            }
        }

        // YearDayConfigs: 6 periods for years 6-7, 7 for years 8-10
        foreach (var year in years)
        {
            var maxPeriods = year.SortOrder <= 7 ? 6 : 7;
            foreach (var day in days)
            {
                db.YearDayConfigs.Add(new YearDayConfig
                {
                    Id = Guid.NewGuid(), TenantId = tenant.Id,
                    YearId = year.Id, SchoolDayId = day.Id, MaxPeriods = maxPeriods
                });
            }
        }

        // Subject lookups
        var math = subjects.First(s => s.Name == "Math");
        var norwegian = subjects.First(s => s.Name == "Norwegian");
        var english = subjects.First(s => s.Name == "English");
        var science = subjects.First(s => s.Name == "Science");
        var socialStudies = subjects.First(s => s.Name == "Social Studies");
        var pe = subjects.First(s => s.Name == "PE");
        var music = subjects.First(s => s.Name == "Music");
        var art = subjects.First(s => s.Name == "Art");
        var foodHealth = subjects.First(s => s.Name == "Food & Health");
        var krle = subjects.First(s => s.Name == "KRLE");
        var natSci = subjects.First(s => s.Name == "Natural Science");
        var spanish = subjects.First(s => s.Name == "Spanish");

        // YearCurricula (realistic period counts)
        var coreSubjects = new (Subject subj, int periods, bool preferDouble, int maxPPD, bool allowDouble)[]
        {
            (math, 5, true, 2, true), (norwegian, 5, true, 2, true), (english, 3, false, 2, true),
            (science, 2, false, 2, false), (socialStudies, 2, false, 2, false), (pe, 3, true, 2, true),
            (music, 1, false, 1, false), (art, 2, false, 2, false), (foodHealth, 2, false, 2, false),
            (krle, 2, false, 2, false), (natSci, 2, false, 2, false), (spanish, 2, false, 2, false)
        };

        foreach (var year in years)
        {
            foreach (var (subj, periods, preferDouble, maxPPD, allowDouble) in coreSubjects)
            {
                db.YearCurricula.Add(new YearCurriculum
                {
                    Id = Guid.NewGuid(), TenantId = tenant.Id,
                    YearId = year.Id, SubjectId = subj.Id,
                    PeriodsPerWeek = periods, PreferDoublePeriods = preferDouble,
                    MaxPeriodsPerDay = maxPPD, AllowDoublePeriods = allowDouble
                });
            }
        }

        // Combined lessons: PE mandatory combined (2 classes at a time)
        foreach (var year in years)
        {
            var yearClasses = classes.Where(c => c.YearId == year.Id).ToList();
            var peConfig = new CombinedLessonConfig
            {
                Id = Guid.NewGuid(), TenantId = tenant.Id,
                YearId = year.Id, SubjectId = pe.Id,
                IsMandatory = true, MaxClassesPerLesson = 2,
                Classes = yearClasses.Take(2).Select(c => new CombinedLessonClass { ClassId = c.Id }).ToList()
            };
            db.CombinedLessonConfigs.Add(peConfig);

            // Music optional combined
            var musicConfig = new CombinedLessonConfig
            {
                Id = Guid.NewGuid(), TenantId = tenant.Id,
                YearId = year.Id, SubjectId = music.Id,
                IsMandatory = false, MaxClassesPerLesson = 2,
                Classes = yearClasses.Take(2).Select(c => new CombinedLessonClass { ClassId = c.Id }).ToList()
            };
            db.CombinedLessonConfigs.Add(musicConfig);
        }

        // Teachers (30 teachers)
        var teacherNames = new[]
        {
            "Erik Hansen", "Maria Johansen", "Ole Nilsen", "Kari Andersen", "Lars Pedersen",
            "Anna Larsen", "Per Olsen", "Ingrid Karlsen", "Bjorn Eriksen", "Solveig Berg",
            "Thomas Haugen", "Hilde Johnsrud", "Anders Moen", "Liv Bakken", "Morten Dahl",
            "Silje Iversen", "Rune Svendsen", "Camilla Lund", "Geir Holmen", "Tone Vik",
            "Espen Strand", "Kristin Haug", "Petter Aas", "Marianne Holm", "Trond Berge",
            "Berit Nygaard", "Stian Henriksen", "Elin Sandberg", "Jon Gulbrandsen", "Lise Fossum"
        };

        var teachers = teacherNames.Select((name, idx) => new Teacher
        {
            Id = Guid.NewGuid(), TenantId = tenant.Id,
            Name = name, Email = $"teacher{idx + 1}@demo.school"
        }).ToList();
        db.Teachers.AddRange(teachers);

        // Assign qualifications spanning appropriate years
        // Math teachers (6): cover all years
        for (var i = 0; i < 6; i++)
        {
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[i].Id,
                SubjectId = math.Id, MinYearId = years.First().Id, MaxYearId = years.Last().Id
            });
        }

        // Norwegian teachers (6)
        for (var i = 0; i < 6; i++)
        {
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[6 + i].Id,
                SubjectId = norwegian.Id, MinYearId = years.First().Id, MaxYearId = years.Last().Id
            });
        }

        // English teachers (4)
        for (var i = 0; i < 4; i++)
        {
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[12 + i].Id,
                SubjectId = english.Id, MinYearId = years.First().Id, MaxYearId = years.Last().Id
            });
        }

        // Science + Nat Sci teachers (4, dual-qualified)
        for (var i = 0; i < 4; i++)
        {
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[16 + i].Id,
                SubjectId = science.Id, MinYearId = years.First().Id, MaxYearId = years.Last().Id
            });
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[16 + i].Id,
                SubjectId = natSci.Id, MinYearId = years.First().Id, MaxYearId = years.Last().Id
            });
        }

        // Social Studies + KRLE teachers (4, dual-qualified)
        for (var i = 0; i < 4; i++)
        {
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[20 + i].Id,
                SubjectId = socialStudies.Id, MinYearId = years.First().Id, MaxYearId = years.Last().Id
            });
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[20 + i].Id,
                SubjectId = krle.Id, MinYearId = years.First().Id, MaxYearId = years.Last().Id
            });
        }

        // PE teachers (3)
        for (var i = 0; i < 3; i++)
        {
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[24 + i].Id,
                SubjectId = pe.Id, MinYearId = years.First().Id, MaxYearId = years.Last().Id
            });
        }

        // Remaining: Music, Art, Food & Health, Spanish
        db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
        {
            Id = Guid.NewGuid(), TeacherId = teachers[27].Id,
            SubjectId = music.Id, MinYearId = years.First().Id, MaxYearId = years.Last().Id
        });
        db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
        {
            Id = Guid.NewGuid(), TeacherId = teachers[27].Id,
            SubjectId = art.Id, MinYearId = years.First().Id, MaxYearId = years.Last().Id
        });
        db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
        {
            Id = Guid.NewGuid(), TeacherId = teachers[28].Id,
            SubjectId = foodHealth.Id, MinYearId = years.First().Id, MaxYearId = years.Last().Id
        });
        db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
        {
            Id = Guid.NewGuid(), TeacherId = teachers[28].Id,
            SubjectId = art.Id, MinYearId = years.First().Id, MaxYearId = years.Last().Id
        });
        db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
        {
            Id = Guid.NewGuid(), TeacherId = teachers[29].Id,
            SubjectId = spanish.Id, MinYearId = years.First().Id, MaxYearId = years.Last().Id
        });
        db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
        {
            Id = Guid.NewGuid(), TeacherId = teachers[29].Id,
            SubjectId = music.Id, MinYearId = years.First().Id, MaxYearId = years.Last().Id
        });

        // Teacher day configs: all teachers available Mon-Fri, 7 periods max
        foreach (var teacher in teachers)
        {
            foreach (var day in days)
            {
                db.TeacherDayConfigs.Add(new TeacherDayConfig
                {
                    Id = Guid.NewGuid(), TeacherId = teacher.Id,
                    SchoolDayId = day.Id, MaxPeriods = 7
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
