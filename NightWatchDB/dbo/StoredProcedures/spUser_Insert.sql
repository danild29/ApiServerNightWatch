
CREATE PROCEDURE [dbo].[spUser_Insert]
	@Id int output,
	@Name nvarchar(50),
	@Email nvarchar(70),
	@Password nvarchar(33)
AS
begin
	insert into [dbo].[User] (Name, Email, Password)
                    values (@Name, @Email, @Password);

	set @Id = SCOPE_IDENTITY();

	return 1;
end
