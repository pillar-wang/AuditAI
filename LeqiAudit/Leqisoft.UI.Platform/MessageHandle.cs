using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using FileTransferModel;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.SignalR;
using Leqisoft.UI.Controls;
using Leqisoft.Util;
using Newtonsoft.Json.Linq;

namespace Leqisoft.UI.Platform;

public class MessageHandle
{
	private delegate void InvokeDelegate();

	public static Dictionary<string, Member> OtherGroupMember = new Dictionary<string, Member>();

	public MessageHandle()
	{
		FileTranferManager instance = FileTranferManager.GetInstance();
		instance.FileTimeout += MessageHandle_FileTimeout;
	}

	private void MessageHandle_FileTimeout(object sender, FileTransferModel.FileInfo e)
	{
		Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件接收超时，任务取消");
	}

	public static async void SignalRClient_MessageReceived(object sender, MessageReceivedEventArgs e)
	{
		string text = Leqisoft.Model.User.Current.Id.ToString();
		try
		{
			MemberManager manager = MemberManager.GetInstance();
			switch (e.Kind)
			{
			case MessageKind.PeerLogin:
			{
				if (long.TryParse(e.FromId, out var result12))
				{
					Member member19 = manager.GetMember(e.FromId);
					if (member19 != null && !(e.FromId == text))
					{
						member19.UserState = new UserState();
						manager.OnOnlineStatusChanged(result12);
					}
				}
				break;
			}
			case MessageKind.PeerLogout:
			{
				if (long.TryParse(e.FromId, out var result7))
				{
					Member member14 = manager.GetMember(e.FromId);
					if (member14 != null && !(e.FromId == text))
					{
						member14.UserState = null;
						manager.OnOnlineStatusChanged(result7);
					}
				}
				break;
			}
			case MessageKind.PeerOpensProject:
			{
				if (!long.TryParse(e.FromId, out var result11))
				{
					break;
				}
				Member member18 = manager.GetMember(e.FromId);
				if (member18 != null && !(e.FromId == text))
				{
					if (member18.UserState == null)
					{
						member18.UserState = new UserState();
					}
					member18.UserState.ProjectId = e.ProjectId;
					member18.UserState.TreeNodeId = null;
					manager.OnOpenProjectChanged(result11);
				}
				break;
			}
			case MessageKind.ProjectSynced:
			{
				if (long.TryParse(e.FromId, out var _))
				{
					Member member17 = manager.GetMember(e.FromId);
					if (member17 != null && !(e.FromId == text))
					{
						manager.OnProjectSynced(e.ProjectId);
					}
				}
				break;
			}
			case MessageKind.PeerOpensTreeNode:
			{
				if (!long.TryParse(e.FromId, out var result2))
				{
					break;
				}
				Member member11 = manager.GetMember(e.FromId);
				if (member11 != null && !(e.FromId == text))
				{
					if (member11.UserState == null)
					{
						member11.UserState = new UserState();
					}
					member11.UserState.ProjectId = e.ProjectId;
					member11.UserState.TreeNodeId = e.NodeId;
					manager.OnOpenTreeNodeChanged(result2);
				}
				break;
			}
			case MessageKind.PeerTableCellChange:
			{
				if (!long.TryParse(e.FromId, out var result15))
				{
					break;
				}
				Member member22 = manager.GetMember(e.FromId);
				if (member22 != null && !(e.FromId == text))
				{
					if (member22.UserState == null)
					{
						member22.UserState = new UserState();
					}
					member22.UserState.TableCellId = e.TableCellId;
					manager.OnTableCellChanged(result15);
				}
				break;
			}
			case MessageKind.PeerParagraphChange:
			{
				if (!long.TryParse(e.FromId, out var result4))
				{
					break;
				}
				Member member12 = manager.GetMember(e.FromId);
				if (member12 != null && !(e.FromId == text))
				{
					if (member12.UserState == null)
					{
						member12.UserState = new UserState();
					}
					member12.UserState.DocParagraphId = e.ParagraphId;
					manager.OnDocParagraphChanged(result4);
				}
				break;
			}
			case MessageKind.PeerPushesTreeNode:
			{
				if (!long.TryParse(e.FromId, out var result9))
				{
					break;
				}
				Member member16 = manager.GetMember(e.FromId);
				if (member16 != null && !(e.FromId == text) && NotifyMessage.TryParse(e.NodeId, out var mpg3) && mpg3.Value != null)
				{
					try
					{
						JObject jObject2 = JObject.Parse(mpg3.Value.ToString());
						long nodeId = long.Parse(jObject2.Value<string>("nodeId"));
						string nodeName = jObject2.Value<string>("nodeName");
						string fromName = jObject2.Value<string>("name");
						manager.OnPushTreeNode(result9, nodeId, nodeName, fromName, mpg3.Text);
					}
					catch (Exception exception)
					{
						exception.Log();
					}
				}
				break;
			}
			case MessageKind.PeerStateUpload:
			{
				if (!long.TryParse(e.FromId, out var result14))
				{
					break;
				}
				Member member21 = manager.GetMember(e.FromId);
				if (member21 != null && !(e.FromId == text))
				{
					if (member21.UserState == null)
					{
						member21.UserState = new UserState();
					}
					member21.UserState.ProjectId = e.ProjectId;
					member21.UserState.TreeNodeId = e.NodeId;
					member21.UserState.TableCellId = e.TableCellId;
					member21.UserState.DocParagraphId = e.ParagraphId;
					manager.OnStatusUploaded(result14);
				}
				break;
			}
			case MessageKind.PeerProjectMembersChanged:
			{
				string currentProject1 = SignalRClient.UserState.ProjectId;
				if (Guid.TryParse(currentProject1, out var result3))
				{
					await ChatManager.UpdateProjectMember(result3);
					manager.OnProjectMembersChanged(currentProject1);
				}
				break;
			}
			case MessageKind.PeerTeamMembersChanged:
				await ChatManager.UpdateTeamMember();
				manager.OnTeamMemberChanged();
				break;
			case MessageKind.PeerMemberInfoChanged:
			{
				string projectId = SignalRClient.UserState.ProjectId;
				if (Guid.TryParse(projectId, out var result5))
				{
					await ChatManager.UpdateProjectMember(result5);
				}
				await ChatManager.UpdateTeamMember();
				manager.OnMemberInfoChanged();
				break;
			}
			case MessageKind.MessageFromUser:
			{
				if (!long.TryParse(e.FromId, out var _) || e.FromId == text || !NotifyMessage.TryParse(e.Message, out var mpg))
				{
					break;
				}
				if (mpg.Kind == "mergemessage")
				{
					if (mpg.Value != null)
					{
						long id = long.Parse(mpg.Value.ToString());
						Leqisoft.DTO.User user = await WebApiClient.GetUserById(id);
						Member member13;
						if (OtherGroupMember.ContainsKey(id.ToString()))
						{
							member13 = OtherGroupMember[id.ToString()];
						}
						else
						{
							member13 = new Member
							{
								Id = id.ToString(),
								Name = user.Name
							};
							member13.SetPicture(user.Picture);
							OtherGroupMember.Add(member13.Id, member13);
						}
						member13.UnhandleActionMessage.Enqueue(new TeamMergeMessage(user.UserName, user.Name, member13.Id));
						if (Program.MainForm != null && Program.MainForm.FormShowned)
						{
							Program.MainForm.HandleSystemMessage();
						}
					}
					break;
				}
				Member member9 = manager.GetMember(e.FromId);
				if (member9 == null)
				{
					break;
				}
				switch (mpg.Kind)
				{
				case "sendfilerequest":
					manager.OnRecieveFileMessage(member9, mpg);
					break;
				case "sendfileresponse":
				{
					JObject jObject = JObject.Parse(mpg.Value.ToString());
					string fileId = jObject.Value<string>("fileId");
					if (!jObject.Value<bool>("recieve"))
					{
						break;
					}
					FileTranferManager fileTransferManager = FileTranferManager.GetInstance();
					if (!fileTransferManager.FileCacheMap.ContainsKey(fileId))
					{
						break;
					}
					FileCache fileCache = fileTransferManager.FileCacheMap[fileId];
					if (!File.Exists(fileCache.LocalFile))
					{
						break;
					}
					fileCache.TransferStart = DateTime.Now;
					fileCache.FileState = FileState.Sending;
					FileSection[] fileSections = FileUtil.GetFileSections(fileCache.LocalFile, fileId);
					FileSection[] array = fileSections;
					foreach (FileSection section in array)
					{
						if (!fileTransferManager.FileCacheMap.ContainsKey(section.Id))
						{
							fileCache.FileState = FileState.SendCancel;
							return;
						}
						await SignalRClient.PushFileSectionToUser(member9.Id.ToString(), section);
						manager.OnSendSection(section);
					}
					fileCache.FileState = FileState.SendComplete;
					manager.OnSendComplete(fileCache);
					FileTranferManager.GetInstance().FileCacheMap.Remove(fileId);
					break;
				}
				case "cancelrecieve":
				{
					string text3 = mpg.Value?.ToString();
					FileTranferManager.GetInstance().CanceledFileSet.Add(text3);
					Dictionary<string, FileCache> fileCacheMap2 = FileTranferManager.GetInstance().FileCacheMap;
					if (text3 != null && fileCacheMap2.ContainsKey(text3))
					{
						manager.OnSendCancel(text3);
						fileCacheMap2.Remove(text3);
					}
					break;
				}
				case "cancelsend":
				{
					string text2 = mpg.Value?.ToString();
					FileTranferManager.GetInstance().CanceledFileSet.Add(text2);
					Dictionary<string, FileCache> fileCacheMap = FileTranferManager.GetInstance().FileCacheMap;
					if (text2 != null && fileCacheMap.ContainsKey(text2))
					{
						manager.OnRecieveCancel(text2);
						fileCacheMap.Remove(text2);
					}
					break;
				}
				default:
				{
					TempRecord item = new TempRecord
					{
						ChatId = e.FromId,
						FromId = e.FromId,
						Message = mpg.Text,
						Bullet = mpg.Bullet,
						Value = mpg.Value
					};
					member9.UnhandleNotifyMessage.Enqueue(item);
					manager.OnMessageArrived(e.FromId, e.FromId, mpg);
					break;
				}
				}
				break;
			}
			case MessageKind.ProjectBroadcast:
			{
				if (!long.TryParse(e.FromId, out var _))
				{
					break;
				}
				Member member20 = manager.GetMember(e.FromId);
				if (member20 != null && !(e.FromId == text) && NotifyMessage.TryParse(e.Message, out var mpg4))
				{
					string projectId2 = SignalRClient.UserState.ProjectId;
					TempRecord item3 = new TempRecord
					{
						ChatId = projectId2,
						FromId = e.FromId,
						Message = mpg4.Text,
						Bullet = mpg4.Bullet,
						Value = mpg4.Value
					};
					Group group2 = manager.GetGroup(projectId2);
					if (group2 != null)
					{
						group2.UnhandleNotifyMessage.Enqueue(item3);
						manager.OnMessageArrived(e.FromId, projectId2, mpg4);
					}
				}
				break;
			}
			case MessageKind.TeamBroadcast:
			{
				if (!long.TryParse(e.FromId, out var _))
				{
					break;
				}
				Member member15 = manager.GetMember(e.FromId);
				if (member15 != null && !(e.FromId == text) && NotifyMessage.TryParse(e.Message, out var mpg2))
				{
					string text4 = Leqisoft.Model.User.Current.TeamId.ToString();
					TempRecord item2 = new TempRecord
					{
						ChatId = text4,
						FromId = e.FromId,
						Message = mpg2.Text,
						Bullet = mpg2.Bullet,
						Value = mpg2.Value
					};
					Group group = manager.GetGroup(text4);
					if (group != null)
					{
						group.UnhandleNotifyMessage.Enqueue(item2);
						manager.OnMessageArrived(e.FromId, text4, mpg2);
					}
				}
				break;
			}
			case MessageKind.PeerFileSectionArrived:
			{
				FileTranferManager instance = FileTranferManager.GetInstance();
				if (instance.FileCacheMap.ContainsKey(e.FileSection.Id))
				{
					FileTranferManager.GetInstance().RecieveSection(e.FileSection);
				}
				break;
			}
			case MessageKind.PeerOpenTicketNavTreeNode:
			{
				if (!long.TryParse(e.FromId, out var _))
				{
					break;
				}
				Member member10 = manager.GetMember(e.FromId);
				if (member10 != null && !(e.FromId == text))
				{
					if (member10.UserState == null)
					{
						member10.UserState = new UserState();
					}
					member10.UserState.ProjectId = e.ProjectId;
					member10.UserState.TreeNodeId = e.NodeId;
					member10.UserState.TicketNavTreeNodePath = e.TicketNavTreeNodePath;
					manager.OnOpenTicketNavTreeNodeChanged(e.FromId, e.ProjectId, e.NodeId, e.TicketNavTreeNodePath);
				}
				break;
			}
			}
		}
		catch (Exception exception2)
		{
			exception2.Log();
		}
	}
}
