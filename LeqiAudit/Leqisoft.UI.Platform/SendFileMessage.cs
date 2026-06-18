using System;
using System.Windows.Forms;
using FileTransferModel;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.SignalR;
using Leqisoft.UI.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Leqisoft.UI.Platform;

public class SendFileMessage : IActionMessage
{
	public SendFileMessage(string sendId, string sendName, NotifyMessage mpg)
	{
		base.Content = Tuple.Create(sendId, sendName, mpg);
		ActionDeal = async delegate(object content)
		{
			if (content is Tuple<string, string, NotifyMessage> { Item1: var item, Item2: var item2 })
			{
				FileInfo fileInfo = JsonConvert.DeserializeObject<FileInfo>(mpg.Value.ToString());
				if (DialogResult.OK == Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, item2 + "向您发送来账套文件：" + fileInfo.Name + "，是否现在接收？", MessageBoxButtons.OKCancel))
				{
					if (FileTranferManager.GetInstance().CanceledFileSet.Contains(fileInfo.Id))
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件发送已被发送者取消！");
					}
					else
					{
						FileTranferManager.GetInstance().RecieveFileInfo(sendId, Leqisoft.Model.User.Current.Id.ToString(), fileInfo);
						JObject jObject = new JObject
						{
							{ "fileId", fileInfo.Id },
							{ "recieve", true }
						};
						await SignalRClient.SendToUser(item, new NotifyMessage
						{
							Kind = "sendfileresponse",
							Bullet = true,
							Text = Leqisoft.Model.User.Current.Name + "接受了发送的文件",
							Value = jObject.ToString()
						}.ToString());
						AppCommandTabs.Ledger.Select();
					}
				}
				else
				{
					JObject jObject2 = new JObject
					{
						{ "fileId", fileInfo.Id },
						{ "recieve", false }
					};
					await SignalRClient.SendToUser(item, new NotifyMessage
					{
						Kind = "sendfileresponse",
						Bullet = true,
						Text = Leqisoft.Model.User.Current.Name + "拒绝了发送的文件",
						Value = jObject2.ToString()
					}.ToString());
				}
			}
		};
	}
}
