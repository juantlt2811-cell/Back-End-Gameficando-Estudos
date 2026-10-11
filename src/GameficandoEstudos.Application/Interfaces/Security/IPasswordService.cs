namespace GameficandoEstudos.Application.Interfaces
{
    public interface IPasswordService
    {
        string HashPassword(string senhaPlana);
        bool VerifyPassword(string senhaHashArmazenado, string senhaDigitada);
    }
}