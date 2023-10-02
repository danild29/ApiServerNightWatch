CREATE PROCEDURE [dbo].[spTeam_Insert]
	@Id int output,
	@Name nvarchar(50),
	@CaptainId int,
	@Password nvarchar(33)
AS
begin
	insert into [dbo].[Team] (Name, CaptainId, Password)
                     values (@Name, @CaptainId, @Password);

	set @Id = SCOPE_IDENTITY();

	return 1;
end
