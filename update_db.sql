    -- SQL script to ensure database column names match the EF Core mapping (TableNameID format)
    -- Run this to synchronize your database schema with the new C# mappings.

    -- 1. Core Models
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Roles') AND name = 'Id') EXEC sp_rename 'Roles.Id', 'RoleID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Permissions') AND name = 'Id') EXEC sp_rename 'Permissions.Id', 'PermissionID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Departments') AND name = 'Id') EXEC sp_rename 'Departments.Id', 'DepartmentID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Positions') AND name = 'Id') EXEC sp_rename 'Positions.Id', 'PositionID', 'COLUMN';

    -- Users table
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Id') EXEC sp_rename 'Users.Id', 'UserID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'DepartmentId') EXEC sp_rename 'Users.DepartmentId', 'DepartmentID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PositionId') EXEC sp_rename 'Users.PositionId', 'PositionID', 'COLUMN';

    -- UserRoles (Join table)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserRoles') AND name = 'UserId') EXEC sp_rename 'UserRoles.UserId', 'UserID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserRoles') AND name = 'RoleId') EXEC sp_rename 'UserRoles.RoleId', 'RoleID', 'COLUMN';

    -- RolePermissions (Join table)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RolePermissions') AND name = 'RoleId') EXEC sp_rename 'RolePermissions.RoleId', 'RoleID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RolePermissions') AND name = 'PermissionId') EXEC sp_rename 'RolePermissions.PermissionId', 'PermissionID', 'COLUMN';

    -- 2. Course Models
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CourseCategories') AND name = 'Id') EXEC sp_rename 'CourseCategories.Id', 'CategoryID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CourseLevels') AND name = 'Id') EXEC sp_rename 'CourseLevels.Id', 'LevelID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CourseTags') AND name = 'Id') EXEC sp_rename 'CourseTags.Id', 'TagID', 'COLUMN';

    -- Courses table
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Courses') AND name = 'Id') EXEC sp_rename 'Courses.Id', 'CourseID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Courses') AND name = 'CategoryId') EXEC sp_rename 'Courses.CategoryId', 'CategoryID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Courses') AND name = 'LevelId') EXEC sp_rename 'Courses.LevelId', 'LevelID', 'COLUMN';
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Courses') AND name = 'Price') ALTER TABLE Courses ADD Price DECIMAL(18,2) NOT NULL DEFAULT 0;

    -- CourseInstructors table
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CourseInstructors') AND name = 'CourseId') EXEC sp_rename 'CourseInstructors.CourseId', 'CourseID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CourseInstructors') AND name = 'UserId') EXEC sp_rename 'CourseInstructors.UserId', 'UserID', 'COLUMN';

    -- 3. Class Models (Classes, Students, Rooms)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClassRooms') AND name = 'Id') EXEC sp_rename 'ClassRooms.Id', 'RoomID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClassStatus') AND name = 'Id') EXEC sp_rename 'ClassStatus.Id', 'StatusID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Classes') AND name = 'Id') EXEC sp_rename 'Classes.Id', 'ClassID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Classes') AND name = 'CourseId') EXEC sp_rename 'Classes.CourseId', 'CourseID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Classes') AND name = 'InstructorId') EXEC sp_rename 'Classes.InstructorId', 'InstructorID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClassStudents') AND name = 'ClassId') EXEC sp_rename 'ClassStudents.ClassId', 'ClassID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ClassStudents') AND name = 'UserId') EXEC sp_rename 'ClassStudents.UserId', 'UserID', 'COLUMN';

    -- 4. Lesson & Progress Models
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Lessons') AND name = 'Id') EXEC sp_rename 'Lessons.Id', 'LessonID', 'COLUMN';
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Lessons') AND name = 'CourseId') EXEC sp_rename 'Lessons.CourseId', 'CourseID', 'COLUMN';

    -- LearningProgress (Ensures table and columns exist)
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('LearningProgress') AND type = 'U')
    BEGIN
        CREATE TABLE LearningProgress (
            UserID INT NOT NULL,
            LessonID INT NOT NULL,
            Completed BIT NOT NULL DEFAULT 0,
            CompletionDate DATETIME NULL,
            PRIMARY KEY (UserID, LessonID)
        );
    END
    ELSE
    BEGIN
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LearningProgress') AND name = 'UserId') EXEC sp_rename 'LearningProgress.UserId', 'UserID', 'COLUMN';
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LearningProgress') AND name = 'LessonId') EXEC sp_rename 'LearningProgress.LessonId', 'LessonID', 'COLUMN';
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LearningProgress') AND name = 'Completed') ALTER TABLE LearningProgress ADD Completed BIT NOT NULL DEFAULT 0;
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LearningProgress') AND name = 'CompletionDate') ALTER TABLE LearningProgress ADD CompletionDate DATETIME NULL;
    END

    PRINT 'Database schema update completed successfully.';
