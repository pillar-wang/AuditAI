﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Timers;
using Leqisoft.DTO;

namespace Leqisoft.Util;

public class TokenUpdater
{
	private System.Timers.Timer timer;

	private Thread thread;

	public TimeSpan Interval { get; set; }

	public TokenUpdater()
	{
		Interval = TimeSpan.FromMinutes(10.0);
		timer = new System.Timers.Timer
		{
			Enabled = false
		};
		timer.Elapsed += Timer_Elapsed;
	}

	public void Start()
	{
		// 本地模式下不启动Token更新定时器
		if (ConfigurationManager.AppSettings["StorageMode"]?.Equals("Local", StringComparison.OrdinalIgnoreCase) == true)
			return;
		if (thread != null)
		{
			thread.Abort();
		}
		thread = new Thread((ThreadStart)delegate
		{
			timer.Interval = Interval.TotalMilliseconds;
			timer.Enabled = true;
			timer.Start();
		});
		thread.IsBackground = true;
		thread.Start();
	}

	public void Stop()
	{
		if (thread != null)
		{
			thread.Abort();
		}
	}

	private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
	{
		try
		{
			if (ConfigurationManager.AppSettings["StorageMode"]?.Equals("Local", StringComparison.OrdinalIgnoreCase) == true)
				return;
			TokenTimer.Token = await WebApiClient.UpdateToken(TokenTimer.LoginInfo.userId);
		}
		catch (HttpRequestException)
		{
		}
		catch (TimeoutException)
		{
		}
		catch (NormalException)
		{
		}
		catch
		{
		}
	}
}
