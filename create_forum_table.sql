-- SQL Script to create CourseForumMessages table
-- Please run this script in your SQL Server database (db46194)

CREATE TABLE [dbo].[CourseForumMessages] (
    [MessageID] INT IDENTITY(1,1) NOT NULL,
    [CourseID] INT NOT NULL,
    [UserID] INT NOT NULL,
    [Content] NVARCHAR(2000) NOT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT (GETDATE()),
    [ParentMessageId] INT NULL,
    
    CONSTRAINT [PK_CourseForumMessages] PRIMARY KEY CLUSTERED ([MessageID] ASC),
    CONSTRAINT [FK_CourseForumMessages_Courses] FOREIGN KEY ([CourseID]) REFERENCES [dbo].[Courses] ([CourseID]) ON DELETE CASCADE,
    CONSTRAINT [FK_CourseForumMessages_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID]) ON DELETE CASCADE,
    CONSTRAINT [FK_CourseForumMessages_Parent] FOREIGN KEY ([ParentMessageId]) REFERENCES [dbo].[CourseForumMessages] ([MessageID])
);

CREATE INDEX [IX_CourseForumMessages_CourseID] ON [dbo].[CourseForumMessages] ([CourseID]);
CREATE INDEX [IX_CourseForumMessages_UserID] ON [dbo].[CourseForumMessages] ([UserID]);
GO
