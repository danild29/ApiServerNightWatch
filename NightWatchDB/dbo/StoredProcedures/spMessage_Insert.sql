CREATE PROCEDURE [dbo].[spMessage_Insert]
	@Id int output,
	@FromTeam INT,
	@ToEvent INT,
	@Content NVARCHAR(MAX),
	@CreatedAt DATETIME
AS
begin
	insert into [dbo].[Message] (FromTeam, ToEvent, Content, CreatedAt)
                     values (@FromTeam, @ToEvent, @Content, @CreatedAt);

	set @Id = SCOPE_IDENTITY();

	return 1;
end