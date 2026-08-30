﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Auditai.DTO;
using Newtonsoft.Json;

namespace Auditai.Util;

public static class TokenTimer
{
	private static UserToken _token;

	/// <summary>保护 _token 读写的锁（Cookie 保留逻辑 + 赋值需原子）</summary>
	private static readonly object _tokenLock = new object();

	private const string COOKIE_MEMERY_FILE = "./config/cookie.json";

	public static UserToken Token
	{
		get
		{
			return _token;
		}
		set
		{
			// 不允许将 Token 设为 null，避免后续 API 调用 NRE
			if (value == null) return;
			lock (_tokenLock)
			{
				// 服务端返回的 UserToken 不包含 Cookie，保留旧 Cookie 避免丢失机器指纹
				if (value.Cookie == null && _token?.Cookie != null)
				{
					value.Cookie = _token.Cookie;
				}
				_token = value;
			}
			SaveCookieToMachine();
		}
	}

	public static TokenUpdater TokenUpdater { get; set; }

	public static LoginInfo LoginInfo { get; set; }

	static TokenTimer()
	{
		Token = new UserToken();
		Token.Cookie = new MachineCookie();
		ReadCookieFromMachine();
		TokenUpdater = new TokenUpdater();
		TokenUpdater.Interval = TimeSpan.FromSeconds(60.0);
		LoginInfo = new LoginInfo();
	}

	public static void ReadCookieFromMachine()
	{
		try
		{
			if (Token == null || !File.Exists("./config/cookie.json"))
			{
				return;
			}
			using FileStream fileStream = new FileStream("./config/cookie.json", FileMode.Open, FileAccess.Read);
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, array.Length);
			string keyBase = Encrypts.CreateBase64Key(MachineCode.Code);
			byte[] bytes = Encrypts.AesDecrypt(array, keyBase);
			string @string = Encoding.Default.GetString(bytes);
			MachineCookie cookie = JsonConvert.DeserializeObject<MachineCookie>(@string);
			Token.Cookie = cookie;
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[TokenTimer] ReadCookieFromMachine failed: {ex.Message}");
		}
	}

	public static void SaveCookieToMachine()
	{
		string tmpFile = null;
		try
		{
			if (Token?.Cookie == null)
			{
				return;
			}
			string s = JsonConvert.SerializeObject(Token.Cookie);
			byte[] bytes = Encoding.Default.GetBytes(s);
			string keyBase = Encrypts.CreateBase64Key(MachineCode.Code);
			byte[] array = Encrypts.AesEncrypt(bytes, keyBase);
			// 原子写入：先写同目录临时文件，再替换目标，避免写入中途崩溃/断电导致 cookie.json 损坏
			tmpFile = COOKIE_MEMERY_FILE + "." + Guid.NewGuid().ToString("N") + ".tmp";
			File.WriteAllBytes(tmpFile, array);
			if (File.Exists(COOKIE_MEMERY_FILE))
			{
				File.Delete(COOKIE_MEMERY_FILE);
			}
			File.Move(tmpFile, COOKIE_MEMERY_FILE);
			tmpFile = null;
		}
		catch (Exception)
		{
			// 清理残留临时文件，避免 config 目录累积垃圾文件
			try { if (tmpFile != null && File.Exists(tmpFile)) File.Delete(tmpFile); } catch { }
		}
	}
}
