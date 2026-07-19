﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.IO;
using System.Text;
using Auditai.DTO;
using Newtonsoft.Json;

namespace Auditai.Util;

public static class TokenTimer
{
	private static UserToken _token;

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
			// 服务端返回的 UserToken 不包含 Cookie，保留旧 Cookie 避免丢失机器指纹
			if (value.Cookie == null && _token?.Cookie != null)
			{
				value.Cookie = _token.Cookie;
			}
			_token = value;
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
		catch (Exception)
		{
		}
	}

	public static void SaveCookieToMachine()
	{
		try
		{
			if (Token?.Cookie == null)
			{
				return;
			}
			if (File.Exists("./config/cookie.json"))
			{
				File.Delete("./config/cookie.json");
			}
			using FileStream fileStream = new FileStream("./config/cookie.json", FileMode.Create, FileAccess.Write);
			string s = JsonConvert.SerializeObject(Token.Cookie);
			byte[] bytes = Encoding.Default.GetBytes(s);
			string keyBase = Encrypts.CreateBase64Key(MachineCode.Code);
			byte[] array = Encrypts.AesEncrypt(bytes, keyBase);
			fileStream.Write(array, 0, array.Length);
		}
		catch (Exception)
		{
		}
	}
}
