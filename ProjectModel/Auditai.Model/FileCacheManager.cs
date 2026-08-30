using System;
using System.IO;
using System.Threading.Tasks;
using Auditai.Util;

namespace Auditai.Model;

public class FileCacheManager
{
	private Project _owner;

	internal FileCacheManager(Project owner)
	{
		_owner = owner;
	}

	public string GetPath(Guid fileId)
	{
		return Path.Combine(GetDirectory().FullName, $"{fileId}.bin");
	}

	public void CopyFrom(string from, Guid toFileId)
	{
		string path = GetPath(toFileId);
		File.Copy(from, path, overwrite: true);
		SetFileAttributeToNormal(path);
	}

	private static void SetFileAttributeToNormal(string filePath)
	{
		try
		{
			if (File.Exists(filePath))
			{
				File.SetAttributes(filePath, FileAttributes.Normal);
			}
		}
		catch (Exception)
		{
		}
	}

	public async Task Upload(Guid fileId)
	{
		// 修复：原 FileStream 未 using/Dispose，上传失败时句柄悬空。
		using FileStream fs = new FileStream(GetPath(fileId), FileMode.Open, FileAccess.Read);
		await WebApiClient.UploadFile(fileId, fs);
	}

	public async Task DownloadIfNotExist(Guid fileId)
	{
		if (Exists(fileId))
		{
			return;
		}
		using Stream stream = await WebApiClient.DownloadFile(fileId);
		if (stream == null)
		{
			return;
		}
		using FileStream fs = new FileStream(GetPath(fileId), FileMode.Create);
		try
		{
			await stream.CopyToAsync(fs);
		}
		catch (IOException)
		{
			File.Delete(GetPath(fileId));
			throw;
		}
		catch (Exception)
		{
			// 修复：原实现仅捕获 IOException 删除半成品，其它异常（如 UnauthorizedAccess）
			// 会留下损坏的缓存文件，后续 Exists 返回 true 用坏数据。任何失败都清理。
			try { File.Delete(GetPath(fileId)); } catch { }
			throw;
		}
	}

	public void Delete(Guid fileId)
	{
		File.Delete(GetPath(fileId));
	}

	public bool Exists(Guid fileId)
	{
		return File.Exists(GetPath(fileId));
	}

	public long GetFileSize(Guid fileId)
	{
		string path = GetPath(fileId);
		if (!File.Exists(path))
		{
			return 0L;
		}
		FileInfo fileInfo = new FileInfo(path);
		return fileInfo.Length;
	}

	public void DuplicateTo(Guid fileId, string dest)
	{
		File.Delete(dest);
		File.Copy(GetPath(fileId), dest);
	}

	public string DuplicateToTemp(Guid fileId, string dest)
	{
		string text = Path.Combine(Path.GetTempPath(), dest);
		File.Delete(text);
		File.Copy(GetPath(fileId), text);
		return text;
	}

	public static string GetLocalDataCacheDirectory(Guid projectId)
	{
		return Directory.CreateDirectory(Path.Combine("data", User.Current.Id.ToString(), projectId.ToString())).FullName;
	}

	private DirectoryInfo GetDirectory()
	{
		return Directory.CreateDirectory(Path.Combine("data", User.Current.Id.ToString(), _owner.Id.ToString(), "FileCache"));
	}
}
