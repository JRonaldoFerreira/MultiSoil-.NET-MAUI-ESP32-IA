namespace MultiSoil_EdgeAI.Models;

public class AuthResult
{
    public bool Success { get; }
    public string Message { get; }
    public User? User { get; }

    public AuthResult(bool success, string message, User? user = null)
    {
        Success = success;
        Message = message;
        User = user;
    }

    public static AuthResult Ok(User u) => new(true, "Autenticado com sucesso.", u);
    public static AuthResult Error(string msg) => new(false, msg);
}
