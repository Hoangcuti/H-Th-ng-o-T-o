-- SQL Script to update database for Year-Semester-Block hierarchy
-- Created based on Implementation Plan V3

-- 1. Create AcademicYears table
CREATE TABLE [AcademicYears] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [YearNumber] INT NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_AcademicYears] PRIMARY KEY ([Id])
);

-- 2. Create Semesters table
CREATE TABLE [Semesters] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [SemesterName] NVARCHAR(20) NOT NULL,
    [YearId] INT NOT NULL,
    [IsCurrent] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Semesters] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Semesters_AcademicYears_YearId] FOREIGN KEY ([YearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE CASCADE
);

-- 3. Create Blocks table
CREATE TABLE [Blocks] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BlockName] NVARCHAR(20) NOT NULL,
    [SemesterId] INT NOT NULL,
    [IsCurrent] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Blocks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Blocks_Semesters_SemesterId] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters] ([Id]) ON DELETE CASCADE
);

-- 4. Update Classes table
ALTER TABLE [Classes] ADD [ClassCode] NVARCHAR(100) NOT NULL DEFAULT '';
ALTER TABLE [Classes] ADD [BlockId] INT NULL;
CREATE INDEX [IX_Classes_BlockId] ON [Classes] ([BlockId]);
ALTER TABLE [Classes] ADD CONSTRAINT [FK_Classes_Blocks_BlockId] FOREIGN KEY ([BlockId]) REFERENCES [Blocks] ([Id]) ON DELETE NO ACTION;

-- 5. Update Courses table
ALTER TABLE [Courses] ADD [BlockId] INT NULL;
CREATE INDEX [IX_Courses_BlockId] ON [Courses] ([BlockId]);
ALTER TABLE [Courses] ADD CONSTRAINT [FK_Courses_Blocks_BlockId] FOREIGN KEY ([BlockId]) REFERENCES [Blocks] ([Id]) ON DELETE NO ACTION;

-- 6. Update Status defaults for existing tables (Optional migration logic)
-- Note: Already updated in C# models, this script ensures DB alignment if needed.
-- UPDATE [Lessons] SET [Status] = 'Pending' WHERE [Status] = 'Approved' AND CreatedByUserId IS NOT NULL;
-- UPDATE [Exams] SET [Status] = 'Pending' WHERE [Status] = 'Approved' AND CreatedByUserId IS NOT NULL;
-- UPDATE [Assignments] SET [Status] = 'Pending' WHERE [Status] = 'Approved' AND CreatedByUserId IS NOT NULL;

PRINT 'Database update script finished successfully.';
