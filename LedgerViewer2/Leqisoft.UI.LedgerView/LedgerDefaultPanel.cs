﻿﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using C1.Win.C1Command;
using C1.Win.C1SplitContainer;
using C1.Win.C1Tile;
using FileTransferModel;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.SignalR;
using Leqisoft.UI.CommonControls;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.LedgerView.Properties;

namespace Leqisoft.UI.LedgerView;

public class LedgerDefaultPanel : ISetTheme
{
	private MultiLedgerViewer _owner;

	private Tile _selectedTile;

	private Template _fileTemplate;

	private Template _titleTemplate;

	private C1TileControlEx _tileControl;

	private C1CommandLink lnkCancelSend = new C1CommandLink();

	private C1Command cmdCancelSend = new C1Command();

	private C1CommandLink lnkCancelRecieve = new C1CommandLink();

	private C1Command cmdCancelRecieve = new C1Command();

	private static string FileSavePath;

	private Dictionary<string, Tile> sendFileIdTileMap = new Dictionary<string, Tile>();

	private Dictionary<string, Tile> recieveFileIdTileMap = new Dictionary<string, Tile>();

	private SolidBrush solidBrush;

	private Font progressFont = new Font("微软雅黑", 9f, FontStyle.Regular);

	private const string SEND_WAITACCEPT_TIP = "正在等待接收";

	private const string SENDING_TIP = "正在发送";

	private const string RECEIEVING_TIP = "正在接收";

	private FlickerManager flickerManager = new FlickerManager();

	private C1CommandLink lnkShare = new C1CommandLink();

	private C1ContextMenu ctxShare = new C1ContextMenu();

	private C1CommandLink lnkOpenLocation = new C1CommandLink();

	private C1Command cmdOpenLocation = new C1Command();

	private C1CommandLink lnkRename = new C1CommandLink();

	private C1Command cmdRename = new C1Command();

	private C1CommandLink lnkDelete = new C1CommandLink();

	private C1Command cmdDelete = new C1Command();

	private C1CommandLink lnkRefresh = new C1CommandLink();

	private C1Command cmdRefresh = new C1Command();

	private Point contextMenuLocation;

	private C1ToolBar toolBar = new C1ToolBar();

	private C1SplitterPanel pnlSidebar;

	private C1ContextMenu contextMenu = new C1ContextMenu();

	private C1ContextMenu cacelContextMenu = new C1ContextMenu();

	private RibbonImageProcess imageProcess = new RibbonImageProcess();

	private C1SplitContainer emptyView;

	private Pen selectBorderPen = new Pen(Theme.SelectedLeqiTheme.ThemeContext.DarkColor, 4f);

	private Tile previousMouseCloseTile;

	private Tile previousMouseSlideTile;

	private System.Drawing.Image imgTileClose = Leqisoft.UI.Controls.Properties.Resources.tileClose;

	private System.Drawing.Image imgTileCloseDown = Leqisoft.UI.Controls.Properties.Resources.tileCloseDown;

	private System.Drawing.Image imgTileCloseSlide = Leqisoft.UI.Controls.Properties.Resources.tileCloseSlide;

	private TooltipBox _tooltipBox = new TooltipBox
	{
		Duration = 15000,
		IsBalloon = true
	};

	private Font titleFont = new Font("微软雅黑", 9f, FontStyle.Regular);

	private Tile SelectedTile
	{
		get
		{
			return _selectedTile;
		}
		set
		{
			_selectedTile = value;
			_tileControl.Invalidate();
		}
	}

	public C1TileControlEx GetTileControl => _tileControl;

	public LedgerDefaultPanel(MultiLedgerViewer owner)
	{
		_owner = owner;
		_tileControl = new C1TileControlEx
		{
			CellWidth = 20,
			CellHeight = 15,
			AllowChecking = false,
			Dock = DockStyle.Fill,
			CellSpacing = 20,
			Margin = new Padding(0),
			Padding = new Padding(0),
			GroupPadding = new Padding(0),
			Orientation = LayoutOrientation.Vertical,
			TileBorderColor = Color.White,
			GroupSpacing = 5,
			ShowToolTips = false
		};
		_fileTemplate = CreateFileTemplate();
		_tileControl.Templates.Add(_fileTemplate);
		_titleTemplate = CreateTitleTemplate();
		_tileControl.Templates.Add(_titleTemplate);
		_tileControl.Paint += _tileControl_Paint;
		_tileControl.TileClicked += _tileControl_TileClicked;
		_tileControl.MouseClick += _tileControl_MouseClick;
		_tileControl.MouseMove += _tileControl_MouseMove;
		_tileControl.MouseDown += _tileControl_MouseDown;
		_tileControl.MouseUp += _tileControl_MouseUp;
		_tileControl.MouseLeave += _tileControl_MouseLeave;
		_tileControl.MouseEnter += _tileControl_MouseEnter;
		_tileControl.MouseUp += _tileControl_MouseUp_Menu;
		ThemeManager.GetInstance().Register(this);
		_tileControl.TileBorderColor = Color.Transparent;
		cmdCancelSend.Text = "取消发送";
		cmdCancelSend.CommandStateQuery += CmdCancelSend_CommandStateQuery;
		cmdCancelSend.Click += CmdCancelSend_Click;
		lnkCancelSend.Command = cmdCancelSend;
		cacelContextMenu.CommandLinks.Add(lnkCancelSend);
		cmdCancelRecieve.Text = "取消接收";
		cmdCancelRecieve.CommandStateQuery += CmdCancelRecieve_CommandStateQuery;
		cmdCancelRecieve.Click += CmdCancelRecieve_Click;
		lnkCancelRecieve.Command = cmdCancelRecieve;
		cacelContextMenu.CommandLinks.Add(lnkCancelRecieve);
		ctxShare.Text = "分享账套";
		ctxShare.Image = ContextResources.ctxShareLedger;
		ctxShare.CommandStateQuery += CmdShare_CommandStateQuery;
		lnkShare.Command = ctxShare;
		contextMenu.CommandLinks.Add(lnkShare);
		cmdRename.Text = "重命名账套";
		cmdRename.Image = ContextResources.ctxMofify;
		cmdRename.Click += delegate
		{
			RenameLedger();
		};
		lnkRename.Command = cmdRename;
		contextMenu.CommandLinks.Add(lnkRename);
		cmdDelete.Text = "删除账套";
		cmdDelete.Image = ContextResources.ctxDelete;
		cmdDelete.Click += delegate
		{
			DeleteLedger();
		};
		lnkDelete.Command = cmdDelete;
		contextMenu.CommandLinks.Add(lnkDelete);
		cmdOpenLocation.Text = "打开账套所在位置";
		cmdOpenLocation.Click += delegate
		{
			OpenLocation();
		};
		lnkOpenLocation.Command = cmdOpenLocation;
		contextMenu.CommandLinks.Add(lnkOpenLocation);
		cmdRefresh.Text = "刷新";
		cmdRefresh.Click += delegate
		{
			Populate((_selectedTile?.Tag as TileInfo)?.LocalFile);
		};
		lnkRefresh.Command = cmdRefresh;
		lnkRefresh.Delimiter = true;
		contextMenu.CommandLinks.Add(lnkRefresh);
		FileTranferManager instance = FileTranferManager.GetInstance();
		instance.FileInfoRecieved += FileTranferManager_FileInfoRecieved;
		instance.FileSectionRecieved += FileTranferManager_FileSectionRecieved;
		instance.FileRecieveCompleted += FileTransferManager_RecieveCompleted;
		FileSavePath = FileRecievedDir();
	}

