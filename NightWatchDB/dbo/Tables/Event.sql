CREATE TABLE [dbo].[Event]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] NVARCHAR(128) NOT NULL, 
    [Password] NVARCHAR(32) NOT NULL,
    [CreatorId] INT NOT NULL,
    [StartAt] DATETIME NOT NULL, 
    [Duration] int NULL, 
    [Description] NVARCHAR(max) NULL, 
    CONSTRAINT [EventOnUser] FOREIGN KEY ([CreatorId]) REFERENCES [User]([Id])
)
