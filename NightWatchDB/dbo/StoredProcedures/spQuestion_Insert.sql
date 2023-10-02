CREATE PROCEDURE [dbo].[spQuestion_Insert]
	@Id int output,
	@Answer nvarchar(128),
	@Content varbinary(max),
    @EventId int,
	@ContentType nvarchar(4)
AS
begin
	INSERT INTO [dbo].[Question] (Answer, Content, ContentType, EventId)
						  values(@Answer, @Content, @ContentType, @EventId);

	set @Id = SCOPE_IDENTITY();
	return 1;
end
