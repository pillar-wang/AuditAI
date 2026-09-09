using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using C1.Win.C1Command;

namespace Auditai.UI.Controls
{
	/// <summary>
	/// C1Command 右键菜单 -> 原生 ContextMenuStrip 的通用转换器。
	///
	/// 背景：C1.Win.C1Command 的弹出菜单窗口不支持 PerMonitorV2 DPI（老控件按 96dpi 绘制后被
	/// 系统位图拉伸），在高分屏下菜单文字发虚。原生 ContextMenuStrip 走系统 GDI/ClearType
	/// 渲染，任意 DPI 下文字清晰。
	///
	/// 用法：把所有 `xxx.ShowContextMenu(host, pt)` 替换成 `NativeMenuShim.Show(xxx, host, pt)`。
	/// 命令仍注册在原 C1Command 上，点击经 <see cref="C1Command.PerformClick"/> 走原分发逻辑，
	/// 功能零改动；Popup / CommandStateQuery / Closed 事件桥接保留原动态行为。
	/// </summary>
	public static class NativeMenuShim
	{
		private static readonly MethodInfo _firePopup = typeof(C1CommandMenu).GetMethod("FirePopupEvent", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo _fireClosed = typeof(C1CommandMenu).GetMethod("FireClosedEvent", BindingFlags.Instance | BindingFlags.NonPublic);

		private static readonly Font MenuFont = new Font("微软雅黑", 9.5f, FontStyle.Regular, GraphicsUnit.Point, 134);

		/// <summary>以原生菜单展示 C1 命令菜单（自动触发 Popup / CommandStateQuery，关闭时触发 Closed）。</summary>
		public static void Show(C1ContextMenu menu, Control host, Point clientPoint)
		{
			if (menu == null || host == null)
			{
				return;
			}
			FirePopup(menu);
			ContextMenuStrip strip = new ContextMenuStrip
			{
				Font = MenuFont,
				ShowImageMargin = true
			};
			AppendLinks(menu, strip.Items);
			// 菜单关闭时桥接 C1 的 Closed 事件（原保存/清理逻辑照常执行），并释放构建的临时菜单
			strip.Closed += delegate
			{
				FireClosed(menu);
				strip.Dispose();
			};
			strip.Show(host, clientPoint);
		}

		/// <summary>为已通过 <c>holder.SetC1ContextMenu</c> 绑定的控件接入原生右键菜单（替换 C1 自动弹出）。</summary>
		public static void Wire(Control host, C1ContextMenu menu)
		{
			if (host == null || menu == null)
			{
				return;
			}
			host.MouseUp += delegate(object s, MouseEventArgs e)
			{
				if (e.Button == MouseButtons.Right)
				{
					Show(menu, host, e.Location);
				}
			};
		}

		/// <summary>触发 Popup 事件（让调用方动态构建 CommandLinks 的逻辑执行）。</summary>
		private static void FirePopup(C1CommandMenu menu)
		{
			try
			{
				_firePopup?.Invoke(menu, null);
			}
			catch (Exception)
			{
				// 个别 Popup 处理器异常不应阻断菜单显示
			}
		}

		/// <summary>触发 Closed 事件（原关闭保存逻辑）。</summary>
		private static void FireClosed(C1CommandMenu menu)
		{
			try
			{
				_fireClosed?.Invoke(menu, null);
			}
			catch (Exception)
			{
			}
		}

		/// <summary>把 C1 命令链接集合递归转换为原生菜单项。</summary>
		private static void AppendLinks(C1CommandMenu owner, ToolStripItemCollection target)
		{
			foreach (object obj in owner.CommandLinks)
			{
				if (!(obj is C1CommandLink link))
				{
					continue;
				}
				C1Command cmd = link.Command;
				if (cmd == null)
				{
					continue;
				}
				// 先执行 StateQuery，让 CommandStateQuery 处理器刷新 Text/Visible/Enabled/Checked
				try
				{
					cmd.StateQuery();
				}
				catch (Exception)
				{
				}
				if (!cmd.Visible)
				{
					continue;
				}
				if (link.Delimiter)
				{
					target.Add(new ToolStripSeparator());
				}
				if (cmd is C1CommandMenu subMenu)
				{
					AppendSubMenu(subMenu, target);
				}
				else
				{
					AppendCommandItem(cmd, target);
				}
			}
		}

		/// <summary>原生子菜单节点：打开前重新触发 Popup 并重建子项；CloseOnItemClick=false 时点击不关闭（支持连续勾选）。</summary>
		private static void AppendSubMenu(C1CommandMenu subMenu, ToolStripItemCollection target)
		{
			ToolStripMenuItem parent = new ToolStripMenuItem(subMenu.Text)
			{
				Enabled = subMenu.Enabled,
				Image = subMenu.Image
			};
			// 首次构建：触发子菜单 Popup（动态构建）+ 递归填充
			FirePopup(subMenu);
			AppendLinks(subMenu, parent.DropDownItems);
			bool keepOpen = !subMenu.CloseOnItemClick;
			parent.DropDownOpening += delegate
			{
				// 每次展开重新触发 Popup（动态重建）+ 重建子项，保证状态最新
				try
				{
					subMenu.StateQuery();
				}
				catch (Exception)
				{
				}
				FirePopup(subMenu);
				parent.DropDownItems.Clear();
				AppendLinks(subMenu, parent.DropDownItems);
			};
			if (keepOpen)
			{
				parent.DropDown.Closing += delegate(object s, ToolStripDropDownClosingEventArgs e)
				{
					if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
					{
						e.Cancel = true;
					}
				};
			}
			target.Add(parent);
		}

		/// <summary>原生命令项：点击转 C1Command.PerformClick（内部处理 CheckAutoToggle），并回写勾选显示。</summary>
		private static void AppendCommandItem(C1Command cmd, ToolStripItemCollection target)
		{
			ToolStripMenuItem item = new ToolStripMenuItem(cmd.Text)
			{
				Enabled = cmd.Enabled,
				Image = cmd.Image,
				Checked = cmd.Checked
			};
			C1Command captured = cmd;
			item.Click += delegate
			{
				try
				{
					captured.PerformClick();
				}
				catch (Exception)
				{
				}
				// PerformClick 内部已按 AutoToggle 切换 Checked，回写显示使其即时生效
				item.Checked = captured.Checked;
			};
			target.Add(item);
		}
	}
}