	private void _tileControl_Paint(object sender, PaintEventArgs e)
	{
		if (_selectedTile != null && _tileControl.Groups.Contains(_selectedTile.Group))
		{
			e.Graphics.DrawRectangle(selectBorderPen, _selectedTile.Group.X + _selectedTile.X - 2, _selectedTile.Group.Y + _selectedTile.Y - 2 + _tileControl.ScrollOffset, _selectedTile.Width + 4, _selectedTile.Height + 4);
		}
	}

	public void LedgerDefaultPanel_AfterRecieveCancel(object sender, string e)
	{
		if (recieveFileIdTileMap.ContainsKey(e))
		{
			Tile tile = recieveFileIdTileMap[e];
			recieveFileIdTileMap.Remove(e);
			if (tile.Tag is TileInfo tileInfo && tileInfo.TileFlag.HasFlag(TileFlag.Recieving))
			{
				tileInfo.TileFlag ^= TileFlag.Recieving;
				tile.Image2 = null;
			}
			tile.Invalidate();
			tile.Group.Tiles.RemoveAt(tile.Index);
		}
	}

	public void LedgerDefaultPanel_AfterSendCancel(object sender, string e)
	{
		if (!sendFileIdTileMap.ContainsKey(e))
		{
			return;
		}
		Tile tile = sendFileIdTileMap[e];
		sendFileIdTileMap.Remove(e);
		if (tile.Tag is TileInfo tileInfo)
		{
			if (tileInfo.TileFlag.HasFlag(TileFlag.WaitSend))
			{
				tileInfo.TileFlag ^= TileFlag.WaitSend;
				tile.Image2 = null;
			}
			if (tileInfo.TileFlag.HasFlag(TileFlag.Sending))
			{
				tileInfo.TileFlag ^= TileFlag.Sending;
				tile.Image2 = null;
			}
		}
		tile.Invalidate();
	}

