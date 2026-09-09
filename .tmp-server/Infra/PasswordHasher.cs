﻿using System.Security.Cryptography;
using System.Text;

namespace AuditApiServer.Infra;

/// <summary>
/// 密码哈希工具。安全审计修复（High）：升级为 PBKDF2-HMACSHA256（100000 次迭代），并保留对旧 SHA256+Salt 哈希的验证兼容。
///
/// 哈希格式：
/// - 新格式（PBKDF2）：pbkdf2$100000$base64(salt)$base64(hash)
/// - 旧格式（SHA256+Salt）：base64(SHA256(salt + SHA256(input)))，由 VerifyPassword 自动识别
///
/// 约定：clientInput 参数始终是客户端发送的 Base64(SHA256(明文))（由客户端 Encrypts.SHA256Encrypt 生成）。
/// 服务端直接用此值作为 PBKDF2 输入，不再做任何规范化。admin 和普通用户使用完全相同的验证方式。
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;
    private const string Pbkdf2Prefix = "pbkdf2";

    /// <summary>生成随机 Salt（Base64，16 字节）</summary>
    public static string GenerateSalt()
    {
        var bytes = RandomNumberGenerator.GetBytes(SaltSize);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// 计算密码哈希（PBKDF2-HMACSHA256，100000 次迭代）。
    /// 输出格式：pbkdf2$100000$base64(salt)$base64(hash)
    /// 兼容性：保留 saltBase64 参数以维持调用方签名不变；salt 同时嵌入到返回字符串中，VerifyPassword 时从字符串解析。
    /// 约定：clientInput 是客户端发送的 Base64(SHA256(明文))，服务端直接用此值作为 PBKDF2 输入。
    /// </summary>
    public static string HashPassword(string clientInput, string saltBase64)
    {
        if (string.IsNullOrEmpty(clientInput)) throw new ArgumentException("clientInput 不能为空", nameof(clientInput));
        if (string.IsNullOrEmpty(saltBase64)) throw new ArgumentException("saltBase64 不能为空", nameof(saltBase64));

        var salt = Convert.FromBase64String(saltBase64);
        using var pbkdf2 = new Rfc2898DeriveBytes(clientInput, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);

        return $"{Pbkdf2Prefix}${Iterations}${saltBase64}${Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// 验证密码（固定时间比较，避免时序攻击）。
    /// 自动识别 storedHash 格式：
    /// - pbkdf2$100000$base64(salt)$base64(hash)：用 PBKDF2 验证（salt 从 storedHash 解析）
    /// - 其他：用旧 SHA256+Salt 算法验证（用 saltBase64 参数），兼容已有用户数据
    /// </summary>
    public static bool VerifyPassword(string clientInput, string storedHash, string saltBase64)
    {
        if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(clientInput)) return false;

        // 检测 PBKDF2 格式
        if (storedHash.StartsWith(Pbkdf2Prefix + "$", StringComparison.Ordinal))
        {
            return VerifyPbkdf2(clientInput, storedHash);
        }

        // 旧 SHA256+Salt 格式
        if (string.IsNullOrEmpty(saltBase64)) return false;
        var computed = HashPasswordLegacy(clientInput, saltBase64);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(storedHash));
    }

    /// <summary>
    /// 用 PBKDF2 算法验证密码。解析 storedHash 中的 iterations/salt/hash，重新计算并固定时间比较。
    /// clientInput 是客户端发送的 Base64(SHA256(明文))，直接作为 PBKDF2 输入。
    /// admin 和普通用户使用完全相同的验证方式。
    /// </summary>
    private static bool VerifyPbkdf2(string clientInput, string storedHash)
    {
        var parts = storedHash.Split('$');
        if (parts.Length != 4 || parts[0] != Pbkdf2Prefix) return false;
        if (!int.TryParse(parts[1], out var iterations) || iterations <= 0) return false;
        var saltBase64 = parts[2];
        var expectedHashBase64 = parts[3];

        byte[] salt;
        try { salt = Convert.FromBase64String(saltBase64); }
        catch { return false; }
        byte[] expected;
        try { expected = Convert.FromBase64String(expectedHashBase64); }
        catch { return false; }

        // 客户端发送的是 Base64(SHA256(明文))（由 Encrypts.SHA256Encrypt 生成），直接作为 PBKDF2 输入。
        using var pbkdf2 = new Rfc2898DeriveBytes(clientInput, salt, iterations, HashAlgorithmName.SHA256);
        var actual = pbkdf2.GetBytes(expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    /// <summary>
    /// 旧 SHA256+Salt 算法（用于验证已存在的 SHA256 格式哈希）。
    /// 服务端先做 SHA256(clientInput) 规范化为中间哈希 H，再做 Base64(SHA256(Salt + H))。
    /// </summary>
    private static string HashPasswordLegacy(string clientInput, string saltBase64)
    {
        var salt = Convert.FromBase64String(saltBase64);
        var normalizedHash = SHA256.HashData(Encoding.UTF8.GetBytes(clientInput));
        using var sha = SHA256.Create();
        var combined = new byte[salt.Length + normalizedHash.Length];
        Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
        Buffer.BlockCopy(normalizedHash, 0, combined, salt.Length, normalizedHash.Length);
        return Convert.ToBase64String(sha.ComputeHash(combined));
    }
}
