﻿CREATE TABLE [dbo].[Team]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [CaptainId] INT NULL,
    [Name] NVARCHAR(50) NOT NULL, 
    [Password] NVARCHAR(33) NOT NULL,
    [EventId] INT NULL,
    CONSTRAINT [TeamOnEvent] FOREIGN KEY ([EventId]) REFERENCES [Event]([Id]) ON DELETE SET NULL ON UPDATE SET NULL
)