	private async void CmdCancelRecieve_Click(object sender, ClickEventArgs e)
	{
		Tile tileAt = _tileControl.GetTileAt(contextMenuLocation);
		if (DialogResult.OK == Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "确定要取消接收吗？", MessageBoxButtons.OKCancel))
		{
			await CancelRecieve(tileAt);
		}
	}

	private static async Task CancelRecieve(Tile clickedTile)
	{
		if (clickedTile?.Tag is TileInfo tileInfo && tileInfo.TileFlag.HasFlag(TileFlag.Recieving))
		{
			if (tileInfo.TileFlag.HasFlag(TileFlag.Recieving))
			{
				tileInfo.TileFlag ^= TileFlag.Recieving;
				clickedTile.Image2 = null;
			}
			string fileId = tileInfo.FileId;
			Dictionary<string, FileCache> fileCacheMap = FileTranferManager.GetInstance().FileCacheMap;
			if (fileId != null && fileCacheMap.ContainsKey(fileId))
			{
				clickedTile.Group.Tiles.RemoveAt(clickedTile.Index);
				FileCache fileCache = fileCacheMap[fileId];
				fileCacheMap.Remove(fileId);
				NotifyMessage notifyMessage = new NotifyMessage
				{
					Kind = "cancelrecieve",
					Bullet = false,
					Value = fileId
				};
				await SignalRClient.SendToUser(fileCache.SendUserId, notifyMessage.ToString());
			}
		}
	}

	private void CmdCancelRecieve_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!(_tileControl.GetTileAt(_tileControl.PointToClient(Control.MousePosition))?.Tag is TileInfo tileInfo))
		{
			e.Visible = false;
		}
		else if (tileInfo.TileFlag.HasFlag(TileFlag.Recieving))
		{
			e.Visible = true;
		}
		else
		{
			e.Visible = false;
		}
	}

	private void CmdCancelSend_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!(_tileControl.GetTileAt(_tileControl.PointToClient(Control.MousePosition))?.Tag is TileInfo tileInfo))
		{
			e.Visible = false;
		}
		else if (tileInfo.TileFlag.HasFlag(TileFlag.WaitSend) || tileInfo.TileFlag.HasFlag(TileFlag.Sending))
		{
			e.Visible = true;
		}
		else
		{
			e.Visible = false;
		}
	}

	private async void CmdCancelSend_Click(object sender, ClickEventArgs e)
	{
		Tile tileAt = _tileControl.GetTileAt(contextMenuLocation);
		if (DialogResult.OK == Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "确定要取消发送吗？", MessageBoxButtons.OKCancel))
		{
			await CancelSend(tileAt);
		}
	}

	private static async Task CancelSend(Tile clickedTile)
	{
		if (clickedTile?.Tag is TileInfo tileInfo && (tileInfo.TileFlag.HasFlag(TileFlag.WaitSend) || tileInfo.TileFlag.HasFlag(TileFlag.Sending)))
		{
			if (tileInfo.TileFlag.HasFlag(TileFlag.WaitSend))
			{
				tileInfo.TileFlag ^= TileFlag.WaitSend;
				clickedTile.Image2 = null;
			}
			if (tileInfo.TileFlag.HasFlag(TileFlag.Sending))
			{
				tileInfo.TileFlag ^= TileFlag.Sending;
				clickedTile.Image2 = null;
			}
			string fileId = tileInfo.FileId;
			Dictionary<string, FileCache> fileCacheMap = FileTranferManager.GetInstance().FileCacheMap;
			if (fileId != null && fileCacheMap.ContainsKey(fileId))
			{
				FileCache fileCache = fileCacheMap[fileId];
				fileCacheMap.Remove(fileId);
				clickedTile.Invalidate();
				NotifyMessage notifyMessage = new NotifyMessage
				{
					Kind = "cancelsend",
					Bullet = false,
					Value = fileId
				};
				await SignalRClient.SendToUser(fileCache.RecieveUserId, notifyMessage.ToString());
			}
		}
	}

	private void CmdShare_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!(_selectedTile?.Tag is TileInfo tileInfo) || !tileInfo.TileFlag.HasFlag(TileFlag.LocalFile))
		{
			e.Visible = false;
			return;
		}
		C1CommandLink[] array = _owner.GetOnlineMemberContextMenu(tileInfo.LocalFile);
		if (array == null || array.Length == 0)
		{
			e.Visible = false;
			return;
		}
		e.Visible = true;
		ctxShare.CommandLinks.Clear();
		ctxShare.CommandLinks.AddRange(array);
	}

	public void BringToFront(string selected = null)
	{
		_owner.ToLedgerPanel();
		emptyView?.BringToFront();
		Populate(selected);
		GetTileControl.Update();
		PopulateSendingFile();
	}

	private void FileTranferManager_FileSectionRecieved(object sender, FileSection e)
	{
		if (recieveFileIdTileMap.ContainsKey(e.Id))
		{
			Tile tile = recieveFileIdTileMap[e.Id];
			if (tile.Tag is TileInfo tileInfo)
			{
				tileInfo.ProgressInfo.current += 1.0;
				tile.Invalidate();
			}
		}
	}

	private void FileTranferManager_FileInfoRecieved(object sender, FileTransferModel.FileInfo e)
	{
		PopulateRecievingFile();
		BringToFront();
	}

	public void PopulateSendingFile()
	{
		try
		{
			IEnumerable<Tile> enumerable = _tileControl.Groups.SelectMany((Group g) => g.Tiles);
			foreach (Tile item in enumerable)
			{
				item.Paint -= SendingTilePaint;
			}
			Dictionary<string, FileCache> dictionary = new Dictionary<string, FileCache>();
			Dictionary<string, FileCache> fileCacheMap = FileTranferManager.GetInstance().FileCacheMap;
			foreach (KeyValuePair<string, FileCache> item2 in fileCacheMap.Where((KeyValuePair<string, FileCache> kv) => kv.Value.FileState == FileState.SendWaitAccept || kv.Value.FileState == FileState.Sending))
			{
				dictionary.Add(item2.Key, item2.Value);
			}
			if (dictionary.Count == 0)
			{
				return;
			}
			foreach (KeyValuePair<string, FileCache> item3 in dictionary)
			{
				FileCache value = item3.Value;
				string file = value.LocalFile;
				Tile tile = enumerable.FirstOrDefault((Tile t) => t.Tag is TileInfo tileInfo2 && tileInfo2.TileFlag.HasFlag(TileFlag.LocalFile) && tileInfo2.LocalFile == file);
				if (tile == null)
				{
					continue;
				}
				tile.Image2 = imgTileClose;
				if (sendFileIdTileMap.ContainsKey(value.FileInfo.Id))
				{
					Tile tile2 = sendFileIdTileMap[value.FileInfo.Id];
					tile.Tag = tile2.Tag;
				}
				else if (tile.Tag is TileInfo tileInfo)
				{
					if (value.FileState == FileState.SendWaitAccept)
					{
						if (!tileInfo.TileFlag.HasFlag(TileFlag.WaitSend))
						{
							tileInfo.TileFlag |= TileFlag.WaitSend;
						}
					}
					else if (value.FileState == FileState.Sending && !tileInfo.TileFlag.HasFlag(TileFlag.Sending))
					{
						tileInfo.TileFlag |= TileFlag.Sending;
					}
					tileInfo.FileId = item3.Key;
					tileInfo.ProgressInfo = new FileTransferProgressInfo
					{
						current = 0.0,
						count = value.FileInfo.SectionCount
					};
					sendFileIdTileMap.Add(value.FileInfo.Id, tile);
				}
				tile.Paint += SendingTilePaint;
				tile.Invalidate();
			}
			_tileControl.Update();
		}
		catch (Exception ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	private void PopulateRecievingFile()
	{
		try
		{
			IEnumerable<Tile> source = _tileControl.Groups.SelectMany((Group g) => g.Tiles);
			IEnumerable<Tile> source2 = source.Where((Tile t) => t.Tag is TileInfo tileInfo3 && tileInfo3.TileFlag.HasFlag(TileFlag.Recieving));
			IEnumerable<IGrouping<Group, Tile>> enumerable = from t in source2
				group t by t.Group;
			foreach (IGrouping<Group, Tile> item in enumerable)
			{
				IOrderedEnumerable<int> orderedEnumerable = from t in item
					select t.Index into i
					orderby i descending
					select i;
				foreach (int item2 in orderedEnumerable)
				{
					item.Key.Tiles.RemoveAt(item2);
				}
			}
			Dictionary<string, FileCache> fileCacheMap = FileTranferManager.GetInstance().FileCacheMap;
			Dictionary<string, FileCache> dictionary = new Dictionary<string, FileCache>();
			foreach (KeyValuePair<string, FileCache> item3 in fileCacheMap.Where((KeyValuePair<string, FileCache> kv) => kv.Value.FileState == FileState.Recieving))
			{
				dictionary.Add(item3.Key, item3.Value);
			}
			if (dictionary.Count == 0)
			{
				return;
			}
			string recievedDir = FileSavePath ?? FileRecievedDir();
			Group group = _tileControl.Groups.FirstOrDefault((Group g) => g.Tag is TileInfo tileInfo2 && tileInfo2.TileFlag.HasFlag(TileFlag.Group) && tileInfo2.LocalFile == recievedDir);
			if (group == null)
			{
				group = CreateFilesGroup(new TileInfo
				{
					TileFlag = TileFlag.Group,
					LocalFile = recievedDir
				}, new string[0], Leqisoft.UI.LedgerView.Properties.Resources.ledger2);
				_tileControl.Groups.Insert(0, group);
				_tileControl.Groups.Insert(0, CreateTitleGroup(recievedDir));
			}
			foreach (KeyValuePair<string, FileCache> item4 in dictionary)
			{
				FileCache value = item4.Value;
				Tile tile = CreateFileTile(value.FileInfo.Name, Leqisoft.UI.LedgerView.Properties.Resources.ledger2, new TileInfo
				{
					TileFlag = TileFlag.Recieving,
					FileId = value.FileInfo.Id
				});
				tile.Image2 = imgTileClose;
				tile.Paint += RecievingTilePaint;
				group.Tiles.Add(tile);
				if (recieveFileIdTileMap.TryGetValue(value.FileInfo.Id, out var value2))
				{
					recieveFileIdTileMap.Remove(value.FileInfo.Id);
					tile.Tag = value2?.Tag;
					recieveFileIdTileMap.Add(value.FileInfo.Id, tile);
				}
				else if (tile.Tag is TileInfo tileInfo)
				{
					tileInfo.FileId = item4.Key;
					tileInfo.ProgressInfo = new FileTransferProgressInfo
					{
						current = 0.0,
						count = value.FileInfo.SectionCount
					};
					recieveFileIdTileMap.Add(value.FileInfo.Id, tile);
				}
				tile.Invalidate();
			}
			Tile tile2 = group?.Tiles.LastOrDefault();
			if (tile2 != null)
			{
				_tileControl.ScrollToTile(tile2, immediate: true);
			}
			_tileControl.Update();
		}
		catch (Exception ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void SendingTilePaint(object sender, PaintEventArgs e)
	{
		if (!(sender is Tile { Tag: TileInfo tag } tile) || (!tag.TileFlag.HasFlag(TileFlag.WaitSend) && !tag.TileFlag.HasFlag(TileFlag.Sending)))
		{
			return;
		}
		if (solidBrush == null)
		{
			Color darkColor = Theme.SelectedLeqiTheme.ThemeContext.DarkColor;
			solidBrush = new SolidBrush(Color.FromArgb(100, darkColor.R, darkColor.G, darkColor.B));
		}
		Dictionary<string, FileCache> fileCacheMap = FileTranferManager.GetInstance().FileCacheMap;
		if (tag.FileId == null || !fileCacheMap.ContainsKey(tag.FileId))
		{
			return;
		}
		try
		{
			FileTransferProgressInfo progressInfo = tag.ProgressInfo;
			FileCache fileCache = fileCacheMap[tag.FileId];
			e.Graphics.FillRectangle(solidBrush, new Rectangle
			{
				X = 0,
				Y = 0,
				Height = tile.Height,
				Width = (int)((double)e.ClipRectangle.Width * (progressInfo.current / progressInfo.count))
			});
			string text = ((fileCache.FileState == FileState.SendWaitAccept) ? "正在等待接收" : "正在发送");
			SizeF sizeF = e.Graphics.MeasureString(text, progressFont);
			e.Graphics.DrawString(text, progressFont, Brushes.Red, ((float)tile.Width - sizeF.Width) / 2f, 5f);
		}
		catch (Exception)
		{
		}
	}

	private void RecievingTilePaint(object sender, PaintEventArgs e)
	{
		if (!(sender is Tile { Tag: TileInfo tag } tile) || !tag.TileFlag.HasFlag(TileFlag.Recieving))
		{
			return;
		}
		Dictionary<string, FileCache> fileCacheMap = FileTranferManager.GetInstance().FileCacheMap;
		if (tag.FileId != null && fileCacheMap.ContainsKey(tag.FileId))
		{
			if (solidBrush == null)
			{
				Color darkColor = Theme.SelectedLeqiTheme.ThemeContext.DarkColor;
				solidBrush = new SolidBrush(Color.FromArgb(100, darkColor.R, darkColor.G, darkColor.B));
			}
			e.Graphics.FillRectangle(solidBrush, new Rectangle
			{
				X = 0,
				Y = 0,
				Height = tile.Height,
				Width = (int)((double)e.ClipRectangle.Width * (tag.ProgressInfo.current / tag.ProgressInfo.count))
			});
			SizeF sizeF = e.Graphics.MeasureString("正在接收", progressFont);
			e.Graphics.DrawString("正在接收", progressFont, Brushes.Red, ((float)tile.Width - sizeF.Width) / 2f, 5f);
		}
	}

	public void LedgerDefaultPanel_AfterSendSection(object sender, FileSection e)
	{
		if (!sendFileIdTileMap.ContainsKey(e.Id))
		{
			return;
		}
		Tile tile = sendFileIdTileMap[e.Id];
		if (tile.Tag is TileInfo tileInfo)
		{
			if (tileInfo.TileFlag.HasFlag(TileFlag.WaitSend))
			{
				tileInfo.TileFlag ^= TileFlag.WaitSend;
			}
			tileInfo.ProgressInfo.current += 1.0;
			tile.Invalidate();
		}
	}

	public void LedgerDefaultPanel_AfterSendComplete(object sender, FileCache e)
	{
		if (!sendFileIdTileMap.ContainsKey(e.FileInfo.Id))
		{
			return;
		}
		Tile tile = sendFileIdTileMap[e.FileInfo.Id];
		if (tile.Tag is TileInfo tileInfo)
		{
			if (tileInfo.TileFlag.HasFlag(TileFlag.Sending))
			{
				tileInfo.TileFlag ^= TileFlag.Sending;
			}
			tile.Image2 = null;
			sendFileIdTileMap.Remove(e.FileInfo.Id);
			tile.Invalidate();
		}
	}

	private void FileTransferManager_RecieveCompleted(object sender, TranferCompleteEventArgs e)
	{
		try
		{
			string localFullName = GetLocalFullName(e.FileName);
			using (FileStream fileStream = new FileStream(localFullName, FileMode.Create))
			{
				fileStream.Write(e.Contents, 0, e.Contents.Length);
			}
			if (recieveFileIdTileMap.ContainsKey(e.Id))
			{
				Tile tile = recieveFileIdTileMap[e.Id];
				if (tile.Tag is TileInfo tileInfo)
				{
					tileInfo.LocalFile = localFullName;
					tileInfo.TileFlag ^= TileFlag.Recieving;
					tileInfo.TileFlag |= TileFlag.LocalFile;
					tile.Image2 = null;
				}
				recieveFileIdTileMap.Remove(e.Id);
				_tileControl.Invoke((MethodInvoker)delegate
				{
					tile.Invalidate();
					TileFlickerProxy tileFlickerProxy = new TileFlickerProxy(tile);
					tileFlickerProxy.Text1 = "接收完成";
					tileFlickerProxy.SetTimer(SecondTrigger.Trigger);
					tileFlickerProxy.SetFlickTime(10);
					flickerManager.Add(tile, tileFlickerProxy);
					flickerManager.Start(tile);
				});
			}
			string recievedDir = FileSavePath ?? FileRecievedDir();
			Group group = _tileControl.Groups.FirstOrDefault((Group g) => g.Tag is TileInfo tileInfo2 && tileInfo2.TileFlag.HasFlag(TileFlag.Group) && tileInfo2.LocalFile == recievedDir);
			if (group == null)
			{
				AppendGroupImpl(recievedDir);
			}
		}
		catch (Exception ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	private string GetLocalFullName(string remoteFileName)
	{
		string text = FileSavePath ?? FileRecievedDir();
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string text2 = Path.Combine(text, remoteFileName);
		string extension = Path.GetExtension(remoteFileName);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(remoteFileName);
		int num = 1;
		while (File.Exists(text2))
		{
			text2 = Path.Combine(text, $"{fileNameWithoutExtension}({num++}){extension}");
		}
		return text2;
	}

	private static string FileRecievedDir()
	{
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		return Path.Combine(directoryName, "账套文件");
	}

	private void OpenLocation()
	{
		if (!(_selectedTile?.Tag is TileInfo tileInfo) || !tileInfo.TileFlag.HasFlag(TileFlag.LocalFile))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择账套！");
			return;
		}
		if (!File.Exists(tileInfo.LocalFile))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该账套文件不存在！");
			return;
		}
		try
		{
			string fullPath = Path.GetFullPath(tileInfo.LocalFile);
			Process.Start("Explorer.exe", "/select," + fullPath);
		}
		catch (Exception)
		{
		}
	}

	private void DeleteLedger()
	{
		if (!(_selectedTile?.Tag is TileInfo tileInfo) || !tileInfo.TileFlag.HasFlag(TileFlag.LocalFile))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择账套！");
			return;
		}
		if (!File.Exists(tileInfo.LocalFile))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该账套文件不存在！");
			return;
		}
		try
		{
			if (_owner.OpenedLedgerViewerDic.ContainsKey(tileInfo.LocalFile))
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前账套在打开状态，无法删除");
			}
			else if (DialogResult.OK == Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "确定要删除选定的账套文件吗？", MessageBoxButtons.OKCancel))
			{
				File.Delete(tileInfo.LocalFile);
				Populate();
			}
		}
		catch (Exception)
		{
		}
	}

	private void RenameLedger()
	{
		if (!(_selectedTile?.Tag is TileInfo tileInfo) || !tileInfo.TileFlag.HasFlag(TileFlag.LocalFile))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择账套！");
			return;
		}
		if (!File.Exists(tileInfo.LocalFile))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该账套文件不存在！");
			return;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(tileInfo.LocalFile);
		string text = InputForm.Text("重命名", "账套名称", fileNameWithoutExtension, 200);
		if (text == null)
		{
			return;
		}
		string directoryName = Path.GetDirectoryName(tileInfo.LocalFile);
		string extension = Path.GetExtension(tileInfo.LocalFile);
		string text2 = Path.Combine(directoryName, text + extension);
		if (File.Exists(text2))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该文件已存在，请使用其他名称");
			return;
		}
		try
		{
			File.Move(tileInfo.LocalFile, text2);
			Populate(_owner.CurrentLedgerViewer?.CurrentFilePath);
			if (_owner.OpenedLedgerViewerDic.ContainsKey(tileInfo.LocalFile))
			{
				LedgerViewer ledgerViewer = _owner.OpenedLedgerViewerDic[tileInfo.LocalFile];
				_owner.OpenedLedgerViewerDic.Remove(tileInfo.LocalFile);
				_owner.OpenedLedgerViewerDic.Add(text2, ledgerViewer);
				ledgerViewer.RenameCurrentFile(text2);
				_owner.PopulateOpenedLedgers();
			}
		}
		catch (Exception ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件重命名失败！失败原因：" + ex.Message);
		}
	}

	private void _tileControl_MouseUp_Menu(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right && _tileControl.GetTileAt(e.Location)?.Tag is TileInfo tileInfo)
		{
			if (tileInfo.TileFlag.HasFlag(TileFlag.WaitSend) || tileInfo.TileFlag.HasFlag(TileFlag.Sending) || tileInfo.TileFlag.HasFlag(TileFlag.Recieving))
			{
				contextMenuLocation = e.Location;
				cacelContextMenu.ShowContextMenu(_tileControl, e.Location);
			}
			else if (_selectedTile?.Tag is TileInfo tileInfo2 && tileInfo2.TileFlag.HasFlag(TileFlag.LocalFile) && tileInfo.TileFlag.HasFlag(TileFlag.LocalFile) && tileInfo2.LocalFile == tileInfo.LocalFile)
			{
				contextMenuLocation = e.Location;
				contextMenu.ShowContextMenu(_tileControl, e.Location);
			}
		}
	}

	public void ShowSideToolbar()
	{
		pnlSidebar?.Show();
	}

	public void HideSideToolbar()
	{
		pnlSidebar?.Hide();
	}

	public C1SplitContainer GetView()
	{
		emptyView = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		C1SplitterPanel c1SplitterPanel = new C1SplitterPanel
		{
			SizeRatio = 100.0
		};
		emptyView.Panels.Add(c1SplitterPanel);
		C1CommandLink lnkShareLedger = new C1CommandLink();
		C1Command c1Command = new C1Command();
		c1Command.Text = "分享账套";
		c1Command.Image = Leqisoft.UI.LedgerView.Properties.Resources.sideShareLedger;
		c1Command.Click += delegate
		{
			if (!(_selectedTile?.Tag is TileInfo tileInfo) || !tileInfo.TileFlag.HasFlag(TileFlag.LocalFile))
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择账套！");
			}
			else if (!File.Exists(tileInfo.LocalFile))
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该账套文件不存在！");
			}
			else
			{
				_owner.OnAfterShare(new LedgerShareEventArgs
				{
					Viewer = null,
					Link = lnkShareLedger,
					File = tileInfo.LocalFile
				});
			}
		};
		lnkShareLedger.Command = c1Command;
		toolBar.CommandLinks.Add(lnkShareLedger);
		C1CommandLink c1CommandLink = new C1CommandLink();
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "重命名账套";
		c1Command2.Image = Leqisoft.UI.LedgerView.Properties.Resources.sideRenameLedger;
		c1Command2.Click += delegate
		{
			RenameLedger();
		};
		c1CommandLink.Command = c1Command2;
		toolBar.CommandLinks.Add(c1CommandLink);
		C1CommandLink c1CommandLink2 = new C1CommandLink();
		C1Command c1Command3 = new C1Command();
		c1Command3.Text = "删除账套";
		c1Command3.Image = Leqisoft.UI.LedgerView.Properties.Resources.sideDeleteLedger;
		c1Command3.Click += delegate
		{
			DeleteLedger();
		};
		c1CommandLink2.Command = c1Command3;
		toolBar.CommandLinks.Add(c1CommandLink2);
		C1CommandLink c1CommandLink3 = new C1CommandLink();
		C1Command c1Command4 = new C1Command();
		c1Command4.Text = "所在位置";
		c1Command4.Image = Leqisoft.UI.LedgerView.Properties.Resources.sideLocation;
		c1Command4.Click += delegate
		{
			OpenLocation();
		};
		c1CommandLink3.Command = c1Command4;
		toolBar.CommandLinks.Add(c1CommandLink3);
		foreach (C1CommandLink commandLink in toolBar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		C1SplitContainer value = ComponentFactory.BuildSidebar(_tileControl, toolBar, out pnlSidebar);
		c1SplitterPanel.Controls.Add(value);
		return emptyView;
	}

	public void Populate(string selected = null)
	{
		SelectedTile = null;
		flickerManager.Clear();
		_tileControl.Groups.Clear();
		List<string> scannedDirs = new List<string>();
		try
		{
			string text = Leqisoft.Model.Project.Current?.Id.ToString();
			if (text == null)
			{
				appendDemoGroup();
				appendReceivedDir();
				appendOther();
				PopulateRecievingFile();
				PopulateSendingFile();
				return;
			}
			Dictionary<string, DateTime> project = LedgerHistory2.OpenHistory.GetProject(text);
			if (project == null || project.Count == 0)
			{
				appendDemoGroup();
				appendReceivedDir();
				appendOther();
				PopulateRecievingFile();
				PopulateSendingFile();
				return;
			}
			IEnumerable<string> enumerable = from h in project
				orderby h.Value descending
				select h.Key;
			foreach (string item2 in enumerable)
			{
				if (!File.Exists(item2))
				{
					continue;
				}
				string directoryName = Path.GetDirectoryName(item2);
				if (Directory.Exists(directoryName) && !scannedDirs.Contains(directoryName))
				{
					scannedDirs.Add(directoryName);
					AppendGroupImpl(directoryName);
				}
			}
			appendDemoGroup();
			appendReceivedDir();
			appendOther();
			PopulateRecievingFile();
			PopulateSendingFile();
		}
		finally
		{
			if (selected != null)
			{
				foreach (Group group2 in _tileControl.Groups)
				{
					foreach (Tile tile in group2.Tiles)
					{
						if (tile.Tag is TileInfo tileInfo && tileInfo.TileFlag.HasFlag(TileFlag.LocalFile) && tileInfo.LocalFile == selected)
						{
							SelectedTile = tile;
							break;
						}
					}
				}
			}
		}
		void appendDemoGroup()
		{
			string folder = "./演示账套/";
			AppendGroupImpl(folder);
		}
		void appendReceivedDir()
		{
			string receivedDir = FileRecievedDir();
			string fullReceivedDir = Path.GetFullPath(receivedDir);
			if (!scannedDirs.Any(d => Path.GetFullPath(d) == fullReceivedDir))
			{
				AppendGroupImpl(receivedDir);
			}
		}
		void appendOther()
		{
			Group item = CreateTitleGroup("其他位置");
			_tileControl.Groups.Add(item);
			Group group = CreateFilesGroup(null, new string[1] { "打开其他账套" }, Leqisoft.UI.LedgerView.Properties.Resources.GraphDir);
			group.Tiles[0].Tag = new TileInfo
			{
				TileFlag = TileFlag.OtherPositionButton
			};
			group.Tiles[0].Click += async delegate
			{
				OpenFileDialog openFileDialog = new OpenFileDialog
				{
					Filter = "账套文件（*.db,*.001）|*.db;*.001"
				};
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						await _owner.OpenLedger(openFileDialog.FileName, userCache: false);
					}
					catch (FileNotFoundException exception)
					{
						exception.Log();
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件不存在！");
					}
					catch (SQLiteException exception2)
					{
						exception2.Log();
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件未能识别为账套格式文件，无法打开。");
					}
					catch (Exception ex)
					{
						ex.Log();
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
					}
				}
			};
			_tileControl.Groups.Add(group);
		}
	}

	public Tile GetTileByTag(object file)
	{
		if (file == null || !File.Exists(file.ToString()))
		{
			return null;
		}
		string fullPath = Path.GetFullPath(file.ToString());
		IEnumerable<Tile> enumerable = _tileControl.Groups.SelectMany((Group g) => g.Tiles);
		foreach (Tile item in enumerable)
		{
			if (item.Tag is TileInfo tileInfo && tileInfo.TileFlag.HasFlag(TileFlag.LocalFile))
			{
				string fullPath2 = Path.GetFullPath(tileInfo.LocalFile);
				if (fullPath2 == fullPath && File.Exists(fullPath2))
				{
					return item;
				}
			}
		}
		return null;
	}

	private void AppendGroupImpl(string folder)
	{
		if (Directory.Exists(folder))
		{
			string[] files = Directory.GetFiles(folder, "*.db");
			if (files.Length != 0)
			{
				string fullPath = Path.GetFullPath(folder);
				_tileControl.Groups.Add(CreateTitleGroup(fullPath));
				_tileControl.Groups.Add(CreateFilesGroup(new TileInfo
				{
					TileFlag = TileFlag.Group,
					LocalFile = fullPath
				}, files, Leqisoft.UI.LedgerView.Properties.Resources.ledger2));
			}
		}
	}

	private Group CreateTitleGroup(string title)
	{
		Group group = new Group();
		group.Tiles.Add(new Tile
		{
			Text = title,
			HorizontalSize = 20,
			VerticalSize = 2,
			Template = _titleTemplate,
			BackColor = Color.Transparent,
			Tag = new TileInfo
			{
				TileFlag = TileFlag.DirectoryTitle,
				LocalFile = title
			}
		});
		return group;
	}

	private Group CreateFilesGroup(TileInfo tileInfo, IEnumerable<string> files, System.Drawing.Image image)
	{
		Group group = new Group
		{
			Tag = tileInfo
		};
		foreach (string file in files)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
			Tile item = CreateFileTile(fileNameWithoutExtension, image, new TileInfo
			{
				TileFlag = TileFlag.LocalFile,
				LocalFile = file
			});
			group.Tiles.Add(item);
		}
		return group;
	}

	private Tile CreateFileTile(string text, System.Drawing.Image image, TileInfo tileInfo)
	{
		return new Tile
		{
			Tag = tileInfo,
			Text = text,
			VerticalSize = 4,
			HorizontalSize = 5,
			Template = _fileTemplate,
			Image1 = image,
			ForeColor1 = Color.Red
		};
	}

	private Template CreateFileTemplate()
	{
		Template template = new Template();
		template.Description = "Win32";
		PanelElement panelElement = new PanelElement();
		panelElement.Alignment = ContentAlignment.TopRight;
		panelElement.FixedHeight = 30;
		panelElement.FixedWidth = 40;
		panelElement.Margin = new Padding(0, 3, 3, 0);
		ImageElement imageElement = new ImageElement();
		imageElement.AlignmentOfContents = ContentAlignment.TopRight;
		imageElement.ColumnIndex = 30;
		imageElement.FixedWidth = 40;
		imageElement.ImageSelector = ImageSelector.Image2;
		panelElement.Children.Add(imageElement);
		PanelElement panelElement2 = new PanelElement();
		panelElement2.FixedHeight = 40;
		panelElement2.FixedWidth = 130;
		panelElement2.Alignment = ContentAlignment.TopCenter;
		TextElement textElement = new TextElement();
		textElement.AlignmentOfContents = ContentAlignment.TopCenter;
		textElement.TextTrimming = TextTrimming.EndEllipsis;
		textElement.SingleLine = false;
		textElement.FixedHeight = 40;
		textElement.FixedWidth = 130;
		textElement.Margin = new Padding(0, 5, 0, 0);
		textElement.TextSelector = TextSelector.Text1;
		textElement.ForeColorSelector = ForeColorSelector.ForeColor1;
		panelElement2.Children.Add(textElement);
		PanelElement panelElement3 = new PanelElement();
		panelElement3.FixedHeight = 50;
		panelElement3.FixedWidth = 50;
		panelElement3.Margin = new Padding(0, 30, 0, 0);
		panelElement3.Alignment = ContentAlignment.TopCenter;
		ImageElement imageElement2 = new ImageElement();
		imageElement2.AlignmentOfContents = ContentAlignment.TopCenter;
		imageElement2.FixedHeight = 50;
		imageElement2.FixedWidth = 50;
		imageElement2.ImageSelector = ImageSelector.Image1;
		panelElement3.Children.Add(imageElement2);
		PanelElement panelElement4 = new PanelElement();
		panelElement4.FixedHeight = 50;
		panelElement4.FixedWidth = 150;
		panelElement4.Alignment = ContentAlignment.BottomCenter;
		TextElement textElement2 = new TextElement();
		textElement2.AlignmentOfContents = ContentAlignment.TopCenter;
		textElement2.TextTrimming = TextTrimming.EndEllipsis;
		textElement2.SingleLine = false;
		textElement2.FixedHeight = 50;
		textElement2.FixedWidth = 150;
		panelElement4.Children.Add(textElement2);
		template.Elements.Add(panelElement);
		template.Elements.Add(panelElement2);
		template.Elements.Add(panelElement3);
		template.Elements.Add(panelElement4);
		template.Name = "mapImgTemplate";
		return template;
	}

	private Template CreateTitleTemplate()
	{
		Template template = new Template();
		template.Description = "Subgroup";
		PanelElement panelElement = new PanelElement();
		panelElement.AlignmentOfContents = ContentAlignment.BottomLeft;
		PanelElement panelElement2 = new PanelElement();
		panelElement2.BackColor = Color.FromArgb(0, 73, 92);
		panelElement2.Dock = DockStyle.Bottom;
		panelElement2.FixedHeight = 1;
		TextElement textElement = new TextElement();
		textElement.ForeColor = Color.FromArgb(0, 73, 92);
		textElement.ForeColorSelector = ForeColorSelector.Unbound;
		textElement.Font = titleFont;
		textElement.Margin = new Padding(0, 0, 0, 6);
		textElement.SingleLine = true;
		textElement.FontUnderline = ThreeStateBoolean.True;
		panelElement.Children.Add(textElement);
		panelElement.Dock = DockStyle.Fill;
		panelElement.Padding = new Padding(8, 0, 8, 8);
		template.Elements.Add(panelElement);
		template.Enabled = false;
		template.Name = "subgroupTemplate";
		return template;
	}

	private async void _tileControl_TileClicked(object sender, TileEventArgs e)
	{
		if (!(e.Tile.Tag is TileInfo tileInfo))
		{
			return;
		}
		if (tileInfo.TileFlag.HasFlag(TileFlag.Recieving) && mousePointInTileCloseArea(e.Tile))
		{
			if (DialogResult.OK == Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "确定要取消接收吗？", MessageBoxButtons.OKCancel))
			{
				await CancelRecieve(e.Tile);
			}
		}
		else if ((tileInfo.TileFlag.HasFlag(TileFlag.WaitSend) || tileInfo.TileFlag.HasFlag(TileFlag.Sending)) && mousePointInTileCloseArea(e.Tile))
		{
			if (DialogResult.OK == Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "确定要取消发送吗？", MessageBoxButtons.OKCancel))
			{
				await CancelSend(e.Tile);
			}
		}
		else if (tileInfo.TileFlag.HasFlag(TileFlag.LocalFile))
		{
			SelectedTile = e.Tile;
		}
	}

	private bool mousePointInTileCloseArea(Tile tile)
	{
		Point point = _tileControl.PointToClient(Control.MousePosition);
		int num = tile.Group.X + tile.X;
		int num2 = tile.Group.Y + tile.Y + _tileControl.ScrollOffset;
		int x = point.X;
		int y = point.Y;
		int num3 = x - num;
		int num4 = y - num2;
		Rectangle rectangle = new Rectangle(tile.Width - 3 - 16, 3, 16, 16);
		return num3 > rectangle.X && num3 < rectangle.X + rectangle.Width && num4 > rectangle.Y && num4 < rectangle.Y + rectangle.Height;
	}

	private void _tileControl_MouseMove(object sender, MouseEventArgs e)
	{
		Tile tileAt = _tileControl.GetTileAt(e.Location);
		if (tileAt == null)
		{
			_tooltipBox.Hide();
			previousMouseSlideTile = null;
			return;
		}
		if (!(tileAt.Tag is TileInfo tileInfo))
		{
			_tooltipBox.Hide();
			previousMouseSlideTile = null;
			return;
		}
		Point point = _tileControl.PointToClient(Control.MousePosition);
		if (tileInfo.TileFlag.HasFlag(TileFlag.WaitSend) || tileInfo.TileFlag.HasFlag(TileFlag.Sending) || tileInfo.TileFlag.HasFlag(TileFlag.Recieving))
		{
			if (mousePointInTileCloseArea(tileAt))
			{
				tileAt.Image2 = imgTileCloseSlide;
				previousMouseCloseTile = tileAt;
			}
			else
			{
				tileAt.Image2 = imgTileClose;
			}
		}
		else if (previousMouseCloseTile != null && previousMouseCloseTile.Tag is TileInfo tileInfo2 && (tileInfo2.TileFlag.HasFlag(TileFlag.WaitSend) || tileInfo2.TileFlag.HasFlag(TileFlag.Sending) || tileInfo2.TileFlag.HasFlag(TileFlag.Recieving)))
		{
			previousMouseCloseTile.Image2 = imgTileClose;
		}
		if (tileInfo.TileFlag.HasFlag(TileFlag.DirectoryTitle))
		{
			SizeF sizeF = tileAt.TileControl.CreateGraphics().MeasureString(tileInfo.LocalFile, titleFont);
			float num = (float)(tileAt.Group.Y + tileAt.Y + _tileControl.ScrollOffset) + ((float)tileAt.Height - sizeF.Height) / 2f;
			if (point.X > tileAt.Group.X && (float)point.X < (float)tileAt.Group.X + sizeF.Width && (float)point.Y > num && (float)point.Y < num + sizeF.Height)
			{
				_tileControl.Cursor = Cursors.Hand;
			}
			else
			{
				_tileControl.Cursor = Cursors.Default;
			}
		}
		else
		{
			_tileControl.Cursor = Cursors.Default;
		}
		if (tileAt == previousMouseSlideTile)
		{
			return;
		}
		if (tileInfo.TileFlag.HasFlag(TileFlag.LocalFile))
		{
			try
			{
				System.IO.FileInfo fileInfo = new System.IO.FileInfo(tileInfo.LocalFile);
				XElement xElement = new XElement("div");
				xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "文件名称：" + fileInfo.Name));
				xElement.Add(new XElement("p", new XAttribute("style", "color:red"), $"文件大小：{(double)fileInfo.Length / 1024.0}KB"));
				xElement.Add(new XElement("p", new XAttribute("style", "color:red"), $"修改日期：{fileInfo.LastWriteTime}"));
				_tooltipBox.SetText(string.Empty, xElement.ToString());
				_tooltipBox.Show(_tileControl, new Point(tileAt.Group.X + tileAt.X + tileAt.Width / 2, tileAt.Group.Y + tileAt.Y + _tileControl.ScrollOffset));
				previousMouseSlideTile = tileAt;
				return;
			}
			catch (Exception)
			{
				return;
			}
		}
		_tooltipBox.Hide();
		previousMouseSlideTile = null;
	}

	private void _tileControl_MouseEnter(object sender, EventArgs e)
	{
		previousMouseSlideTile = null;
	}

	private void _tileControl_MouseLeave(object sender, EventArgs e)
	{
		_tooltipBox.Hide();
	}

	private void _tileControl_MouseClick(object sender, MouseEventArgs e)
	{
		Tile tileAt = _tileControl.GetTileAt(e.Location);
		if (tileAt == null || !(tileAt.Tag is TileInfo tileInfo) || !tileInfo.TileFlag.HasFlag(TileFlag.DirectoryTitle))
		{
			return;
		}
		Point point = _tileControl.PointToClient(Control.MousePosition);
		SizeF sizeF = tileAt.TileControl.CreateGraphics().MeasureString(tileInfo.LocalFile, titleFont);
		float num = (float)(tileAt.Group.Y + tileAt.Y + _tileControl.ScrollOffset) + ((float)tileAt.Height - sizeF.Height) / 2f;
		if (point.X <= tileAt.Group.X || !((float)point.X < (float)tileAt.Group.X + sizeF.Width) || !((float)point.Y > num) || !((float)point.Y < num + sizeF.Height))
		{
			return;
		}
		try
		{
			string fullPath = Path.GetFullPath(tileInfo.LocalFile);
			Process.Start(fullPath);
		}
		catch (Exception)
		{
		}
	}

	private void _tileControl_MouseUp(object sender, MouseEventArgs e)
	{
		Tile tileAt = _tileControl.GetTileAt(e.Location);
		if (tileAt != null && tileAt.Tag is TileInfo tileInfo && (tileInfo.TileFlag.HasFlag(TileFlag.WaitSend) || tileInfo.TileFlag.HasFlag(TileFlag.Sending) || tileInfo.TileFlag.HasFlag(TileFlag.Recieving)) && mousePointInTileCloseArea(tileAt))
		{
			tileAt.Image2 = imgTileCloseSlide;
			previousMouseSlideTile = tileAt;
		}
	}

	private void _tileControl_MouseDown(object sender, MouseEventArgs e)
	{
		Tile tileAt = _tileControl.GetTileAt(e.Location);
		if (tileAt != null && tileAt.Tag is TileInfo tileInfo && (tileInfo.TileFlag.HasFlag(TileFlag.WaitSend) || tileInfo.TileFlag.HasFlag(TileFlag.Sending) || tileInfo.TileFlag.HasFlag(TileFlag.Recieving)) && mousePointInTileCloseArea(tileAt))
		{
			tileAt.Image2 = imgTileCloseDown;
			previousMouseSlideTile = tileAt;
		}
	}

	public void SetTheme()
	{
		_tileControl.TileBorderColor = Color.Transparent;
		if (Theme.SelectedLeqiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			imageProcess.SetImageStrategy(new WhiteImageStrategy());
		}
		else
		{
			imageProcess.SetImageStrategy(new DefaultImageStrategy());
		}
		imageProcess.ProcessImage();
		Color darkColor = Theme.SelectedLeqiTheme.ThemeContext.DarkColor;
		selectBorderPen = new Pen(darkColor, 4f);
		solidBrush = new SolidBrush(Color.FromArgb(100, darkColor.R, darkColor.G, darkColor.B));
	}
}
