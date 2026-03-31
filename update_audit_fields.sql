-- 1. Thêm cột Status cho các bảng nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Lessons') AND name = 'Status')
    ALTER TABLE Lessons ADD Status NVARCHAR(20) DEFAULT 'Approved' NOT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Exams') AND name = 'Status')
    ALTER TABLE Exams ADD Status NVARCHAR(20) DEFAULT 'Approved' NOT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assignments') AND name = 'Status')
    ALTER TABLE Assignments ADD Status NVARCHAR(20) DEFAULT 'Approved' NOT NULL;

-- 2. Thêm cột Auditing (Người tạo/người sửa)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Lessons') AND name = 'CreatedByUserId')
    ALTER TABLE Lessons ADD CreatedByUserId INT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Exams') AND name = 'CreatedByUserId')
    ALTER TABLE Exams ADD CreatedByUserId INT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assignments') AND name = 'CreatedByUserId')
    ALTER TABLE Assignments ADD CreatedByUserId INT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Lessons') AND name = 'UpdatedByUserId')
    ALTER TABLE Lessons ADD UpdatedByUserId INT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Exams') AND name = 'UpdatedByUserId')
    ALTER TABLE Exams ADD UpdatedByUserId INT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assignments') AND name = 'UpdatedByUserId')
    ALTER TABLE Assignments ADD UpdatedByUserId INT NULL;

-- 3. Đảm bảo bảng AssignmentSubmission có trường Điểm (Grade/Score)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AssignmentSubmissions') AND name = 'Score')
    ALTER TABLE AssignmentSubmissions ADD Score FLOAT DEFAULT 0;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AssignmentSubmissions') AND name = 'Status')
    ALTER TABLE AssignmentSubmissions ADD Status NVARCHAR(20) DEFAULT 'Pending' NOT NULL;

GO
