using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace FileTransferModel;

public class FileTranferManager
{
	private static FileTranferManager instance;

	private Timer timer = new Timer
	{
		Interval = 1000.0
	};

	public readonly HashSet<string> CanceledFileSet = new HashSet<string>();

	public readonly Dictionary<string, FileCache> FileCacheMap = new Dictionary<string, FileCache>();

	private TimeSpan TimeoutSpan { get; set; } = new TimeSpan(0, 30, 0);


	public event EventHandler<FileInfo> FileTimeout;

	public event EventHandler<FileInfo> FileInfoRecieved;

	public event EventHandler<FileSection> FileSectionRecieved;

	public event EventHandler<TranferCompleteEventArgs> FileRecieveCompleted;

	public static FileTranferManager GetInstance()
	{
		if (instance == null)
		{
			instance = new FileTranferManager();
		}
		return instance;
	}

	private FileTranferManager()
	{
		timer.Elapsed += Timer_Elapsed;
	}

	public void RecieveFileInfo(string sendId, string recieveId, FileInfo fileInfo)
	{
		if (fileInfo != null)
		{
			if (FileCacheMap.ContainsKey(fileInfo.Id))
			{
				FileCacheMap.Remove(fileInfo.Id);
			}
			FileCacheMap.Add(fileInfo.Id, new FileCache
			{
				SendUserId = sendId,
				RecieveUserId = recieveId,
				FileInfo = fileInfo,
				TransferStart = DateTime.Now,
				FileState = FileState.Recieving,
				RecievedSections = new List<FileSection>()
			});
			OnFileInfoRecieved(fileInfo);
		}
	}

	public void RecieveSection(FileSection section)
	{
		if (section == null || !FileCacheMap.ContainsKey(section.Id))
		{
			return;
		}
		try
		{
			FileCache fileCache = FileCacheMap[section.Id];
			if (fileCache.RecievedSections.Count == 0)
			{
				fileCache.TransferStart = DateTime.Now;
			}
			if (!timer.Enabled)
			{
				timer.Enabled = true;
			}
			fileCache.RecievedSections.Add(section);
			OnFileSectionRecieved(section);
			if (fileCache.RecievedSections.Count == fileCache.FileInfo.SectionCount)
			{
				fileCache.FileState = FileState.RecieveComplete;
				byte[] fileFromSections = FileUtil.GetFileFromSections(fileCache.RecievedSections);
				OnFileTranferCompleted(fileCache.FileInfo.Id, fileCache.FileInfo.Name, fileFromSections);
				FileCacheMap.Remove(section.Id);
			}
		}
		catch (Exception)
		{
		}
	}

	protected void OnFileTimeout(FileInfo fileInfo)
	{
		this.FileTimeout?.Invoke(this, fileInfo);
	}

	protected void OnFileInfoRecieved(FileInfo fileInfo)
	{
		this.FileInfoRecieved?.Invoke(this, fileInfo);
	}

	protected void OnFileSectionRecieved(FileSection section)
	{
		this.FileSectionRecieved?.Invoke(this, section);
	}

	protected void OnFileTranferCompleted(string id, string filename, byte[] contents)
	{
		this.FileRecieveCompleted?.Invoke(this, new TranferCompleteEventArgs(id, filename, contents));
	}

	private void Timer_Elapsed(object sender, ElapsedEventArgs e)
	{
		try
		{
			foreach (string item in from kv in FileCacheMap
				where kv.Value.FileState == FileState.Recieving || kv.Value.FileState == FileState.Sending
				select kv.Key)
			{
				FileCache fileCache = FileCacheMap[item];
				if (DateTime.Now - fileCache.TransferStart > TimeoutSpan)
				{
					if (fileCache.FileState == FileState.Sending)
					{
						fileCache.FileState = FileState.SendTimeout;
					}
					else if (fileCache.FileState == FileState.Recieving)
					{
						fileCache.FileState = FileState.RecieveTimeout;
					}
					OnFileTimeout(fileCache.FileInfo);
					FileCacheMap.Remove(item);
				}
			}
			if (FileCacheMap.Count == 0 && timer.Enabled)
			{
				timer.Enabled = false;
			}
		}
		catch (Exception)
		{
		}
	}
}
