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
    public DbSet<ClassStatus> ClassStatus => Set<ClassStatus>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
    public DbSet<LearningProgress> LearningProgress => Set<LearningProgress>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<ClassStudent> ClassStudents => Set<ClassStudent>();
    public DbSet<ClassAttendance> ClassAttendance => Set<ClassAttendance>();
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
    public DbSet<LessonProgress> LessonProgress => Set<LessonProgress>();
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
        modelBuilder.Entity<RolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });
        modelBuilder.Entity<CourseInstructor>().HasKey(x => new { x.CourseId, x.UserId });
        modelBuilder.Entity<CourseTagMapping>().HasKey(x => new { x.CourseId, x.TagId });
        modelBuilder.Entity<CoursePrerequisite>().HasKey(x => new { x.CourseId, x.PrerequisiteId });
        modelBuilder.Entity<ClassStudent>().HasKey(x => new { x.ClassId, x.UserId });
        modelBuilder.Entity<UserNotification>().HasKey(x => new { x.UserId, x.NotificationId });
        modelBuilder.Entity<LearningProgress>().HasKey(lp => new { lp.UserId, lp.LessonId });

        // Ánh xạ chính xác các cột theo Database của bạn
        modelBuilder.Entity<User>(entity => {
            entity.ToTable("Users");
            entity.Property(e => e.Id).HasColumnName("UserID");
            entity.Property(e => e.StudentCode).HasColumnName("StudentCode");
        });

        modelBuilder.Entity<Course>(entity => {
            entity.ToTable("Courses");
            entity.Property(e => e.Id).HasColumnName("CourseID");
            entity.Property(e => e.Price).HasColumnName("Price");
        });

        modelBuilder.Entity<Lesson>(entity => {
            entity.ToTable("Lessons");
            entity.Property(e => e.Id).HasColumnName("LessonID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
        });

        modelBuilder.Entity<Class>(entity => {
            entity.ToTable("Classes");
            entity.Property(e => e.Id).HasColumnName("ClassID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.InstructorId).HasColumnName("InstructorID");
        });

        modelBuilder.Entity<LearningProgress>(entity => {
            entity.ToTable("LearningProgress");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.LessonId).HasColumnName("LessonID");
            entity.Property(e => e.Completed).HasColumnName("Completed");
            entity.Property(e => e.CompletionDate).HasColumnName("CompletionDate");
        });

        modelBuilder.Entity<ClassStudent>(entity => {
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
        });

        modelBuilder.Entity<CourseInstructor>(entity => {
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
        });

        modelBuilder.Entity<InstructorCourseApplication>(entity => {
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
        });

        modelBuilder.Entity<Exam>(entity => {
            entity.Property(e => e.Id).HasColumnName("ExamID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
        });

        modelBuilder.Entity<ClassAttendance>(entity => {
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<UserCertificate>(entity => {
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<Class>()
            .HasOne(c => c.Instructor)
            .WithMany(u => u.Classes)
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
