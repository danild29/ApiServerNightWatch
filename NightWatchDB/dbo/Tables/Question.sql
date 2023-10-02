CREATE TABLE [dbo].[Question]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
	[EventId] INT NOT NULL , 
	[Answer] nvarchar(128) NOT NULL,
	[Question] nvarchar(max) NOT NULL,
	[ContentType] nvarchar(4),
	[Content] varbinary(max),
	CONSTRAINT [QuestionOnEvent] FOREIGN KEY ([EventId]) REFERENCES [Event]([Id]) ON DELETE CASCADE ON UPDATE CASCADE 
)
