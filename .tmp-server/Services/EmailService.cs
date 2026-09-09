﻿﻿﻿﻿﻿﻿﻿﻿using System.Net;
using System.Net.Mail;

namespace AuditApiServer.Services;

public class EmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromAddress;
    private readonly string _fromDisplayName;
    private readonly bool _enabled;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        var section = configuration.GetSection("EmailSettings");
        _enabled = bool.TryParse(section["Enabled"], out var en) ? en : true;
        _smtpServer = section["SmtpServer"] ?? "smtp.126.com";
        _smtpPort = int.TryParse(section["SmtpPort"], out var port) ? port : 465;
        _smtpUsername = section["SmtpUsername"] ?? "";
        // V2-C-11 修复：优先从环境变量 Email__SmtpPassword 读取 SMTP 密码，
        // appsettings.json 中不再明文存储凭据；环境变量缺失时回退到配置（应保持为空）。
        _smtpPassword = Environment.GetEnvironmentVariable("Email__SmtpPassword") ?? section["SmtpPassword"] ?? "";
        _fromAddress = section["FromAddress"] ?? "";
        _fromDisplayName = section["FromDisplayName"] ?? "审计系统";
        // 两者都为空时仅记录警告，不抛异常（部署可能不需要邮件功能，SendEmailAsync 会进入测试模式）
        if (string.IsNullOrEmpty(_smtpPassword))
        {
            _logger.LogWarning("SMTP 密码未配置：环境变量 Email__SmtpPassword 与 appsettings EmailSettings:SmtpPassword 均为空，邮件功能将以测试模式运行");
        }
    }

    public async Task SendVerificationCodeAsync(string toEmail, string code)
    {
        var subject = "验证码 - 审计系统";
        var body = "您正在进行安全验证，本次验证码为：" + code + "，有效期5分钟。\r\n\r\n如果您没有进行此操作，请忽略此邮件。";
        await SendEmailAsync(toEmail, subject, body, false);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        // 配置 Enabled=false 或 SMTP 凭证未配置时，仅记录日志（与 SmsService 行为一致），不实际发送邮件
        if (!_enabled || string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
        {
            _logger.LogInformation("邮箱验证码（测试模式，未发送）: to={ToEmail} subject={Subject}", MaskEmail(toEmail), subject);
            _logger.LogDebug("邮箱验证码（测试模式，正文）: body={Body}", body);
            await Task.CompletedTask;
            return;
        }

        try
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true,
                Timeout = 10000 // 10 秒超时，防止网络不可达时无限挂起
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromAddress, _fromDisplayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml,
                Priority = MailPriority.Normal
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);

            _logger.LogInformation("邮件发送成功: to={ToEmail} subject={Subject}", MaskEmail(toEmail), subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "邮件发送失败: to={ToEmail} subject={Subject}", MaskEmail(toEmail), subject);
            throw;
        }
    }

    /// <summary>
    /// 脱敏邮箱：保留前 3 后 4，中间以 * 替代。
    /// </summary>
    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email)) return "";
        if (email.Length <= 7) return new string('*', email.Length);
        return email.Substring(0, 3) + new string('*', email.Length - 7) + email.Substring(email.Length - 4);
    }
}