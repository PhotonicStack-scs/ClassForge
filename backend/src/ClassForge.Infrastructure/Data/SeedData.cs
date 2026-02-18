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

        // Grades (6-10)
        var grades = Enumerable.Range(6, 5).Select(i => new Grade
        {
            Id = Guid.NewGuid(), TenantId = tenant.Id, Name = $"Grade {i}", SortOrder = i
        }).ToList();
        db.Grades.AddRange(grades);

        // Groups (A, B, C per grade)
        var groups = new List<Group>();
        foreach (var grade in grades)
        {
            foreach (var (name, idx) in new[] { ("A", 0), ("B", 1), ("C", 2) })
            {
                groups.Add(new Group
                {
                    Id = Guid.NewGuid(), TenantId = tenant.Id,
                    GradeId = grade.Id, Name = name, SortOrder = idx
                });
            }
        }
        db.Groups.AddRange(groups);

        // Subjects
        var subjectDefs = new[]
        {
            ("Math", false, (Guid?)null, 2, true),
            ("Norwegian", false, null, 2, true),
            ("English", false, null, 2, true),
            ("Science", false, null, 2, false),
            ("Social Studies", false, null, 2, false),
            ("PE", false, null, 2, true),
            ("Music", false, null, 1, false),
            ("Art", false, null, 2, false),
            ("Food & Health", false, null, 2, false),
            ("KRLE", false, null, 2, false),
            ("Natural Science", false, null, 2, false),
            ("Spanish", false, null, 2, false),
            ("Technology & Design", false, null, 2, false),
            ("Programming", false, null, 1, false),
            ("Math Extra", false, null, 1, false)
        };

        var subjects = subjectDefs.Select(s => new Subject
        {
            Id = Guid.NewGuid(), TenantId = tenant.Id,
            Name = s.Item1, RequiresSpecialRoom = s.Item2,
            SpecialRoomId = s.Item3, MaxPeriodsPerDay = s.Item4,
            AllowDoublePeriods = s.Item5
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

        // Teaching days (Mon-Fri)
        var days = Enumerable.Range(1, 5).Select(d => new TeachingDay
        {
            Id = Guid.NewGuid(), TenantId = tenant.Id,
            DayOfWeek = d, IsActive = true, SortOrder = d
        }).ToList();
        db.TeachingDays.AddRange(days);

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
                    TeachingDayId = day.Id, SlotNumber = num,
                    StartTime = TimeOnly.Parse(start), EndTime = TimeOnly.Parse(end),
                    IsBreak = isBreak
                });
            }
        }

        // GradeDayConfigs: 6 periods for grades 6-7, 7 for grades 8-10
        foreach (var grade in grades)
        {
            var maxPeriods = grade.SortOrder <= 7 ? 6 : 7;
            foreach (var day in days)
            {
                db.GradeDayConfigs.Add(new GradeDayConfig
                {
                    Id = Guid.NewGuid(), TenantId = tenant.Id,
                    GradeId = grade.Id, TeachingDayId = day.Id, MaxPeriods = maxPeriods
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

        // GradeSubjectRequirements (realistic period counts)
        var coreSubjects = new (Subject subj, int periods, bool preferDouble)[]
        {
            (math, 5, true), (norwegian, 5, true), (english, 3, false),
            (science, 2, false), (socialStudies, 2, false), (pe, 3, true),
            (music, 1, false), (art, 2, false), (foodHealth, 2, false),
            (krle, 2, false), (natSci, 2, false), (spanish, 2, false)
        };

        foreach (var grade in grades)
        {
            foreach (var (subj, periods, preferDouble) in coreSubjects)
            {
                db.GradeSubjectRequirements.Add(new GradeSubjectRequirement
                {
                    Id = Guid.NewGuid(), TenantId = tenant.Id,
                    GradeId = grade.Id, SubjectId = subj.Id,
                    PeriodsPerWeek = periods, PreferDoublePeriods = preferDouble
                });
            }
        }

        // Combined lessons: PE mandatory combined (2 groups at a time)
        foreach (var grade in grades)
        {
            var gradeGroups = groups.Where(g => g.GradeId == grade.Id).ToList();
            var peConfig = new CombinedLessonConfig
            {
                Id = Guid.NewGuid(), TenantId = tenant.Id,
                GradeId = grade.Id, SubjectId = pe.Id,
                IsMandatory = true, MaxGroupsPerLesson = 2,
                Groups = gradeGroups.Take(2).Select(g => new CombinedLessonGroup { GroupId = g.Id }).ToList()
            };
            db.CombinedLessonConfigs.Add(peConfig);

            // Music optional combined
            var musicConfig = new CombinedLessonConfig
            {
                Id = Guid.NewGuid(), TenantId = tenant.Id,
                GradeId = grade.Id, SubjectId = music.Id,
                IsMandatory = false, MaxGroupsPerLesson = 2,
                Groups = gradeGroups.Take(2).Select(g => new CombinedLessonGroup { GroupId = g.Id }).ToList()
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

        // Assign qualifications spanning appropriate grades
        // Math teachers (6): cover all grades
        for (var i = 0; i < 6; i++)
        {
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[i].Id,
                SubjectId = math.Id, MinGradeId = grades.First().Id, MaxGradeId = grades.Last().Id
            });
        }

        // Norwegian teachers (6)
        for (var i = 0; i < 6; i++)
        {
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[6 + i].Id,
                SubjectId = norwegian.Id, MinGradeId = grades.First().Id, MaxGradeId = grades.Last().Id
            });
        }

        // English teachers (4)
        for (var i = 0; i < 4; i++)
        {
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[12 + i].Id,
                SubjectId = english.Id, MinGradeId = grades.First().Id, MaxGradeId = grades.Last().Id
            });
        }

        // Science + Nat Sci teachers (4, dual-qualified)
        for (var i = 0; i < 4; i++)
        {
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[16 + i].Id,
                SubjectId = science.Id, MinGradeId = grades.First().Id, MaxGradeId = grades.Last().Id
            });
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[16 + i].Id,
                SubjectId = natSci.Id, MinGradeId = grades.First().Id, MaxGradeId = grades.Last().Id
            });
        }

        // Social Studies + KRLE teachers (4, dual-qualified)
        for (var i = 0; i < 4; i++)
        {
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[20 + i].Id,
                SubjectId = socialStudies.Id, MinGradeId = grades.First().Id, MaxGradeId = grades.Last().Id
            });
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[20 + i].Id,
                SubjectId = krle.Id, MinGradeId = grades.First().Id, MaxGradeId = grades.Last().Id
            });
        }

        // PE teachers (3)
        for (var i = 0; i < 3; i++)
        {
            db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
            {
                Id = Guid.NewGuid(), TeacherId = teachers[24 + i].Id,
                SubjectId = pe.Id, MinGradeId = grades.First().Id, MaxGradeId = grades.Last().Id
            });
        }

        // Remaining: Music, Art, Food & Health, Spanish
        db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
        {
            Id = Guid.NewGuid(), TeacherId = teachers[27].Id,
            SubjectId = music.Id, MinGradeId = grades.First().Id, MaxGradeId = grades.Last().Id
        });
        db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
        {
            Id = Guid.NewGuid(), TeacherId = teachers[27].Id,
            SubjectId = art.Id, MinGradeId = grades.First().Id, MaxGradeId = grades.Last().Id
        });
        db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
        {
            Id = Guid.NewGuid(), TeacherId = teachers[28].Id,
            SubjectId = foodHealth.Id, MinGradeId = grades.First().Id, MaxGradeId = grades.Last().Id
        });
        db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
        {
            Id = Guid.NewGuid(), TeacherId = teachers[28].Id,
            SubjectId = art.Id, MinGradeId = grades.First().Id, MaxGradeId = grades.Last().Id
        });
        db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
        {
            Id = Guid.NewGuid(), TeacherId = teachers[29].Id,
            SubjectId = spanish.Id, MinGradeId = grades.First().Id, MaxGradeId = grades.Last().Id
        });
        db.TeacherSubjectQualifications.Add(new TeacherSubjectQualification
        {
            Id = Guid.NewGuid(), TeacherId = teachers[29].Id,
            SubjectId = music.Id, MinGradeId = grades.First().Id, MaxGradeId = grades.Last().Id
        });

        // Teacher day configs: all teachers available Mon-Fri, 7 periods max
        foreach (var teacher in teachers)
        {
            foreach (var day in days)
            {
                db.TeacherDayConfigs.Add(new TeacherDayConfig
                {
                    Id = Guid.NewGuid(), TeacherId = teacher.Id,
                    TeachingDayId = day.Id, MaxPeriods = 7
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
