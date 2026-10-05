using System.Security.Cryptography;
using System.Text;

namespace Multitool.Services;

public static class GeneratorTools
{
    private static readonly char[] Lower = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
    private static readonly char[] Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private static readonly char[] Digits = "0123456789".ToCharArray();
    private static readonly char[] Symbols = "!@#$%^&*()-_=+[]{};:,.<>?".ToCharArray();

    public static string NewPassword(int length = 20, bool upper = true, bool digits = true, bool symbols = true)
    {
        var pool = new List<char>(Lower);
        if (upper) pool.AddRange(Upper);
        if (digits) pool.AddRange(Digits);
        if (symbols) pool.AddRange(Symbols);
        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
            sb.Append(pool[RandomNumberGenerator.GetInt32(pool.Count)]);
        return sb.ToString();
    }

    public static string Sha256(string input)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input))).ToLowerInvariant();

    public static string Md5(string input)
        => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(input))).ToLowerInvariant();

    public static string ToBase64(string input)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(input));

    public static string FromBase64(string input)
    {
        try { return Encoding.UTF8.GetString(Convert.FromBase64String(input.Trim())); }
        catch { return "Base64 invalide."; }
    }
}
