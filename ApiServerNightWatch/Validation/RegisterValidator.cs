using DataAccess.DbModels.Dtos;
using FluentValidation;
namespace ApiServerNightWatch.Validation;


public class RegisterValidator : AbstractValidator<UserRegisterDto>
{
    private const int MinLenPassword = 4;
    private const int MaxLenPassword = 32;

    private const int MinLenName = 4;
    private const int MaxLenName = 50;

    private const int MinLenEmail = 1;
    private const int MaxLenEmail = 75;

    public RegisterValidator()
    {
        RuleFor(u => u.Name)
            .NotEmpty().WithMessage("Ник не должен быть пустым")
            .Length(MinLenName, MaxLenName).WithMessage(x =>  $"ник должен содержать от {MinLenName} до {MaxLenName} символов");

        RuleFor(u => u.Email)
            //.EmailAddress().WithMessage("Неверный email адрес")
            .NotEmpty().WithMessage("Email не должен быть пустым")
            .Length(MinLenEmail, MaxLenEmail).WithMessage($"Email должен содержать от {MinLenName} до {MaxLenName} символов");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Пароль не должен быть пустым")
            .Length(MinLenPassword, MaxLenPassword).WithMessage($"Пароль должен содержать от {MinLenName} до {MaxLenName} символов");

    }


    protected bool BeAValidName(string name)
    {
        name = name.Replace(" ", "");
        name = name.Replace("-", "");
        return name.All(char.IsLetter);
    }

}