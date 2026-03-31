-- SQL script to update ExamAttempt and ExamResult for Midnight Pro Quiz System

-- 1. Update ExamAttempt table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExamAttempts') AND name = 'RemainingSeconds')
BEGIN
    ALTER TABLE ExamAttempts ADD RemainingSeconds INT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExamAttempts') AND name = 'AnswersJson')
BEGIN
    ALTER TABLE ExamAttempts ADD AnswersJson NVARCHAR(MAX) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExamAttempts') AND name = 'Status')
BEGIN
    ALTER TABLE ExamAttempts ADD Status NVARCHAR(50) NOT NULL DEFAULT 'Started';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExamAttempts') AND name = 'StartedAt')
BEGIN
    ALTER TABLE ExamAttempts ADD StartedAt DATETIME NOT NULL DEFAULT GETDATE();
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExamAttempts') AND name = 'CompletedAt')
BEGIN
    ALTER TABLE ExamAttempts ADD CompletedAt DATETIME NULL;
END

-- 2. Update ExamResult table
-- Note: Score needs to be float/decimal for 10.0 scale
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExamResults') AND name = 'Score')
BEGIN
    ALTER TABLE ExamResults ALTER COLUMN Score FLOAT NOT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExamResults') AND name = 'CorrectCount')
BEGIN
    ALTER TABLE ExamResults ADD CorrectCount INT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExamResults') AND name = 'TotalQuestions')
BEGIN
    ALTER TABLE ExamResults ADD TotalQuestions INT NOT NULL DEFAULT 0;
END

PRINT 'Exam persistence and scoring schema updated successfully.';
