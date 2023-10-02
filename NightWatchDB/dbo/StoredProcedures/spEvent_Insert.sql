CREATE PROCEDURE [dbo].[spEvent_Insert]	
	@Id int output,
	@Name NVARCHAR(128),
	@Password nvarchar(33),
	@CreatorId int,
    @StartAt DATETIME, 
    @Duration int, 
    @Description NVARCHAR(max)
AS
begin
	insert into [dbo].[Event] (Name, Password, CreatorId, StartAt, Duration, Description)
                     values (@Name, @Password, @CreatorId, @StartAt, @Duration, @Description);

	set @Id = SCOPE_IDENTITY();

	return 1;
end