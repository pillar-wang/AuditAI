﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Leqisoft.Model;
using Leqisoft.UI.Controls.CollectCell;
using Leqisoft.UI.Controls.Properties;

namespace Leqisoft.UI.Controls.CellCollect;

public class frmCellCollect : C1RibbonForm
{
	private enum View
	{
		Balance,
		Subsidiary
	}

	private View activeView;

	private frmFillGuide fillGuidBox;

	private BalanceEditor balanceEditor;

	private SubsidiayEditor subsidiayEditor;

	private CollectorManager collectManager;

	public EventHandler<DateTime> StartTimeChanged;

	public EventHandler<DateTime> EndTimeChanged;

	private Form owner;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlHeader;

	private C1Label lblMonthEnd;

	private C1Label lblMonthStart;

	private C1Label lblCollectData;

	private C1Label lblCollectObject;

	internal C1ComboBox comboEndMonth;

	internal C1ComboBox comboStartMonth;

	private C1ComboBox comboCollectObject;

	private C1SplitterPanel pnlDockingTab;

	private C1DockingTab DockingTab;

	private C1DockingTabPage tabBalance;

	internal C1FlexGrid grdBalance;

	private C1DockingTabPage tabSubsidiary;

	internal C1FlexGrid grdSubsidiary;

	private C1SplitterPanel pnlBottomBtn;

	private C1Button btnConfirm;

	private C1Button btnCancel;

	private C1Button btnFillGuid;

	private C1Button btnIntelligentFill;

	private C1CommandLink c1CommandLink1;

	public C1CommandHolder holder;

	internal Ledger Ledger { get; set; }

	public int Row { get; set; }

	public int Column { get; set; }

	internal DateTime StartTime { get; set; }

	internal DateTime EndTime { get; set; }

	public decimal? Value { get; private set; }

	public string Formula { get; private set; }

	public frmCellCollect(Ledger ledger, Table table, Tuple<DateTime, DateTime> initPeriod, Form owner)
	{
		InitializeComponent();
		base.Shown += FrmCellCollect_Shown;
		this.owner = owner;
		Ledger = ledger;
		collectManager = new CollectorManager(ledger, table, initPeriod);
		Initialize(ledger, table);
	}

	private void FrmCellCollect_Shown(object sender, EventArgs e)
	{
		base.Icon = Theme.SelectedLeqiTheme.GetThemedIcon(Resources.CellCollect16);
	}

	public new DialogResult ShowDialog()
	{
		DockingTab.ShowTabs = false;
		CollectManager collector = collectManager.Collector;
		switch (collector.CollectObject)
		{
		case CollectObjectEnum.Balance:
		{
			SwitchToView(View.Balance);
			IEnumerable<BalanceItem> enumerable2 = collector.CollectItems.OfType<BalanceItem>();
			balanceEditor.PopulateCollects(enumerable2);
			if (enumerable2.Count() > 0)
			{
				BalanceItem balanceItem = enumerable2.First();
				comboStartMonth.Text = balanceItem.StartTime.Month.ToString();
				comboEndMonth.Text = balanceItem.EndTime.Month.ToString();
			}
			break;
		}
		case CollectObjectEnum.Subsidiary:
		{
			SwitchToView(View.Subsidiary);
			IEnumerable<SubsidiaryItem> enumerable = collector.CollectItems.OfType<SubsidiaryItem>();
			subsidiayEditor.PopulateCollects(enumerable);
			if (enumerable.Count() > 0)
			{
				SubsidiaryItem subsidiaryItem = enumerable.First();
				comboStartMonth.Text = subsidiaryItem.StartTime.Month.ToString();
				comboEndMonth.Text = subsidiaryItem.EndTime.Month.ToString();
			}
			break;
		}
		}
		Theme.SetCurrentTree(this);
		return ShowDialog(owner);
	}

	public void LoadFormula(string formula)
	{
		collectManager.LoadFormula(formula);
	}

