using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class TrainingContext : DbContext
{
    public TrainingContext(DbContextOptions<TrainingContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();
    public DbSet<CourseLevel> CourseLevels => Set<CourseLevel>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseInstructor> CourseInstructors => Set<CourseInstructor>();
    public DbSet<CourseTag> CourseTags => Set<CourseTag>();
    public DbSet<CourseTagMapping> CourseTagMappings => Set<CourseTagMapping>();
    public DbSet<CoursePrerequisite> CoursePrerequisites => Set<CoursePrerequisite>();
    public DbSet<ClassRoom> ClassRooms => Set<ClassRoom>();
    public DbSet<ClassStatus> ClassStatuses => Set<ClassStatus>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
    public DbSet<LearningProgress> LearningProgresses => Set<LearningProgress>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<ClassStudent> ClassStudents => Set<ClassStudent>();
    public DbSet<ClassAttendance> ClassAttendances => Set<ClassAttendance>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonContent> LessonContents => Set<LessonContent>();
    public DbSet<LessonVideo> LessonVideos => Set<LessonVideo>();
    public DbSet<LessonDocument> LessonDocuments => Set<LessonDocument>();
    public DbSet<LessonAssignment> LessonAssignments => Set<LessonAssignment>();
    public DbSet<LessonComment> LessonComments => Set<LessonComment>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();
    public DbSet<ExamResult> ExamResults => Set<ExamResult>();
    public DbSet<AnswerDetail> AnswerDetails => Set<AnswerDetail>();
    public DbSet<CertificateTemplate> CertificateTemplates => Set<CertificateTemplate>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<UserCertificate> UserCertificates => Set<UserCertificate>();
    public DbSet<CertificateLog> CertificateLogs => Set<CertificateLog>();
    public DbSet<CourseCompletion> CourseCompletions => Set<CourseCompletion>();
    public DbSet<StudyTimeLog> StudyTimeLogs => Set<StudyTimeLog>();
    public DbSet<NotificationType> NotificationTypes => Set<NotificationType>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<CourseReview> CourseReviews => Set<CourseReview>();
    public DbSet<InstructorReview> InstructorReviews => Set<InstructorReview>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<FileType> FileTypes => Set<FileType>();
    public DbSet<SystemFile> Files => Set<SystemFile>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();
    public DbSet<Token> Tokens => Set<Token>();
    public DbSet<InstructorCourseApplication> InstructorCourseApplications => Set<InstructorCourseApplication>();
    public DbSet<CourseForumMessage> CourseForumMessages => Set<CourseForumMessage>();
    public DbSet<CourseChapter> CourseChapters => Set<CourseChapter>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();

    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Block> Blocks => Set<Block>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRole>(entity => {
            entity.HasKey(x => new { x.UserId, x.RoleId });
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
        });

        modelBuilder.Entity<RolePermission>(entity => {
            entity.HasKey(x => new { x.RoleId, x.PermissionId });
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.PermissionId).HasColumnName("PermissionID");
        });

        modelBuilder.Entity<CourseInstructor>().HasKey(x => new { x.CourseId, x.UserId });
        modelBuilder.Entity<CourseTagMapping>().HasKey(x => new { x.CourseId, x.TagId });
        modelBuilder.Entity<CoursePrerequisite>().HasKey(x => new { x.CourseId, x.PrerequisiteId });
        modelBuilder.Entity<ClassStudent>().HasKey(x => new { x.ClassId, x.UserId });
        
        modelBuilder.Entity<UserNotification>(entity => {
            entity.HasKey(x => new { x.UserId, x.NotificationId });
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
        });

        // Audit Tracking Restrictions (Prevent Multiple Cascade Paths)
        modelBuilder.Entity<Lesson>().HasOne(l => l.CreatedByUser).WithMany().HasForeignKey(l => l.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Lesson>().HasOne(l => l.UpdatedByUser).WithMany().HasForeignKey(l => l.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Exam>().HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Exam>().HasOne(e => e.UpdatedByUser).WithMany().HasForeignKey(e => e.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Assignment>().HasOne(a => a.CreatedByUser).WithMany().HasForeignKey(a => a.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Assignment>().HasOne(a => a.UpdatedByUser).WithMany().HasForeignKey(a => a.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AssignmentSubmission>().HasOne(s => s.GradedByUser).WithMany().HasForeignKey(s => s.GradedByUserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AssignmentSubmission>().HasOne(s => s.Student).WithMany().HasForeignKey(s => s.StudentId).OnDelete(DeleteBehavior.Cascade);


        // 1. Core Models
        modelBuilder.Entity<Role>(e => e.Property(x => x.Id).HasColumnName("RoleID"));
        modelBuilder.Entity<Permission>(e => e.Property(x => x.Id).HasColumnName("PermissionID"));
        modelBuilder.Entity<Department>(e => e.Property(x => x.Id).HasColumnName("DepartmentID"));
        modelBuilder.Entity<Position>(e => e.Property(x => x.Id).HasColumnName("PositionID"));

        modelBuilder.Entity<User>(entity => {
            entity.ToTable("Users");
            entity.Property(e => e.Id).HasColumnName("UserID");
        });

        // 2. Course Models
        modelBuilder.Entity<CourseCategory>(e => e.Property(x => x.Id).HasColumnName("CategoryID"));
        modelBuilder.Entity<CourseLevel>(e => e.Property(x => x.Id).HasColumnName("LevelID"));
        
        modelBuilder.Entity<Course>(entity => {
            entity.ToTable("Courses");
            entity.Property(e => e.Id).HasColumnName("CourseID");
        });

        modelBuilder.Entity<CourseTag>(e => e.Property(x => x.Id).HasColumnName("TagID"));
        
        modelBuilder.Entity<CourseInstructor>(entity => {
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
        });

        modelBuilder.Entity<CourseTagMapping>(entity => {
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.TagId).HasColumnName("TagID");
        });

        modelBuilder.Entity<CoursePrerequisite>(entity => {
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.PrerequisiteId).HasColumnName("PrerequisiteID");
        });

        // 3. Class Models
        modelBuilder.Entity<ClassRoom>(e => {
            e.ToTable("ClassRooms");
            e.Property(x => x.Id).HasColumnName("RoomID");
        });
        
        modelBuilder.Entity<ClassStatus>(e => {
            e.ToTable("ClassStatuses");
            e.Property(x => x.Id).HasColumnName("StatusID");
        });

        modelBuilder.Entity<Class>(entity => {
            entity.ToTable("Classes");
            entity.Property(e => e.Id).HasColumnName("ClassID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.InstructorId).HasColumnName("InstructorID");
        });

        modelBuilder.Entity<ClassSchedule>(entity => {
            entity.ToTable("ClassSchedules");
            entity.Property(e => e.Id).HasColumnName("ScheduleID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
        });

        modelBuilder.Entity<ClassStudent>(entity => {
            entity.ToTable("ClassStudents");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
        });

        modelBuilder.Entity<ClassAttendance>(entity => {
            entity.ToTable("ClassAttendance");
            entity.Property(e => e.Id).HasColumnName("AttendanceID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
        });

        // 4. Lesson Models
        modelBuilder.Entity<Lesson>(entity => {
            entity.ToTable("Lessons");
            entity.Property(e => e.Id).HasColumnName("LessonID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.ChapterId).HasColumnName("ChapterID");
        });

        modelBuilder.Entity<CourseChapter>(entity => {
            entity.ToTable("CourseChapters");
            entity.Property(e => e.Id).HasColumnName("ChapterID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
        });

        modelBuilder.Entity<LessonContent>(e => {
            e.Property(x => x.Id).HasColumnName("ContentID");
            e.Property(x => x.LessonId).HasColumnName("LessonID");
        });

        modelBuilder.Entity<LessonVideo>(e => {
            e.Property(x => x.Id).HasColumnName("VideoID");
            e.Property(x => x.LessonId).HasColumnName("LessonID");
        });

        modelBuilder.Entity<LessonDocument>(e => {
            e.Property(x => x.Id).HasColumnName("DocumentID");
            e.Property(x => x.LessonId).HasColumnName("LessonID");
        });

        modelBuilder.Entity<LessonAssignment>(e => {
            e.Property(x => x.Id).HasColumnName("AssignmentID");
            e.Property(x => x.LessonId).HasColumnName("LessonID");
        });

        modelBuilder.Entity<LessonComment>(e => {
            e.Property(x => x.Id).HasColumnName("CommentID");
            e.Property(x => x.LessonId).HasColumnName("LessonID");
            e.Property(x => x.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<LearningProgress>(entity => {
            entity.ToTable("LearningProgresses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ProgressID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.LessonId).HasColumnName("LessonID");
        });

        // 5. Exam Models
        modelBuilder.Entity<Exam>(entity => {
            entity.Property(e => e.Id).HasColumnName("ExamID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
        });

        modelBuilder.Entity<Question>(e => {
            e.Property(x => x.Id).HasColumnName("QuestionID");
            e.Property(x => x.ExamId).HasColumnName("ExamID");
        });

        modelBuilder.Entity<QuestionOption>(e => {
            e.Property(x => x.Id).HasColumnName("OptionID");
            e.Property(x => x.QuestionId).HasColumnName("QuestionID");
        });

        modelBuilder.Entity<ExamAttempt>(e => {
            e.Property(x => x.Id).HasColumnName("AttemptID");
            e.Property(x => x.ExamId).HasColumnName("ExamID");
            e.Property(x => x.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<ExamResult>(e => {
            e.Property(x => x.Id).HasColumnName("ResultID");
            e.Property(x => x.AttemptId).HasColumnName("AttemptID");
        });

        modelBuilder.Entity<AnswerDetail>(e => {
            e.Property(x => x.Id).HasColumnName("DetailID");
            e.Property(x => x.AttemptId).HasColumnName("AttemptID");
            e.Property(x => x.QuestionId).HasColumnName("QuestionID");
            e.Property(x => x.SelectedOptionId).HasColumnName("OptionID");
        });

        // 6. Certificate Models
        modelBuilder.Entity<CertificateTemplate>(e => e.Property(x => x.Id).HasColumnName("TemplateID"));
        modelBuilder.Entity<Certificate>(e => e.Property(x => x.Id).HasColumnName("CertificateID"));
        
        modelBuilder.Entity<UserCertificate>(entity => {
            entity.Property(e => e.Id).HasColumnName("UserCertificateID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CertificateId).HasColumnName("CertificateID");
        });

        modelBuilder.Entity<CertificateLog>(e => {
            e.Property(x => x.Id).HasColumnName("LogID");
            e.Property(x => x.CertificateId).HasColumnName("CertificateID");
        });

        // 7. General System Models
        modelBuilder.Entity<CourseCompletion>(entity => {
            entity.Property(e => e.Id).HasColumnName("CompletionID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
        });

        modelBuilder.Entity<StudyTimeLog>(e => {
            e.Property(x => x.Id).HasColumnName("LogID");
            e.Property(x => x.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<NotificationType>(e => e.Property(x => x.Id).HasColumnName("TypeID"));
        modelBuilder.Entity<Notification>(e => e.Property(x => x.Id).HasColumnName("NotificationID"));
        modelBuilder.Entity<UserNotification>(e => e.Property(x => x.UserId).HasColumnName("UserID"));

        modelBuilder.Entity<Rating>(e => e.Property(x => x.Id).HasColumnName("RatingID"));
        
        modelBuilder.Entity<CourseReview>(entity => {
            entity.ToTable("CourseReviews");
            entity.Property(e => e.Id).HasColumnName("ReviewID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.RatingId).HasColumnName("RatingID");
        });

        modelBuilder.Entity<InstructorReview>(entity => {
            entity.Property(e => e.Id).HasColumnName("ReviewID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.RatingId).HasColumnName("RatingID");
        });

        modelBuilder.Entity<Feedback>(e => {
            e.Property(x => x.Id).HasColumnName("FeedbackID");
            e.Property(x => x.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<FileType>(e => e.Property(x => x.Id).HasColumnName("TypeID"));
        modelBuilder.Entity<SystemFile>(e => {
            e.Property(x => x.Id).HasColumnName("FileID");
        });

        modelBuilder.Entity<AuditLog>(e => {
            e.Property(x => x.Id).HasColumnName("AuditLogID");
            e.Property(x => x.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<ActivityLog>(e => {
            e.Property(x => x.Id).HasColumnName("ActivityLogID");
            e.Property(x => x.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<ErrorLog>(e => {
            e.Property(x => x.Id).HasColumnName("ErrorLogID");
        });

        modelBuilder.Entity<SystemSetting>(e => e.Property(x => x.Id).HasColumnName("SettingID"));
        modelBuilder.Entity<Token>(e => e.Property(x => x.Id).HasColumnName("TokenID"));

        modelBuilder.Entity<InstructorCourseApplication>(entity => {
            entity.Property(e => e.Id).HasColumnName("ApplicationID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
        });

        modelBuilder.Entity<CourseForumMessage>(entity => {
            entity.ToTable("CourseForumMessages");
            entity.Property(e => e.Id).HasColumnName("MessageID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<Class>()
            .HasOne(c => c.Instructor)
            .WithMany(u => u.Classes)
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Semester>()
            .HasOne(s => s.Year)
            .WithMany(y => y.Semesters)
            .HasForeignKey(s => s.YearId);

        modelBuilder.Entity<Block>()
            .HasOne(b => b.Semester)
            .WithMany(s => s.Blocks)
            .HasForeignKey(b => b.SemesterId);

        modelBuilder.Entity<Class>()
            .HasOne(c => c.Block)
            .WithMany(b => b.Classes)
            .HasForeignKey(c => c.BlockId);

        modelBuilder.Entity<Course>()
            .HasOne(c => c.Block)
            .WithMany(b => b.Courses)
            .HasForeignKey(c => c.BlockId);
    }
}
