-- SQL script to associate Quizzes (Exams) with Lessons
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Exams') AND name = 'LessonID')
BEGIN
    ALTER TABLE Exams ADD LessonID INT NULL;
    
    -- Optional: Add Foreign Key
    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('Lessons') AND type = 'U')
    BEGIN
        ALTER TABLE Exams ADD CONSTRAINT FK_Exams_Lessons FOREIGN KEY (LessonID) REFERENCES Lessons(LessonID);
    END
END

-- Add CreatedAt to ClassStudents for Revenue tracking
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClassStudents') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE ClassStudents ADD CreatedAt DATETIME NOT NULL DEFAULT GETDATE();
END

PRINT 'SQL update for Lesson-Quiz association and Revenue tracking completed.';