	private void CbxCollectObject_TextChanged(object sender, EventArgs e)
	{
		if (comboCollectObject.SelectedIndex == 0)
		{
			SwitchToView(View.Balance);
		}
		else if (comboCollectObject.SelectedIndex == 1)
		{
			SwitchToView(View.Subsidiary);
		}
	}

	private void ComboStartMonth_TextChanged(object sender, EventArgs e)
	{
		if (int.TryParse(comboStartMonth.Text.Trim(), out var result))
		{
			if (result >= 1 && result <= 12)
			{
				StartTime = new DateTime(collectManager.TitlePeriod.Item1.Year, result, 1);
			}
			else
			{
				comboStartMonth.Text = "1";
				StartTime = new DateTime(collectManager.TitlePeriod.Item1.Year, 1, 1);
			}
		}
		else
		{
			comboStartMonth.Text = "1";
			StartTime = new DateTime(collectManager.TitlePeriod.Item1.Year, 1, 1);
		}
		StartTimeChanged?.Invoke(this, StartTime);
	}

	private void ComboEndMonth_TextChanged(object sender, EventArgs e)
	{
		if (int.TryParse(comboEndMonth.Text.Trim(), out var result))
		{
			if (result >= 1 && result <= 12)
			{
				EndTime = new DateTime(collectManager.TitlePeriod.Item1.Year, result, DateTime.DaysInMonth(collectManager.TitlePeriod.Item1.Year, result));
			}
			else
			{
				comboEndMonth.Text = "12";
				EndTime = new DateTime(collectManager.TitlePeriod.Item1.Year, 12, DateTime.DaysInMonth(collectManager.TitlePeriod.Item1.Year, 12));
			}
		}
		else
		{
			comboEndMonth.Text = "12";
			EndTime = new DateTime(collectManager.TitlePeriod.Item1.Year, 12, DateTime.DaysInMonth(collectManager.TitlePeriod.Item1.Year, 12));
		}
		EndTimeChanged?.Invoke(this, EndTime);
	}

