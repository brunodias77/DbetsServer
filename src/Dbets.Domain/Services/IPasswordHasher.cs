namespace Dbets.Domain.Services;

/// <summary>
/// Interface para serviços de hash de senha
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Gera o hash de uma senha
    /// </summary>
    /// <param name="password">Senha em texto plano</param>
    /// <returns>Hash da senha</returns>
    string HashPassword(string password);
    
    /// <summary>
    /// Verifica se uma senha corresponde ao hash
    /// </summary>
    /// <param name="password">Senha em texto plano</param>
    /// <param name="hashedPassword">Hash da senha</param>
    /// <returns>True se a senha corresponde ao hash</returns>
    bool VerifyPassword(string password, string hashedPassword);
}