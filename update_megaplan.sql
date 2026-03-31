-- 1. CẬP NHẬT BẢNG BÀI GIẢNG (LESSONS)
ALTER TABLE [Lessons] ADD [CreatedByUserId] INT NULL;
ALTER TABLE [Lessons] ADD [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE();
ALTER TABLE [Lessons] ADD [UpdatedByUserId] INT NULL;
ALTER TABLE [Lessons] ADD [UpdatedAt] DATETIME NULL;

ALTER TABLE [Lessons] ADD CONSTRAINT [FK_Lessons_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users]([UserID]);
ALTER TABLE [Lessons] ADD CONSTRAINT [FK_Lessons_Users_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [Users]([UserID]);

-- 2. CẬP NHẬT BẢNG BÀI QUIZ (EXAMS)
ALTER TABLE [Exams] ADD [Status] NVARCHAR(20) NOT NULL DEFAULT 'Approved';
ALTER TABLE [Exams] ADD [CreatedByUserId] INT NULL;
ALTER TABLE [Exams] ADD [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE();
ALTER TABLE [Exams] ADD [UpdatedByUserId] INT NULL;
ALTER TABLE [Exams] ADD [UpdatedAt] DATETIME NULL;

ALTER TABLE [Exams] ADD CONSTRAINT [FK_Exams_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users]([UserID]);
ALTER TABLE [Exams] ADD CONSTRAINT [FK_Exams_Users_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [Users]([UserID]);

-- 3. TẠO BẢNG BÀI TẬP LỚN (ASSIGNMENTS)
CREATE TABLE [Assignments] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [CourseId] INT NOT NULL,
    [LessonId] INT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(2000) NULL,
    [FileUrl] NVARCHAR(500) NULL,
    [DueDate] DATETIME NULL,
    [MaxScore] INT NOT NULL DEFAULT 100,
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'Approved',
    [CreatedByUserId] INT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedByUserId] INT NULL,
    [UpdatedAt] DATETIME NULL,
    CONSTRAINT [PK_Assignments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Assignments_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses]([CourseID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assignments_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons]([LessonID]),
    CONSTRAINT [FK_Assignments_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users]([UserID]),
    CONSTRAINT [FK_Assignments_Users_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [Users]([UserID])
);

-- 4. TẠO BẢNG NỘP BÀI TẬP VÀ CHẤM ĐIỂM (ASSIGNMENTSUBMISSIONS)
CREATE TABLE [AssignmentSubmissions] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [AssignmentId] INT NOT NULL,
    [StudentId] INT NOT NULL,
    [Content] NVARCHAR(2000) NULL,
    [FileUrl] NVARCHAR(500) NULL,
    [SubmittedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [Score] INT NULL,
    [Feedback] NVARCHAR(1000) NULL,
    [GradedByUserId] INT NULL,
    [GradedAt] DATETIME NULL,
    CONSTRAINT [PK_AssignmentSubmissions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AssignmentSubmissions_Assignments_AssignmentId] FOREIGN KEY ([AssignmentId]) REFERENCES [Assignments]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AssignmentSubmissions_Users_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Users]([UserID]) ON DELETE CASCADE,
    CONSTRAINT [FK_AssignmentSubmissions_Users_GradedByUserId] FOREIGN KEY ([GradedByUserId]) REFERENCES [Users]([UserID])
);
GO
