using System;
using System.Windows.Forms;
using C1.Win.C1Preview;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.LedgerView;

public class PreviewEditor
{
	private LedgerViewer _owner;

	private bool _isPreview;

	public C1PreviewPane Preview;

	public LedgerPrinter _printer;

	private DateTime StartDate => _owner.StartDate;

	private DateTime EndDate => _owner.EndDate;

	private Ledger Ledger => _owner.Ledger;

	public bool IsPreview
	{
		get
		{
			return _isPreview;
		}
		set
		{
			_isPreview = value;
		}
	}

	public PreviewEditor(LedgerViewer owner)
	{
		_owner = owner;
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		Preview = new C1PreviewPane();
		Preview.Dock = DockStyle.Fill;
	}

	private LedgerPrinter CreatePrinter()
	{
		if (Ledger == null)
		{
			throw new ArgumentOutOfRangeException("请先打开账套后操作");
		}
		switch (_owner.CurrentView)
		{
		case ActiveView.Balance:
		{
			BalancePrinter balancePrinter = new BalancePrinter(Ledger, Ledger.RootAccounts);
			balancePrinter.StartDate = StartDate;
			balancePrinter.EndDate = EndDate;
			return balancePrinter;
		}
		case ActiveView.Subsidiary:
		{
			SubsidiaryLedger subsidiaryLedger = Ledger.GetSubsidiaryLedger(_owner.CurrentAccount, StartDate, EndDate);
			object auxiliary = null;
			if (_owner.AccountTreeEditor.Tree.Row >= 0)
			{
				object userData = _owner.AccountTreeEditor.Tree.Rows[_owner.AccountTreeEditor.Tree.Row].UserData;
				if (!(userData is Tuple<Account, AuxiliaryClass> tuple))
				{
					if (userData is Tuple<Account, AuxiliaryItem> tuple2)
					{
						auxiliary = tuple2.Item2;
						subsidiaryLedger = Ledger.GetSubsidiaryLedger(tuple2.Item1, StartDate, EndDate, tuple2.Item2);
					}
				}
				else
				{
					auxiliary = tuple.Item2;
					subsidiaryLedger = Ledger.GetSubsidiaryLedger(tuple.Item1, StartDate, EndDate, tuple.Item2);
				}
			}
			SubsidiaryPrinter subsidiaryPrinter = new SubsidiaryPrinter(Ledger, subsidiaryLedger, auxiliary);
			subsidiaryPrinter.TotalFlag = _owner.SubsidiaryEditor.TotalFlagDisplay;
			subsidiaryPrinter.StartDate = StartDate;
			subsidiaryPrinter.EndDate = EndDate;
			return subsidiaryPrinter;
		}
		case ActiveView.VoucherList:
		{
			VoucherPrinter voucherPrinter = new VoucherPrinter(Ledger, _owner.VoucherListEditor.Vouchers);
			voucherPrinter.StartDate = StartDate;
			voucherPrinter.EndDate = EndDate;
			return voucherPrinter;
		}
		case ActiveView.MarkVoucers:
		{
			MarkVouchersPrinter markVouchersPrinter = new MarkVouchersPrinter(Ledger, _owner.VoucherMarkedEditor.GetVouchers());
			markVouchersPrinter.StartDate = StartDate;
			markVouchersPrinter.EndDate = EndDate;
			return markVouchersPrinter;
		}
		default:
			throw new PreviewNotSupport("当前页不支持打印操作");
		}
	}

	public void PrintPreview()
	{
		_printer = CreatePrinter();
		_printer.Landscape = false;
		_printer.Build();
		Preview.Document = _printer.PrintDocument;
	}

	public void Print()
	{
		if (_printer == null)
		{
			_printer = CreatePrinter();
			_printer.Landscape = false;
			_printer.Build();
		}
		_printer.PrintDocument.PrintDialog();
	}

	public LedgerExporter CreateExporter()
	{
		if (Ledger == null)
		{
			throw new ArgumentOutOfRangeException("请先打开账套后操作");
		}
		switch (_owner.CurrentView)
		{
		case ActiveView.Balance:
		{
			BalanceExporter balanceExporter = new BalanceExporter(Ledger, Ledger.RootAccounts);
			balanceExporter.StartDate = StartDate;
			balanceExporter.EndDate = EndDate;
			return balanceExporter;
		}
		case ActiveView.Subsidiary:
		{
			SubsidiaryExporter subsidiaryExporter = new SubsidiaryExporter(Ledger, (_owner.CurrentAuxiliary == null) ? Ledger.GetSubsidiaryLedger(_owner.CurrentAccount, StartDate, EndDate) : Ledger.GetSubsidiaryLedger(_owner.CurrentAccount, StartDate, EndDate, (AuxiliaryItem)_owner.CurrentAuxiliary));
			subsidiaryExporter.TotalFlag = _owner.SubsidiaryEditor.TotalFlagDisplay;
			subsidiaryExporter.StartDate = StartDate;
			subsidiaryExporter.EndDate = EndDate;
			return subsidiaryExporter;
		}
		case ActiveView.VoucherList:
		{
			VoucherExporter voucherExporter = new VoucherExporter(Ledger, _owner.VoucherListEditor.Vouchers);
			voucherExporter.StartDate = StartDate;
			voucherExporter.EndDate = EndDate;
			return voucherExporter;
		}
		case ActiveView.MarkVoucers:
		{
			MarkedVouchersExporter markedVouchersExporter = new MarkedVouchersExporter(Ledger, _owner.VoucherMarkedEditor.GetVouchers());
			markedVouchersExporter.StartDate = StartDate;
			markedVouchersExporter.EndDate = EndDate;
			return markedVouchersExporter;
		}
		case ActiveView.MonthSummary:
			return new MothSummaryExporter(_owner.SummaryEditor);
		case ActiveView.AgeAnalazy:
			return new LedgerAgingExporter(_owner.LedgerAgingEditor);
		default:
			throw new PreviewNotSupport("当前页不支持导出操作");
		}
	}

	public void SaveToExcel()
	{
		LedgerExporter ledgerExporter = CreateExporter();
		ledgerExporter.Build();
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Filter = "Excel|*.xlsx",
			DefaultExt = ".xlsx"
		};
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			ledgerExporter.xlBook.Save(saveFileDialog.FileName);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件导出成功.");
		}
	}
}
