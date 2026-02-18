using System.Security.Cryptography;
using System.Text;

namespace Scanitech_Logic.Security;

public interface IPasswordProtector
{
    string ProtectIfNeeded(string value);
    string Unprotect(string value);
}

public sealed class PasswordProtector : IPasswordProtector
{
    private const string Prefix = "enc:";

    public string ProtectIfNeeded(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.StartsWith(Prefix)) return value;

        var data = Encoding.UTF8.GetBytes(value);
        var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.LocalMachine);
        return $"{Prefix}{Convert.ToBase64String(encrypted)}";
    }

    public string Unprotect(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(Prefix)) return value;

        var base64Data = value[Prefix.Length..];
        var data = Convert.FromBase64String(base64Data);
        var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.LocalMachine);
        return Encoding.UTF8.GetString(decrypted);
    }
}