using System;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Auditai.DTO;

namespace Auditai.Util;

public class TokenUpdater
{
	private System.Timers.Timer timer;

	private CancellationTokenSource _cts;

	private volatile bool _running = false;

	private bool _reloginInProgress = false;

	private bool _elapsedInProgress = false;

	public TimeSpan Interval { get; set; }

	public TokenUpdater()
	{
		Interval = TimeSpan.FromMinutes(10.0);
		timer = new System.Timers.Timer
		{
			Enabled = false
		};
		timer.Elapsed += Timer_Elapsed;
		_cts = new CancellationTokenSource();
	}

	public void Start()
	{
		// 本地模式下不启动Token更新定时器
		if (ConfigurationManager.AppSettings["StorageMode"]?.Equals("Local", StringComparison.OrdinalIgnoreCase) == true)
			return;
		// 取消之前未完成的刷新任务（若有）
		_cts.Cancel();
		_cts.Dispose();
		_cts = new CancellationTokenSource();
		_running = true;
		timer.Interval = Interval.TotalMilliseconds;
		timer.Enabled = true;
		timer.Start();
	}

	public void Stop()
	{
		_running = false;
		try { timer.Stop(); } catch { }
		try { _cts.Cancel(); } catch { }
	}

	private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
	{
		if (!_running) return;
		// 修复：System.Timers.Timer 默认 AutoReset=true，一次 UpdateToken 耗时超过
		// 间隔时会重入并发执行，并发写 Token/Cookie 文件（删除+重建+加密，非原子）有损坏风险。
		// 用标志串行化，重入时直接放弃本次刷新。
		if (_elapsedInProgress) return;
		_elapsedInProgress = true;
		var token = _cts.Token;
		try
		{
			if (ConfigurationManager.AppSettings["StorageMode"]?.Equals("Local", StringComparison.OrdinalIgnoreCase) == true)
				return;
			if (token.IsCancellationRequested) return;
			UserToken newToken = await WebApiClient.UpdateToken(TokenTimer.LoginInfo.userId);
			if (token.IsCancellationRequested) return;
			// UpdateToken 返回 null 时不要覆盖现有 Token，避免丢失有效凭据
			if (newToken != null && !string.IsNullOrEmpty(newToken.TokenValue))
			{
				// 保留旧 Token 的 Cookie（服务端 UpdateToken 不返回 Cookie）
				var oldCookie = TokenTimer.Token?.Cookie;
				if (newToken.Cookie == null && oldCookie != null)
				{
					newToken.Cookie = oldCookie;
				}
				TokenTimer.Token = newToken;
			}
		}
		catch (HttpRequestException ex)
		{
			var msg = ex.Message ?? "";
			if (msg.Contains("401") || msg.IndexOf("Unauthorized", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				// Token 已失效（过期/DB 重置/被踢），尝试自动重新登录
				try { timer.Stop(); } catch { }
				await TryRelogin();
			}
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
		finally
		{
			_elapsedInProgress = false;
		}
	}

	private async Task TryRelogin()
	{
		if (_reloginInProgress)
			return;
		_reloginInProgress = true;
		try
		{
			await WebApiClient.ReloginForTokenUpdate();
			// 重新登录成功，重启定时器
			if (_running)
			{
				try { timer.Enabled = true; timer.Start(); } catch { }
			}
		}
		catch
		{
			// 重新登录也失败，用户需要手动重新登录
		}
		finally
		{
			_reloginInProgress = false;
		}
	}
}
