using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public static class DataSeeder
{
    public static void Seed(TrainingContext context)
    {
        // try nhanh để tránh timeout trên DB lớn
        context.Database.SetCommandTimeout(15);

        // Ensure Roles exist
        var existingRoles = context.Roles.Select(r => r.Name).ToList();
        Role rAdmin = context.Roles.FirstOrDefault(r => r.Name == "Admin") ?? new Role { Name = "Admin" };
        Role rInstructor = context.Roles.FirstOrDefault(r => r.Name == "Instructor") ?? new Role { Name = "Instructor" };
        Role rStudent = context.Roles.FirstOrDefault(r => r.Name == "Student") ?? new Role { Name = "Student" };
        
        if (!existingRoles.Contains("Admin")) context.Roles.Add(rAdmin);
        if (!existingRoles.Contains("Instructor")) context.Roles.Add(rInstructor);
        if (!existingRoles.Contains("Student")) context.Roles.Add(rStudent);
        context.SaveChanges();

        if (context.Users.Take(1).Any())
        {
            return; // already seeded core entities
        }

        var roles = new[] { rAdmin, rInstructor, rStudent };

        var permissions = new[]
        {
            new Permission { Name = "Create_Course" },
            new Permission { Name = "View_Report" },
            new Permission { Name = "Take_Exam" }
        };

        var departments = new[]
        {
            new Department { Name = "Marketing" },
            new Department { Name = "Thiết kế đồ họa" },
            new Department { Name = "Hardware" },
            new Department { Name = "Software Developer" }
        };

        var positions = new[]
        {
            new Position { Name = "Nhân viên" },
            new Position { Name = "Trưởng phòng" },
            new Position { Name = "Giảng viên nội bộ" }
        };

        var admin = new User
        {
            Username = "admin_user",
            Password = "123",
            Email = "admin@company.com",
            FullName = "Nguyễn Văn Admin",
            Department = departments[3],
            Position = positions[1]
        };

        var instructor = new User
        {
            Username = "gv_son",
            Password = "123",
            Email = "son.instructor@company.com",
            FullName = "Trần Thế Sơn",
            Department = departments[1],
            Position = positions[2]
        };

        var student = new User
        {
            Username = "hv_an",
            Password = "123",
            Email = "an.student@company.com",
            FullName = "Lê Thị An",
            Department = departments[0],
            Position = positions[0]
        };

        var categories = new[]
        {
            new CourseCategory { Name = "Marketing" },
            new CourseCategory { Name = "Graphic Design" },
            new CourseCategory { Name = "Hardware" },
            new CourseCategory { Name = "Software Development" }
        };

        var levels = new[]
        {
            new CourseLevel { Name = "Cơ bản" },
            new CourseLevel { Name = "Nâng cao" }
        };

        var courses = new[]
        {
            new Course { CourseName = "Digital Marketing 101", Category = categories[0], Level = levels[0] },
            new Course { CourseName = "Thiết kế đồ họa với Figma", Category = categories[1], Level = levels[0] },
            new Course { CourseName = "Phần cứng máy tính nền tảng", Category = categories[2], Level = levels[0] },
            new Course { CourseName = "Lộ trình .NET Backend Developer", Category = categories[3], Level = levels[1] }
        };

        var courseInstructors = new[]
        {
            new CourseInstructor { Course = courses[0], User = instructor },
            new CourseInstructor { Course = courses[1], User = instructor },
            new CourseInstructor { Course = courses[3], User = instructor }
        };

        var classRooms = new[]
        {
            new ClassRoom { Name = "Trực tuyến" },
            new ClassRoom { Name = "Lab 1" },
            new ClassRoom { Name = "Phòng họp 2" }
        };

        var classStatus = new[]
        {
            new ClassStatus { Name = "Đang mở" },
            new ClassStatus { Name = "Sắp khai giảng" },
            new ClassStatus { Name = "Đã kết thúc" }
        };

        var marketingClass = new Class
        {
            Course = courses[0],
            Instructor = instructor,
            Room = classRooms[0],
            Status = classStatus[0],
            Schedules = new List<ClassSchedule>
            {
                new() { ScheduleDate = DateTime.UtcNow.AddDays(2) },
                new() { ScheduleDate = DateTime.UtcNow.AddDays(9) }
            }
        };

        var dotnetClass = new Class
        {
            Course = courses[3],
            Instructor = instructor,
            Room = classRooms[1],
            Status = classStatus[1],
            Schedules = new List<ClassSchedule>
            {
                new() { ScheduleDate = DateTime.UtcNow.AddDays(5) }
            }
        };

        context.ClassSchedules.AddRange(marketingClass.Schedules);
        context.ClassSchedules.AddRange(dotnetClass.Schedules);

        context.SaveChanges();

        context.UserRoles.AddRange(
            new UserRole { User = admin, Role = roles[0] },
            new UserRole { User = instructor, Role = roles[1] },
            new UserRole { User = student, Role = roles[2] }
        );

        context.RolePermissions.AddRange(
            new RolePermission { Role = roles[0], Permission = permissions[0] },
            new RolePermission { Role = roles[0], Permission = permissions[1] },
            new RolePermission { Role = roles[1], Permission = permissions[0] },
            new RolePermission { Role = roles[2], Permission = permissions[2] }
        );

        context.SaveChanges();
    }
}