	public void btnConfirm_Click(object sender, EventArgs e)
	{
		string formula = null;
		try
		{
			base.DialogResult = DialogResult.OK;
			switch (activeView)
			{
			case View.Balance:
				collectManager.Collector.CollectObject = CollectObjectEnum.Balance;
				collectManager.Collector.CollectItems = ((IEnumerable<CollectItem>)balanceEditor.Collects).ToList();
				break;
			case View.Subsidiary:
				collectManager.Collector.CollectObject = CollectObjectEnum.Subsidiary;
				collectManager.Collector.CollectItems = ((IEnumerable<CollectItem>)subsidiayEditor.Collects).ToList();
				break;
			}
			collectManager.Apply(out var value, out formula);
			Formula = formula;
			Value = value;
			Close();
		}
		catch (Exception)
		{
			Formula = formula;
			Close();
			throw;
		}
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void btnFillGuid_Click(object sender, EventArgs e)
	{
		fillGuidBox.StartTime = StartTime;
		fillGuidBox.EndTime = EndTime;
		switch (activeView)
		{
		case View.Balance:
			if (fillGuidBox.ShowBalance() == DialogResult.OK)
			{
				IEnumerable<CollectItem> collectItems2 = fillGuidBox.CollectItems;
				balanceEditor.PopulateCollects(collectItems2.Cast<BalanceItem>());
			}
			break;
		case View.Subsidiary:
			if (fillGuidBox.ShowSubsidiary() == DialogResult.OK)
			{
				IEnumerable<CollectItem> collectItems = fillGuidBox.CollectItems;
				subsidiayEditor.PopulateCollects(collectItems.Cast<SubsidiaryItem>());
			}
			break;
		}
	}

	private async void btnIntelligenceFill_Click(object sender, EventArgs e)
	{
		if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode && DictionarySync.CellCollector.Version == 0)
		{
			try
			{
				await DictionarySync.CheckCellCollectDicVersionAndUpdate();
			}
			catch (WebException)
			{
				if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode && DictionarySync.CellCollector.Version == 0)
				{
					System.Windows.Forms.MessageBox.Show("因网络问题，字典更新失败！");
				}
			}
			catch (TimeoutException)
			{
				if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode && DictionarySync.CellCollector.Version == 0)
				{
					MessageBox.Show(MessageBoxIcon.None, "更新字典失败！网络超时,请重试");
				}
			}
			catch (Exception ex3)
			{
				if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode && DictionarySync.CellCollector.Version == 0)
				{
					MessageBox.Show(MessageBoxIcon.None, "更新字典失败！" + ex3.Message + ",请重试");
				}
			}
		}
		collectManager.Intelligence(Row, Column);
		if (collectManager.Collector.CollectItems != null)
		{
			IEnumerable<BalanceItem> enumerable = collectManager.Collector.CollectItems.OfType<BalanceItem>();
			balanceEditor.PopulateCollects(enumerable);
			if (enumerable.Count() > 0)
			{
				BalanceItem balanceItem = enumerable.First();
				comboStartMonth.Text = balanceItem.StartTime.Month.ToString();
				comboEndMonth.Text = balanceItem.EndTime.Month.ToString();
			}
		}
	}

	private void CellCollectBox_Resize(object sender, EventArgs e)
	{
		Update();
	}

	private void Initialize(Ledger ledger, Table table)
	{
		base.AcceptButton = btnConfirm;
		DockingTab.ShowTabs = false;
		base.DialogResult = DialogResult.None;
		base.StartPosition = FormStartPosition.CenterScreen;
		grdBalance.Paint += delegate(object s1, PaintEventArgs e1)
		{
			grdBalance.DrawFormBorder(e1.Graphics);
		};
		grdSubsidiary.Paint += delegate(object s1, PaintEventArgs e1)
		{
			grdSubsidiary.DrawFormBorder(e1.Graphics);
		};
		comboStartMonth.TextChanged += ComboStartMonth_TextChanged;
		comboEndMonth.TextChanged += ComboEndMonth_TextChanged;
		comboStartMonth.Text = "1";
		comboEndMonth.Text = "12";
		comboCollectObject.TextChanged += CbxCollectObject_TextChanged;
		comboCollectObject.SelectedIndex = 0;
		fillGuidBox = new frmFillGuide(ledger, collectManager.TitlePeriod.Item1.Year, this);
		balanceEditor = new BalanceEditor(this, collectManager.TitlePeriod.Item1.Year);
		subsidiayEditor = new SubsidiayEditor(this, collectManager.TitlePeriod.Item1.Year);
	}

	private void SwitchToView(View view)
	{
		activeView = view;
		switch (activeView)
		{
		case View.Balance:
			DockingTab.SelectedTab = tabBalance;
			if (comboCollectObject.SelectedIndex != 0)
			{
				comboCollectObject.SelectedIndex = 0;
			}
			setTimeControlVisible(display: true);
			break;
		case View.Subsidiary:
			DockingTab.SelectedTab = tabSubsidiary;
			if (comboCollectObject.SelectedIndex != 1)
			{
				comboCollectObject.SelectedIndex = 1;
			}
			setTimeControlVisible(display: false);
			break;
		}
		void setTimeControlVisible(bool display)
		{
			lblCollectData.Visible = display;
			lblMonthStart.Visible = display;
			lblMonthEnd.Visible = display;
			comboEndMonth.Visible = display;
			comboStartMonth.Visible = display;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlHeader = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.lblMonthEnd = new C1.Win.C1Input.C1Label();
		this.lblMonthStart = new C1.Win.C1Input.C1Label();
		this.comboStartMonth = new C1.Win.C1Input.C1ComboBox();
		this.lblCollectObject = new C1.Win.C1Input.C1Label();
		this.comboEndMonth = new C1.Win.C1Input.C1ComboBox();
		this.comboCollectObject = new C1.Win.C1Input.C1ComboBox();
		this.lblCollectData = new C1.Win.C1Input.C1Label();
		this.pnlBottomBtn = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnConfirm = new C1.Win.C1Input.C1Button();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.btnFillGuid = new C1.Win.C1Input.C1Button();
		this.btnIntelligentFill = new C1.Win.C1Input.C1Button();
		this.pnlDockingTab = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.DockingTab = new C1.Win.C1Command.C1DockingTab();
		this.tabBalance = new C1.Win.C1Command.C1DockingTabPage();
		this.grdBalance = new C1.Win.C1FlexGrid.C1FlexGrid();
		this.tabSubsidiary = new C1.Win.C1Command.C1DockingTabPage();
		this.grdSubsidiary = new C1.Win.C1FlexGrid.C1FlexGrid();
		this.c1CommandLink1 = new C1.Win.C1Command.C1CommandLink();
		this.holder = new C1.Win.C1Command.C1CommandHolder();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlHeader.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.lblMonthEnd).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblMonthStart).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.comboStartMonth).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblCollectObject).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.comboEndMonth).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.comboCollectObject).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblCollectData).BeginInit();
		this.pnlBottomBtn.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnFillGuid).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnIntelligentFill).BeginInit();
		this.pnlDockingTab.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.DockingTab).BeginInit();
		this.DockingTab.SuspendLayout();
		this.tabBalance.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdBalance).BeginInit();
		this.tabSubsidiary.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdSubsidiary).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.holder).BeginInit();
		base.SuspendLayout();
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.ctnAll.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.ctnAll.HeaderHeight = 27;
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlHeader);
		this.ctnAll.Panels.Add(this.pnlBottomBtn);
		this.ctnAll.Panels.Add(this.pnlDockingTab);
		this.ctnAll.Size = new System.Drawing.Size(780, 411);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 0;
		this.pnlHeader.Controls.Add(this.lblMonthEnd);
		this.pnlHeader.Controls.Add(this.lblMonthStart);
		this.pnlHeader.Controls.Add(this.comboStartMonth);
		this.pnlHeader.Controls.Add(this.lblCollectObject);
		this.pnlHeader.Controls.Add(this.comboEndMonth);
		this.pnlHeader.Controls.Add(this.comboCollectObject);
		this.pnlHeader.Controls.Add(this.lblCollectData);
		this.pnlHeader.Height = 52;
		this.pnlHeader.KeepRelativeSize = false;
		this.pnlHeader.Location = new System.Drawing.Point(0, 0);
		this.pnlHeader.MinHeight = 52;
		this.pnlHeader.MinWidth = 52;
		this.pnlHeader.Name = "pnlHeader";
		this.pnlHeader.Resizable = false;
		this.pnlHeader.Size = new System.Drawing.Size(780, 52);
		this.pnlHeader.SizeRatio = 6.0;
		this.pnlHeader.TabIndex = 0;
		this.pnlHeader.Width = 780;
		this.lblMonthEnd.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblMonthEnd.AutoSize = true;
		this.lblMonthEnd.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMonthEnd.Location = new System.Drawing.Point(748, 17);
		this.lblMonthEnd.Name = "lblMonthEnd";
		this.lblMonthEnd.Size = new System.Drawing.Size(20, 17);
		this.lblMonthEnd.TabIndex = 7;
		this.lblMonthEnd.Tag = null;
		this.lblMonthEnd.Text = "月";
		this.lblMonthEnd.TextDetached = true;
		this.lblMonthStart.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblMonthStart.AutoSize = true;
		this.lblMonthStart.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMonthStart.Location = new System.Drawing.Point(641, 16);
		this.lblMonthStart.Name = "lblMonthStart";
		this.lblMonthStart.Size = new System.Drawing.Size(33, 17);
		this.lblMonthStart.TabIndex = 6;
		this.lblMonthStart.Tag = null;
		this.lblMonthStart.Text = "月 - ";
		this.lblMonthStart.TextDetached = true;
		this.comboStartMonth.AllowSpinLoop = false;
		this.comboStartMonth.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.comboStartMonth.GapHeight = 0;
		this.comboStartMonth.ImagePadding = new System.Windows.Forms.Padding(0);
		this.comboStartMonth.Items.Add("1");
		this.comboStartMonth.Items.Add("2");
		this.comboStartMonth.Items.Add("3");
		this.comboStartMonth.Items.Add("4");
		this.comboStartMonth.Items.Add("5");
		this.comboStartMonth.Items.Add("6");
		this.comboStartMonth.Items.Add("7");
		this.comboStartMonth.Items.Add("8");
		this.comboStartMonth.Items.Add("9");
		this.comboStartMonth.Items.Add("10");
		this.comboStartMonth.Items.Add("11");
		this.comboStartMonth.Items.Add("12");
		this.comboStartMonth.ItemsDisplayMember = "";
		this.comboStartMonth.ItemsValueMember = "";
		this.comboStartMonth.Location = new System.Drawing.Point(588, 14);
		this.comboStartMonth.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.comboStartMonth.Name = "comboStartMonth";
		this.comboStartMonth.Size = new System.Drawing.Size(47, 21);
		this.comboStartMonth.TabIndex = 1;
		this.comboStartMonth.Tag = null;
		this.comboStartMonth.TextDetached = true;
		this.lblCollectObject.AutoSize = true;
		this.lblCollectObject.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblCollectObject.Location = new System.Drawing.Point(14, 17);
		this.lblCollectObject.Name = "lblCollectObject";
		this.lblCollectObject.Size = new System.Drawing.Size(68, 17);
		this.lblCollectObject.TabIndex = 3;
		this.lblCollectObject.Tag = null;
		this.lblCollectObject.Text = "采集对象：";
		this.lblCollectObject.TextDetached = true;
		this.comboEndMonth.AllowSpinLoop = false;
		this.comboEndMonth.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.comboEndMonth.GapHeight = 0;
		this.comboEndMonth.ImagePadding = new System.Windows.Forms.Padding(0);
		this.comboEndMonth.Items.Add("1");
		this.comboEndMonth.Items.Add("2");
		this.comboEndMonth.Items.Add("3");
		this.comboEndMonth.Items.Add("4");
		this.comboEndMonth.Items.Add("5");
		this.comboEndMonth.Items.Add("6");
		this.comboEndMonth.Items.Add("7");
		this.comboEndMonth.Items.Add("8");
		this.comboEndMonth.Items.Add("9");
		this.comboEndMonth.Items.Add("10");
		this.comboEndMonth.Items.Add("11");
		this.comboEndMonth.Items.Add("12");
		this.comboEndMonth.ItemsDisplayMember = "";
		this.comboEndMonth.ItemsValueMember = "";
		this.comboEndMonth.Location = new System.Drawing.Point(680, 14);
		this.comboEndMonth.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.comboEndMonth.Name = "comboEndMonth";
		this.comboEndMonth.Size = new System.Drawing.Size(62, 21);
		this.comboEndMonth.TabIndex = 2;
		this.comboEndMonth.Tag = null;
		this.comboEndMonth.TextDetached = true;
		this.comboCollectObject.AllowSpinLoop = false;
		this.comboCollectObject.GapHeight = 0;
		this.comboCollectObject.ImagePadding = new System.Windows.Forms.Padding(0);
		this.comboCollectObject.Items.Add("科目余额表");
		this.comboCollectObject.Items.Add("明细账");
		this.comboCollectObject.ItemsDisplayMember = "";
		this.comboCollectObject.ItemsValueMember = "";
		this.comboCollectObject.Location = new System.Drawing.Point(83, 13);
		this.comboCollectObject.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.comboCollectObject.Name = "comboCollectObject";
		this.comboCollectObject.Size = new System.Drawing.Size(131, 21);
		this.comboCollectObject.TabIndex = 0;
		this.comboCollectObject.Tag = null;
		this.comboCollectObject.TextDetached = true;
		this.lblCollectData.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblCollectData.AutoSize = true;
		this.lblCollectData.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblCollectData.Location = new System.Drawing.Point(519, 16);
		this.lblCollectData.Name = "lblCollectData";
		this.lblCollectData.Size = new System.Drawing.Size(68, 17);
		this.lblCollectData.TabIndex = 5;
		this.lblCollectData.Tag = null;
		this.lblCollectData.Text = "会计期间：";
		this.lblCollectData.TextDetached = true;
		this.pnlBottomBtn.Controls.Add(this.btnConfirm);
		this.pnlBottomBtn.Controls.Add(this.btnCancel);
		this.pnlBottomBtn.Controls.Add(this.btnFillGuid);
		this.pnlBottomBtn.Controls.Add(this.btnIntelligentFill);
		this.pnlBottomBtn.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlBottomBtn.Height = 60;
		this.pnlBottomBtn.KeepRelativeSize = false;
		this.pnlBottomBtn.Location = new System.Drawing.Point(0, 351);
		this.pnlBottomBtn.MinHeight = 60;
		this.pnlBottomBtn.MinWidth = 52;
		this.pnlBottomBtn.Name = "pnlBottomBtn";
		this.pnlBottomBtn.Resizable = false;
		this.pnlBottomBtn.Size = new System.Drawing.Size(780, 60);
		this.pnlBottomBtn.TabIndex = 2;
		this.pnlBottomBtn.Width = 780;
		this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnConfirm.Location = new System.Drawing.Point(553, 21);
		this.btnConfirm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size(70, 26);
		this.btnConfirm.TabIndex = 3;
		this.btnConfirm.Text = "确定";
		this.btnConfirm.UseVisualStyleBackColor = true;
		this.btnConfirm.Click += new System.EventHandler(btnConfirm_Click);
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Location = new System.Drawing.Point(666, 21);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnFillGuid.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnFillGuid.Location = new System.Drawing.Point(442, 21);
		this.btnFillGuid.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnFillGuid.Name = "btnFillGuid";
		this.btnFillGuid.Size = new System.Drawing.Size(70, 26);
		this.btnFillGuid.TabIndex = 1;
		this.btnFillGuid.Text = "设置向导";
		this.btnFillGuid.UseVisualStyleBackColor = true;
		this.btnFillGuid.Click += new System.EventHandler(btnFillGuid_Click);
		this.btnIntelligentFill.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnIntelligentFill.Location = new System.Drawing.Point(329, 21);
		this.btnIntelligentFill.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnIntelligentFill.Name = "btnIntelligentFill";
		this.btnIntelligentFill.Size = new System.Drawing.Size(70, 26);
		this.btnIntelligentFill.TabIndex = 0;
		this.btnIntelligentFill.Text = "智能设置";
		this.btnIntelligentFill.UseVisualStyleBackColor = true;
		this.btnIntelligentFill.Click += new System.EventHandler(btnIntelligenceFill_Click);
		this.pnlDockingTab.Controls.Add(this.DockingTab);
		this.pnlDockingTab.Height = 297;
		this.pnlDockingTab.Location = new System.Drawing.Point(0, 53);
		this.pnlDockingTab.MinHeight = 52;
		this.pnlDockingTab.MinWidth = 52;
		this.pnlDockingTab.Name = "pnlDockingTab";
		this.pnlDockingTab.Size = new System.Drawing.Size(780, 297);
		this.pnlDockingTab.SizeRatio = 92.0;
		this.pnlDockingTab.TabIndex = 1;
		this.pnlDockingTab.Width = 780;
		this.DockingTab.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.DockingTab.Controls.Add(this.tabBalance);
		this.DockingTab.Controls.Add(this.tabSubsidiary);
		this.DockingTab.Dock = System.Windows.Forms.DockStyle.Fill;
		this.DockingTab.Indent = 0;
		this.DockingTab.Location = new System.Drawing.Point(0, 0);
		this.DockingTab.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.DockingTab.Name = "DockingTab";
		this.DockingTab.Size = new System.Drawing.Size(780, 297);
		this.DockingTab.TabIndex = 0;
		this.DockingTab.TabsSpacing = 0;
		this.DockingTab.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.tabBalance.Controls.Add(this.grdBalance);
		this.tabBalance.Location = new System.Drawing.Point(1, 27);
		this.tabBalance.Name = "tabBalance";
		this.tabBalance.Size = new System.Drawing.Size(778, 269);
		this.tabBalance.TabIndex = 0;
		this.tabBalance.Text = "第1页";
		this.grdBalance.AllowSorting = C1.Win.C1FlexGrid.AllowSortingEnum.None;
		this.grdBalance.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this.grdBalance.ColumnInfo = "10,1,0,0,0,100,Columns:";
		this.grdBalance.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdBalance.Location = new System.Drawing.Point(0, 0);
		this.grdBalance.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.grdBalance.Name = "grdBalance";
		this.grdBalance.Rows.DefaultSize = 20;
		this.grdBalance.Size = new System.Drawing.Size(778, 269);
		this.grdBalance.TabIndex = 0;
		this.tabSubsidiary.Controls.Add(this.grdSubsidiary);
		this.tabSubsidiary.Location = new System.Drawing.Point(1, 27);
		this.tabSubsidiary.Name = "tabSubsidiary";
		this.tabSubsidiary.Size = new System.Drawing.Size(778, 269);
		this.tabSubsidiary.TabIndex = 1;
		this.tabSubsidiary.Text = "第2页";
		this.grdSubsidiary.AllowSorting = C1.Win.C1FlexGrid.AllowSortingEnum.None;
		this.grdSubsidiary.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this.grdSubsidiary.ColumnInfo = "10,1,0,0,0,100,Columns:";
		this.grdSubsidiary.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdSubsidiary.Location = new System.Drawing.Point(0, 0);
		this.grdSubsidiary.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.grdSubsidiary.Name = "grdSubsidiary";
		this.grdSubsidiary.Rows.DefaultSize = 20;
		this.grdSubsidiary.Size = new System.Drawing.Size(778, 269);
		this.grdSubsidiary.TabIndex = 0;
		this.c1CommandLink1.Text = "新命令";
		this.holder.Owner = this;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(780, 411);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.MinimumSize = new System.Drawing.Size(500, 300);
		base.Name = "frmCellCollect";
		base.ShowInTaskbar = false;
		this.Text = "单元格采账设置";
		base.Resize += new System.EventHandler(CellCollectBox_Resize);
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlHeader.ResumeLayout(false);
		this.pnlHeader.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.lblMonthEnd).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblMonthStart).EndInit();
		((System.ComponentModel.ISupportInitialize)this.comboStartMonth).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblCollectObject).EndInit();
		((System.ComponentModel.ISupportInitialize)this.comboEndMonth).EndInit();
		((System.ComponentModel.ISupportInitialize)this.comboCollectObject).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblCollectData).EndInit();
		this.pnlBottomBtn.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnFillGuid).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnIntelligentFill).EndInit();
		this.pnlDockingTab.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.DockingTab).EndInit();
		this.DockingTab.ResumeLayout(false);
		this.tabBalance.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdBalance).EndInit();
		this.tabSubsidiary.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdSubsidiary).EndInit();
		((System.ComponentModel.ISupportInitialize)this.holder).EndInit();
		base.ResumeLayout(false);
	}
}
