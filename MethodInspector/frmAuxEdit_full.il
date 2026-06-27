=== TYPE: Leqisoft.UI.Platform.frmAuxEdit ===
Fields (84):
  System.Boolean _allowFreeInput
  System.Boolean _allowMultiSelect
  System.Boolean _attachEvent
  System.String _commentNotice
  System.String _commentValue
  System.String _defaultNotice
  System.String _defaultValue
  System.String _dropNotice
  System.String _dropValue
  System.Boolean _isTextChanging
  Leqisoft.UI.Platform.AuxEditor _owner
  System.Boolean <QueryCommentValueChanged>k__BackingField
  System.Boolean <QueryDefaultValueChanged>k__BackingField
  System.Boolean <QueryValueChanged>k__BackingField
  C1.Win.C1Input.C1Button btnCancle
  C1.Win.C1Input.C1Button btnConfirm
  C1.Win.C1Command.C1CommandHolder c1CommandHolder1
  C1.Win.C1Command.C1CommandLink c1CommandLink1
  C1.Win.C1Command.C1ContextMenu c1ContextMenu1
  C1.Win.C1SplitContainer.C1SplitterPanel c1SplitterPanel2
  C1.Win.C1SplitContainer.C1SplitterPanel c1SplitterPanel4
  C1.Win.C1Input.C1CheckBox ckbFreeInput
  C1.Win.C1Input.C1CheckBox ckbMultiSelect
  C1.Win.C1Command.C1Command cmdCopy1
  C1.Win.C1Command.C1Command cmdCopy2
  C1.Win.C1Command.C1Command cmdCopy3
  C1.Win.C1Command.C1Command cmdCut1
  C1.Win.C1Command.C1Command cmdCut2
  C1.Win.C1Command.C1Command cmdCut3
  C1.Win.C1Command.C1Command cmdInputList
  C1.Win.C1Command.C1Command cmdInsertColumn1
  C1.Win.C1Command.C1Command cmdInsertVariable1
  C1.Win.C1Command.C1Command cmdInsertVariable2
  C1.Win.C1Command.C1Command cmdInsertVariable3
  C1.Win.C1Command.C1Command cmdMultiList
  C1.Win.C1Command.C1Command cmdOtherFunc
  C1.Win.C1Command.C1Command cmdPaste1
  C1.Win.C1Command.C1Command cmdPaste2
  C1.Win.C1Command.C1Command cmdPaste3
  C1.Win.C1Command.C1Command cmdTableList
  C1.Win.C1Command.C1Command cmdTreeList
  System.ComponentModel.IContainer components
  C1.Win.C1SplitContainer.C1SplitContainer ctnCommentInput
  C1.Win.C1SplitContainer.C1SplitContainer ctnDefaultInput
  C1.Win.C1SplitContainer.C1SplitContainer ctnDock
  C1.Win.C1SplitContainer.C1SplitContainer ctnDropInput
  C1.Win.C1Command.C1ContextMenu ctx1
  C1.Win.C1Command.C1ContextMenu ctx2
  C1.Win.C1Command.C1ContextMenu ctx3
  C1.Win.C1Command.C1ContextMenu ctxOtherFunc
  C1.Win.C1Command.C1DockingTab DockingTab
  C1.Win.C1Input.C1Label lblFunctionHint
  C1.Win.C1Command.C1CommandLink lnkCopy1
  C1.Win.C1Command.C1CommandLink lnkCopy2
  C1.Win.C1Command.C1CommandLink lnkCopy3
  C1.Win.C1Command.C1CommandLink lnkCut1
  C1.Win.C1Command.C1CommandLink lnkCut2
  C1.Win.C1Command.C1CommandLink lnkCut3
  C1.Win.C1Command.C1CommandLink lnkInputList
  C1.Win.C1Command.C1CommandLink lnkInsertColumn1
  C1.Win.C1Command.C1CommandLink lnkInsertVariable1
  C1.Win.C1Command.C1CommandLink lnkInsertVariable2
  C1.Win.C1Command.C1CommandLink lnkInsertVariable3
  C1.Win.C1Command.C1CommandLink lnkMultiList
  C1.Win.C1Command.C1CommandLink lnkOtherFunc
  C1.Win.C1Command.C1CommandLink lnkPaste1
  C1.Win.C1Command.C1CommandLink lnkPaste2
  C1.Win.C1Command.C1CommandLink lnkPaste3
  C1.Win.C1Command.C1CommandLink lnkTableList
  C1.Win.C1Command.C1CommandLink lnkTreeList
  C1.Win.C1SplitContainer.C1SplitterPanel pnlButtons
  C1.Win.C1SplitContainer.C1SplitterPanel pnlCombo
  C1.Win.C1SplitContainer.C1SplitterPanel pnlFunctionHint
  C1.Win.C1SplitContainer.C1SplitterPanel pnlFunctions
  C1.Win.C1SplitContainer.C1SplitterPanel pnlInput
  C1.Win.C1SplitContainer.C1SplitterPanel pnlInputBox
  System.Windows.Forms.RichTextBoxEx rtbDropInput
  C1.Win.C1Command.C1DockingTabPage tabDefault
  C1.Win.C1Command.C1DockingTabPage tabDropList
  C1.Win.C1Command.C1DockingTabPage tabEdit
  C1.Win.C1Command.C1ToolBar tbrFunctions
  Leqisoft.UI.Controls.C1TextBoxEx txbCommentInput
  Leqisoft.UI.Controls.C1TextBoxEx txbDefaultInput
  C1.Win.C1Command.C1ContextMenu txbDropInputContextMenu

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::.ctor(Leqisoft.UI.Platform.AuxEditor) ---
HasBody=True RVA=0x00027C90 IL=130
IL_0000: ldarg.0        
IL_0001: newobj         System.Void C1.Win.C1Command.C1ContextMenu::.ctor()
IL_0006: stfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx1
IL_000B: ldarg.0        
IL_000C: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_0011: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCopy1
IL_0016: ldarg.0        
IL_0017: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor()
IL_001C: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCopy1
IL_0021: ldarg.0        
IL_0022: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_0027: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCut1
IL_002C: ldarg.0        
IL_002D: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor()
IL_0032: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCut1
IL_0037: ldarg.0        
IL_0038: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_003D: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdPaste1
IL_0042: ldarg.0        
IL_0043: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor()
IL_0048: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkPaste1
IL_004D: ldarg.0        
IL_004E: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_0053: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertVariable1
IL_0058: ldarg.0        
IL_0059: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor()
IL_005E: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInsertVariable1
IL_0063: ldarg.0        
IL_0064: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_0069: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertColumn1
IL_006E: ldarg.0        
IL_006F: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor()
IL_0074: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInsertColumn1
IL_0079: ldarg.0        
IL_007A: newobj         System.Void C1.Win.C1Command.C1ContextMenu::.ctor()
IL_007F: stfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx2
IL_0084: ldarg.0        
IL_0085: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_008A: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCopy2
IL_008F: ldarg.0        
IL_0090: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor()
IL_0095: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCopy2
IL_009A: ldarg.0        
IL_009B: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_00A0: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCut2
IL_00A5: ldarg.0        
IL_00A6: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor()
IL_00AB: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCut2
IL_00B0: ldarg.0        
IL_00B1: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_00B6: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdPaste2
IL_00BB: ldarg.0        
IL_00BC: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor()
IL_00C1: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkPaste2
IL_00C6: ldarg.0        
IL_00C7: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_00CC: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertVariable2
IL_00D1: ldarg.0        
IL_00D2: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor()
IL_00D7: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInsertVariable2
IL_00DC: ldarg.0        
IL_00DD: newobj         System.Void C1.Win.C1Command.C1ContextMenu::.ctor()
IL_00E2: stfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx3
IL_00E7: ldarg.0        
IL_00E8: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_00ED: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCopy3
IL_00F2: ldarg.0        
IL_00F3: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor()
IL_00F8: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCopy3
IL_00FD: ldarg.0        
IL_00FE: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_0103: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCut3
IL_0108: ldarg.0        
IL_0109: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor()
IL_010E: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCut3
IL_0113: ldarg.0        
IL_0114: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_0119: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdPaste3
IL_011E: ldarg.0        
IL_011F: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor()
IL_0124: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkPaste3
IL_0129: ldarg.0        
IL_012A: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_012F: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertVariable3
IL_0134: ldarg.0        
IL_0135: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor()
IL_013A: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInsertVariable3
IL_013F: ldarg.0        
IL_0140: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_0145: dup            
IL_0146: ldstr          "TreeList函数"
IL_014B: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_0150: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdTreeList
IL_0155: ldarg.0        
IL_0156: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_015B: dup            
IL_015C: ldstr          "TableList函数"
IL_0161: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_0166: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdTableList
IL_016B: ldarg.0        
IL_016C: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_0171: dup            
IL_0172: ldstr          "MultiList函数"
IL_0177: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_017C: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdMultiList
IL_0181: ldarg.0        
IL_0182: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_0187: dup            
IL_0188: ldstr          "InputList函数"
IL_018D: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_0192: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInputList
IL_0197: ldarg.0        
IL_0198: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_019D: dup            
IL_019E: ldstr          "其他函数"
IL_01A3: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_01A8: stfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdOtherFunc
IL_01AD: ldarg.0        
IL_01AE: newobj         System.Void C1.Win.C1Command.C1ContextMenu::.ctor()
IL_01B3: stfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctxOtherFunc
IL_01B8: ldarg.0        
IL_01B9: call           System.Void C1.Win.C1Ribbon.C1RibbonForm::.ctor()
IL_01BE: ldarg.0        
IL_01BF: ldarg.1        
IL_01C0: stfld          Leqisoft.UI.Platform.AuxEditor Leqisoft.UI.Platform.frmAuxEdit::_owner
IL_01C5: ldarg.0        
IL_01C6: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::InitializeComponent()
IL_01CB: ldarg.0        
IL_01CC: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::Initialize()
IL_01D1: ret            

--- System.Windows.Forms.TextBoxBase Leqisoft.UI.Platform.frmAuxEdit::ActiveTextBox() ---
HasBody=True RVA=0x00028AE4 IL=21
IL_0000: ldarg.0        
IL_0001: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_0006: callvirt       C1.Win.C1Command.C1DockingTabPage C1.Win.C1Command.C1DockingTab::get_SelectedTab()
IL_000B: ldarg.0        
IL_000C: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDropList
IL_0011: bne.un.s       IL_001a: ldarg.0
IL_0013: ldarg.0        
IL_0014: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0019: ret            
IL_001A: ldarg.0        
IL_001B: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_0020: callvirt       C1.Win.C1Command.C1DockingTabPage C1.Win.C1Command.C1DockingTab::get_SelectedTab()
IL_0025: ldarg.0        
IL_0026: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabEdit
IL_002B: bne.un.s       IL_0034: ldarg.0
IL_002D: ldarg.0        
IL_002E: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_0033: ret            
IL_0034: ldarg.0        
IL_0035: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_003A: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::AttachEvent() ---
HasBody=True RVA=0x00028DF4 IL=34
IL_0000: ldarg.0        
IL_0001: ldfld          System.Boolean Leqisoft.UI.Platform.frmAuxEdit::_attachEvent
IL_0006: brtrue.s       IL_007b: ret
IL_0008: ldarg.0        
IL_0009: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_000E: ldarg.0        
IL_000F: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput_CheckedChanged(System.Object,System.EventArgs)
IL_0015: newobj         System.Void System.EventHandler::.ctor(System.Object,System.IntPtr)
IL_001A: callvirt       System.Void System.Windows.Forms.CheckBox::add_CheckedChanged(System.EventHandler)
IL_001F: ldarg.0        
IL_0020: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_0025: ldarg.0        
IL_0026: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect_CheckedChanged(System.Object,System.EventArgs)
IL_002C: newobj         System.Void System.EventHandler::.ctor(System.Object,System.IntPtr)
IL_0031: callvirt       System.Void System.Windows.Forms.CheckBox::add_CheckedChanged(System.EventHandler)
IL_0036: ldarg.0        
IL_0037: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_003C: ldarg.0        
IL_003D: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::txbDropInput_TextChanged(System.Object,System.EventArgs)
IL_0043: newobj         System.Void System.EventHandler::.ctor(System.Object,System.IntPtr)
IL_0048: callvirt       System.Void System.Windows.Forms.Control::add_TextChanged(System.EventHandler)
IL_004D: ldarg.0        
IL_004E: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_0053: ldarg.0        
IL_0054: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput_TextChanged(System.Object,System.EventArgs)
IL_005A: newobj         System.Void System.EventHandler::.ctor(System.Object,System.IntPtr)
IL_005F: callvirt       System.Void System.Windows.Forms.Control::add_TextChanged(System.EventHandler)
IL_0064: ldarg.0        
IL_0065: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_006A: ldarg.0        
IL_006B: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput_TextChanged(System.Object,System.EventArgs)
IL_0071: newobj         System.Void System.EventHandler::.ctor(System.Object,System.IntPtr)
IL_0076: callvirt       System.Void System.Windows.Forms.Control::add_TextChanged(System.EventHandler)
IL_007B: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::btnCancle_Click(System.Object,System.EventArgs) ---
HasBody=True RVA=0x00022F24 IL=6
IL_0000: ldarg.0        
IL_0001: ldc.i4.2       
IL_0002: call           System.Void System.Windows.Forms.Form::set_DialogResult(System.Windows.Forms.DialogResult)
IL_0007: ldarg.0        
IL_0008: call           System.Void System.Windows.Forms.Form::Close()
IL_000D: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::btnConfirm_Click(System.Object,System.EventArgs) ---
HasBody=True RVA=0x00028010 IL=23
Locals:
  [0] System.String
IL_0000: ldarg.0        
IL_0001: ldloca.s       V_0
IL_0003: call           System.Boolean Leqisoft.UI.Platform.frmAuxEdit::ValidateFormula(System.String&)
IL_0008: brfalse.s      IL_0040: ret
IL_000A: ldarg.0        
IL_000B: ldc.i4.1       
IL_000C: call           System.Void System.Windows.Forms.Form::set_DialogResult(System.Windows.Forms.DialogResult)
IL_0011: ldarg.0        
IL_0012: ldloc.0        
IL_0013: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::set_Value(System.String)
IL_0018: ldarg.0        
IL_0019: ldarg.0        
IL_001A: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_001F: callvirt       System.String System.Windows.Forms.Control::get_Text()
IL_0024: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::set_DefaultValue(System.String)
IL_0029: ldarg.0        
IL_002A: ldarg.0        
IL_002B: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_0030: callvirt       System.String System.Windows.Forms.Control::get_Text()
IL_0035: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::set_CommentValue(System.String)
IL_003A: ldarg.0        
IL_003B: call           System.Void System.Windows.Forms.Form::Close()
IL_0040: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput_CheckedChanged(System.Object,System.EventArgs) ---
HasBody=True RVA=0x00028B20 IL=9
IL_0000: ldarg.0        
IL_0001: ldarg.0        
IL_0002: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_0007: callvirt       System.Boolean System.Windows.Forms.CheckBox::get_Checked()
IL_000C: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::set_AllowFreeInput(System.Boolean)
IL_0011: ldarg.0        
IL_0012: ldc.i4.1       
IL_0013: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::set_QueryValueChanged(System.Boolean)
IL_0018: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect_CheckedChanged(System.Object,System.EventArgs) ---
HasBody=True RVA=0x00028B3A IL=20
IL_0000: ldarg.0        
IL_0001: ldarg.0        
IL_0002: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_0007: callvirt       System.Boolean System.Windows.Forms.CheckBox::get_Checked()
IL_000C: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::set_AllowMultiSelect(System.Boolean)
IL_0011: ldarg.0        
IL_0012: call           System.Boolean Leqisoft.UI.Platform.frmAuxEdit::get_AllowMultiSelect()
IL_0017: brfalse.s      IL_0022: ldarg.0
IL_0019: ldarg.0        
IL_001A: ldc.i4.0       
IL_001B: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::set_AllowFreeInput(System.Boolean)
IL_0020: br.s           IL_002e: ldarg.0
IL_0022: ldarg.0        
IL_0023: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_0028: ldc.i4.1       
IL_0029: callvirt       System.Void System.Windows.Forms.Control::set_Enabled(System.Boolean)
IL_002E: ldarg.0        
IL_002F: ldc.i4.1       
IL_0030: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::set_QueryValueChanged(System.Boolean)
IL_0035: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdCopy_Click(System.Object,C1.Win.C1Command.ClickEventArgs) ---
HasBody=True RVA=0x00028A9C IL=17
Locals:
  [0] System.Windows.Forms.TextBoxBase
  [1] System.String
ExceptionHandlers:
  Catch try=IL_0000..IL_0025 handler=IL_0025..IL_0028
IL_0000: ldarg.0        
IL_0001: call           System.Windows.Forms.TextBoxBase Leqisoft.UI.Platform.frmAuxEdit::ActiveTextBox()
IL_0006: stloc.0        
IL_0007: ldloc.0        
IL_0008: callvirt       System.String System.Windows.Forms.TextBoxBase::get_SelectedText()
IL_000D: stloc.1        
IL_000E: ldloc.1        
IL_000F: call           System.Boolean System.String::IsNullOrEmpty(System.String)
IL_0014: brfalse.s      IL_001d: ldloc.1
IL_0016: call           System.Void System.Windows.Forms.Clipboard::Clear()
IL_001B: br.s           IL_0023: leave.s IL_0028
IL_001D: ldloc.1        
IL_001E: call           System.Void System.Windows.Forms.Clipboard::SetText(System.String)
IL_0023: leave.s        IL_0028: ret
IL_0025: pop            
IL_0026: leave.s        IL_0028: ret
IL_0028: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdCut_Click(System.Object,C1.Win.C1Command.ClickEventArgs) ---
HasBody=True RVA=0x00028A4C IL=20
Locals:
  [0] System.Windows.Forms.TextBoxBase
  [1] System.String
ExceptionHandlers:
  Catch try=IL_0007..IL_0025 handler=IL_0025..IL_0028
IL_0000: ldarg.0        
IL_0001: call           System.Windows.Forms.TextBoxBase Leqisoft.UI.Platform.frmAuxEdit::ActiveTextBox()
IL_0006: stloc.0        
IL_0007: ldloc.0        
IL_0008: callvirt       System.String System.Windows.Forms.TextBoxBase::get_SelectedText()
IL_000D: stloc.1        
IL_000E: ldloc.1        
IL_000F: call           System.Boolean System.String::IsNullOrEmpty(System.String)
IL_0014: brfalse.s      IL_001d: ldloc.1
IL_0016: call           System.Void System.Windows.Forms.Clipboard::Clear()
IL_001B: br.s           IL_0023: leave.s IL_0028
IL_001D: ldloc.1        
IL_001E: call           System.Void System.Windows.Forms.Clipboard::SetText(System.String)
IL_0023: leave.s        IL_0028: ldloc.0
IL_0025: pop            
IL_0026: leave.s        IL_0028: ldloc.0
IL_0028: ldloc.0        
IL_0029: ldsfld         System.String System.String::Empty
IL_002E: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_SelectedText(System.String)
IL_0033: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdInputList_Click(System.Object,C1.Win.C1Command.ClickEventArgs) ---
HasBody=True RVA=0x000287FA IL=17
IL_0000: ldarg.0        
IL_0001: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0006: ldstr          "InputList()"
IL_000B: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_SelectedText(System.String)
IL_0010: ldarg.0        
IL_0011: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0016: ldarg.0        
IL_0017: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_001C: callvirt       System.Int32 System.Windows.Forms.TextBoxBase::get_SelectionStart()
IL_0021: ldarg.0        
IL_0022: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0027: callvirt       System.Int32 System.Windows.Forms.TextBoxBase::get_SelectionLength()
IL_002C: add            
IL_002D: ldc.i4.1       
IL_002E: sub            
IL_002F: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_SelectionStart(System.Int32)
IL_0034: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdInsertColumn1_Click(System.Object,C1.Win.C1Command.ClickEventArgs) ---
HasBody=True RVA=0x00028974 IL=20
Locals:
  [0] Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass102_0
IL_0000: newobj         System.Void Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass102_0::.ctor()
IL_0005: stloc.0        
IL_0006: ldloc.0        
IL_0007: ldarg.0        
IL_0008: stfld          Leqisoft.UI.Platform.frmAuxEdit Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass102_0::<>4__this
IL_000D: ldloc.0        
IL_000E: call           Leqisoft.UI.Controls.MergeForm Leqisoft.UI.Controls.MergeForm::GetInstance()
IL_0013: stfld          Leqisoft.UI.Controls.MergeForm Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass102_0::form
IL_0018: ldloc.0        
IL_0019: ldfld          Leqisoft.UI.Controls.MergeForm Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass102_0::form
IL_001E: call           Leqisoft.UI.Platform.MainForm Leqisoft.UI.Platform.Program::get_MainForm()
IL_0023: callvirt       Leqisoft.Model.Project Leqisoft.UI.Platform.MainForm::get_CurrentProject()
IL_0028: callvirt       System.Void Leqisoft.UI.Controls.MergeForm::Show(Leqisoft.Model.Project)
IL_002D: ldloc.0        
IL_002E: ldfld          Leqisoft.UI.Controls.MergeForm Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass102_0::form
IL_0033: ldloc.0        
IL_0034: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass102_0::<CmdInsertColumn1_Click>b__0(System.Object,Leqisoft.Model.Column)
IL_003A: newobj         System.Void System.EventHandler`1<Leqisoft.Model.Column>::.ctor(System.Object,System.IntPtr)
IL_003F: callvirt       System.Void Leqisoft.UI.Controls.MergeForm::add_AfterSelected(System.EventHandler`1<Leqisoft.Model.Column>)
IL_0044: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdInsertVariable_Click(System.Object,C1.Win.C1Command.ClickEventArgs) ---
HasBody=True RVA=0x000289C8 IL=20
Locals:
  [0] Leqisoft.UI.Platform.ReferenceEditor
  [1] Leqisoft.Model.DataReference
IL_0000: newobj         System.Void Leqisoft.UI.Platform.ReferenceEditor::.ctor()
IL_0005: stloc.0        
IL_0006: ldloc.0        
IL_0007: callvirt       System.Windows.Forms.DialogResult Leqisoft.UI.Platform.ReferenceEditor::ShowSelect()
IL_000C: ldc.i4.1       
IL_000D: bne.un.s       IL_003c: ret
IL_000F: ldloc.0        
IL_0010: callvirt       Leqisoft.Model.DataReference Leqisoft.UI.Platform.ReferenceEditor::get_SelectedReference()
IL_0015: stloc.1        
IL_0016: ldarg.0        
IL_0017: call           System.Windows.Forms.TextBoxBase Leqisoft.UI.Platform.frmAuxEdit::ActiveTextBox()
IL_001C: dup            
IL_001D: callvirt       System.String System.Windows.Forms.TextBoxBase::get_SelectedText()
IL_0022: ldstr          "Var(""
IL_0027: ldloc.1        
IL_0028: callvirt       System.String Leqisoft.Model.DataReference::get_Key()
IL_002D: ldstr          "")"
IL_0032: call           System.String System.String::Concat(System.String,System.String,System.String,System.String)
IL_0037: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_SelectedText(System.String)
IL_003C: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdMultiList_Click(System.Object,C1.Win.C1Command.ClickEventArgs) ---
HasBody=True RVA=0x00028758 IL=17
IL_0000: ldarg.0        
IL_0001: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0006: ldstr          "MultiList()"
IL_000B: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_SelectedText(System.String)
IL_0010: ldarg.0        
IL_0011: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0016: ldarg.0        
IL_0017: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_001C: callvirt       System.Int32 System.Windows.Forms.TextBoxBase::get_SelectionStart()
IL_0021: ldarg.0        
IL_0022: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0027: callvirt       System.Int32 System.Windows.Forms.TextBoxBase::get_SelectionLength()
IL_002C: add            
IL_002D: ldc.i4.1       
IL_002E: sub            
IL_002F: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_SelectionStart(System.Int32)
IL_0034: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdOtherFunc_Click(System.Object,C1.Win.C1Command.ClickEventArgs) ---
HasBody=True RVA=0x00028735 IL=10
IL_0000: ldarg.0        
IL_0001: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctxOtherFunc
IL_0006: ldarg.0        
IL_0007: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_000C: ldarg.0        
IL_000D: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_0012: call           System.Drawing.Point System.Windows.Forms.Cursor::get_Position()
IL_0017: callvirt       System.Drawing.Point System.Windows.Forms.Control::PointToClient(System.Drawing.Point)
IL_001C: callvirt       System.Void C1.Win.C1Command.C1ContextMenu::ShowContextMenu(System.Windows.Forms.Control,System.Drawing.Point)
IL_0021: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdPaste_Click(System.Object,C1.Win.C1Command.ClickEventArgs) ---
HasBody=True RVA=0x00028A14 IL=12
Locals:
  [0] System.Windows.Forms.TextBoxBase
  [1] System.String
ExceptionHandlers:
  Catch try=IL_0000..IL_0016 handler=IL_0016..IL_0019
IL_0000: ldarg.0        
IL_0001: call           System.Windows.Forms.TextBoxBase Leqisoft.UI.Platform.frmAuxEdit::ActiveTextBox()
IL_0006: stloc.0        
IL_0007: call           System.String System.Windows.Forms.Clipboard::GetText()
IL_000C: stloc.1        
IL_000D: ldloc.0        
IL_000E: ldloc.1        
IL_000F: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_SelectedText(System.String)
IL_0014: leave.s        IL_0019: ret
IL_0016: pop            
IL_0017: leave.s        IL_0019: ret
IL_0019: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdTableList_Click(System.Object,C1.Win.C1Command.ClickEventArgs) ---
HasBody=True RVA=0x0002878E IL=17
IL_0000: ldarg.0        
IL_0001: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0006: ldstr          "TableList()"
IL_000B: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_SelectedText(System.String)
IL_0010: ldarg.0        
IL_0011: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0016: ldarg.0        
IL_0017: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_001C: callvirt       System.Int32 System.Windows.Forms.TextBoxBase::get_SelectionStart()
IL_0021: ldarg.0        
IL_0022: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0027: callvirt       System.Int32 System.Windows.Forms.TextBoxBase::get_SelectionLength()
IL_002C: add            
IL_002D: ldc.i4.1       
IL_002E: sub            
IL_002F: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_SelectionStart(System.Int32)
IL_0034: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdTreeList_Click(System.Object,C1.Win.C1Command.ClickEventArgs) ---
HasBody=True RVA=0x000287C4 IL=17
IL_0000: ldarg.0        
IL_0001: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0006: ldstr          "TreeList()"
IL_000B: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_SelectedText(System.String)
IL_0010: ldarg.0        
IL_0011: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0016: ldarg.0        
IL_0017: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_001C: callvirt       System.Int32 System.Windows.Forms.TextBoxBase::get_SelectionStart()
IL_0021: ldarg.0        
IL_0022: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0027: callvirt       System.Int32 System.Windows.Forms.TextBoxBase::get_SelectionLength()
IL_002C: add            
IL_002D: ldc.i4.1       
IL_002E: sub            
IL_002F: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_SelectionStart(System.Int32)
IL_0034: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::Dispose(System.Boolean) ---
HasBody=True RVA=0x000290E0 IL=12
IL_0000: ldarg.1        
IL_0001: brfalse.s      IL_0016: ldarg.0
IL_0003: ldarg.0        
IL_0004: ldfld          System.ComponentModel.IContainer Leqisoft.UI.Platform.frmAuxEdit::components
IL_0009: brfalse.s      IL_0016: ldarg.0
IL_000B: ldarg.0        
IL_000C: ldfld          System.ComponentModel.IContainer Leqisoft.UI.Platform.frmAuxEdit::components
IL_0011: callvirt       System.Void System.IDisposable::Dispose()
IL_0016: ldarg.0        
IL_0017: ldarg.1        
IL_0018: call           System.Void C1.Win.C1Ribbon.C1RibbonForm::Dispose(System.Boolean)
IL_001D: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::FrmAuxEdit_FormClosing(System.Object,System.Windows.Forms.FormClosingEventArgs) ---
HasBody=True RVA=0x0002871C IL=8
IL_0000: ldarg.0        
IL_0001: ldfld          Leqisoft.UI.Platform.AuxEditor Leqisoft.UI.Platform.frmAuxEdit::_owner
IL_0006: ldc.i4.0       
IL_0007: callvirt       System.Void Leqisoft.UI.Platform.AuxEditor::set_IsEditing(System.Boolean)
IL_000C: ldarg.0        
IL_000D: ldfld          Leqisoft.UI.Platform.AuxEditor Leqisoft.UI.Platform.frmAuxEdit::_owner
IL_0012: callvirt       System.Void Leqisoft.UI.Platform.AuxEditor::OnClosed()
IL_0017: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::FrmAuxEdit_Shown(System.Object,System.EventArgs) ---
HasBody=True RVA=0x000286B0 IL=9
IL_0000: ldarg.0        
IL_0001: call           System.Void Leqisoft.UI.Controls.Theme::SetCurrentTree(System.Windows.Forms.Control)
IL_0006: ldarg.0        
IL_0007: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::ResetQueryStatus()
IL_000C: ldarg.0        
IL_000D: ldfld          Leqisoft.UI.Platform.AuxEditor Leqisoft.UI.Platform.frmAuxEdit::_owner
IL_0012: ldc.i4.1       
IL_0013: callvirt       System.Void Leqisoft.UI.Platform.AuxEditor::set_IsEditing(System.Boolean)
IL_0018: ret            

--- System.Boolean Leqisoft.UI.Platform.frmAuxEdit::get_AllowFreeInput() ---
HasBody=True RVA=0x00027C1B IL=3
IL_0000: ldarg.0        
IL_0001: ldfld          System.Boolean Leqisoft.UI.Platform.frmAuxEdit::_allowFreeInput
IL_0006: ret            

--- System.Boolean Leqisoft.UI.Platform.frmAuxEdit::get_AllowMultiSelect() ---
HasBody=True RVA=0x00027C53 IL=3
IL_0000: ldarg.0        
IL_0001: ldfld          System.Boolean Leqisoft.UI.Platform.frmAuxEdit::_allowMultiSelect
IL_0006: ret            

--- System.String Leqisoft.UI.Platform.frmAuxEdit::get_CommentNotice() ---
HasBody=True RVA=0x00027B98 IL=3
IL_0000: ldarg.0        
IL_0001: ldfld          System.String Leqisoft.UI.Platform.frmAuxEdit::_commentNotice
IL_0006: ret            

--- System.String Leqisoft.UI.Platform.frmAuxEdit::get_CommentValue() ---
HasBody=True RVA=0x00027BFE IL=3
IL_0000: ldarg.0        
IL_0001: ldfld          System.String Leqisoft.UI.Platform.frmAuxEdit::_commentValue
IL_0006: ret            

--- System.String Leqisoft.UI.Platform.frmAuxEdit::get_DefaultNotice() ---
HasBody=True RVA=0x00027B87 IL=3
IL_0000: ldarg.0        
IL_0001: ldfld          System.String Leqisoft.UI.Platform.frmAuxEdit::_defaultNotice
IL_0006: ret            

--- System.String Leqisoft.UI.Platform.frmAuxEdit::get_DefaultValue() ---
HasBody=True RVA=0x00027BDC IL=3
IL_0000: ldarg.0        
IL_0001: ldfld          System.String Leqisoft.UI.Platform.frmAuxEdit::_defaultValue
IL_0006: ret            

--- System.String Leqisoft.UI.Platform.frmAuxEdit::get_Notice() ---
HasBody=True RVA=0x00027B76 IL=3
IL_0000: ldarg.0        
IL_0001: ldfld          System.String Leqisoft.UI.Platform.frmAuxEdit::_dropNotice
IL_0006: ret            

--- System.Boolean Leqisoft.UI.Platform.frmAuxEdit::get_QueryCommentValueChanged() ---
HasBody=True RVA=0x00027C7D IL=3
IL_0000: ldarg.0        
IL_0001: ldfld          System.Boolean Leqisoft.UI.Platform.frmAuxEdit::<QueryCommentValueChanged>k__BackingField
IL_0006: ret            

--- System.Boolean Leqisoft.UI.Platform.frmAuxEdit::get_QueryDefaultValueChanged() ---
HasBody=True RVA=0x00027C6C IL=3
IL_0000: ldarg.0        
IL_0001: ldfld          System.Boolean Leqisoft.UI.Platform.frmAuxEdit::<QueryDefaultValueChanged>k__BackingField
IL_0006: ret            

--- System.Boolean Leqisoft.UI.Platform.frmAuxEdit::get_QueryValueChanged() ---
HasBody=True RVA=0x00027C5B IL=3
IL_0000: ldarg.0        
IL_0001: ldfld          System.Boolean Leqisoft.UI.Platform.frmAuxEdit::<QueryValueChanged>k__BackingField
IL_0006: ret            

--- System.String Leqisoft.UI.Platform.frmAuxEdit::get_Value() ---
HasBody=True RVA=0x00027BBA IL=3
IL_0000: ldarg.0        
IL_0001: ldfld          System.String Leqisoft.UI.Platform.frmAuxEdit::_dropValue
IL_0006: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::Initialize() ---
HasBody=True RVA=0x00028074 IL=450
IL_0000: ldarg.0        
IL_0001: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_0006: ldc.i4.0       
IL_0007: callvirt       System.Void C1.Win.C1Command.C1DockingTab::set_ShowTabs(System.Boolean)
IL_000C: ldarg.0        
IL_000D: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0012: ldarg.0        
IL_0013: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::TxbDropInput_SelectionChanged(System.Object,System.EventArgs)
IL_0019: newobj         System.Void System.EventHandler::.ctor(System.Object,System.IntPtr)
IL_001E: callvirt       System.Void System.Windows.Forms.RichTextBox::add_SelectionChanged(System.EventHandler)
IL_0023: ldarg.0        
IL_0024: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCopy1
IL_0029: ldstr          "复制"
IL_002E: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_0033: ldarg.0        
IL_0034: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCopy1
IL_0039: ldarg.0        
IL_003A: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdCopy_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_0040: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_0045: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_004A: ldarg.0        
IL_004B: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCopy1
IL_0050: ldarg.0        
IL_0051: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCopy1
IL_0056: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_Command(C1.Win.C1Command.C1Command)
IL_005B: ldarg.0        
IL_005C: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCut1
IL_0061: ldstr          "剪切"
IL_0066: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_006B: ldarg.0        
IL_006C: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCut1
IL_0071: ldarg.0        
IL_0072: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdCut_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_0078: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_007D: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_0082: ldarg.0        
IL_0083: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCut1
IL_0088: ldarg.0        
IL_0089: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCut1
IL_008E: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_Command(C1.Win.C1Command.C1Command)
IL_0093: ldarg.0        
IL_0094: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdPaste1
IL_0099: ldstr          "粘贴"
IL_009E: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_00A3: ldarg.0        
IL_00A4: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdPaste1
IL_00A9: ldarg.0        
IL_00AA: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdPaste_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_00B0: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_00B5: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_00BA: ldarg.0        
IL_00BB: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkPaste1
IL_00C0: ldarg.0        
IL_00C1: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdPaste1
IL_00C6: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_Command(C1.Win.C1Command.C1Command)
IL_00CB: ldarg.0        
IL_00CC: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertVariable1
IL_00D1: ldstr          "插入变量"
IL_00D6: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_00DB: ldarg.0        
IL_00DC: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertVariable1
IL_00E1: ldarg.0        
IL_00E2: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdInsertVariable_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_00E8: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_00ED: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_00F2: ldarg.0        
IL_00F3: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInsertVariable1
IL_00F8: ldarg.0        
IL_00F9: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertVariable1
IL_00FE: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_Command(C1.Win.C1Command.C1Command)
IL_0103: ldarg.0        
IL_0104: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertColumn1
IL_0109: ldstr          "插入列来源"
IL_010E: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_0113: ldarg.0        
IL_0114: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertColumn1
IL_0119: ldarg.0        
IL_011A: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdInsertColumn1_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_0120: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_0125: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_012A: ldarg.0        
IL_012B: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInsertColumn1
IL_0130: ldarg.0        
IL_0131: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertColumn1
IL_0136: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_Command(C1.Win.C1Command.C1Command)
IL_013B: ldarg.0        
IL_013C: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx1
IL_0141: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_0146: ldarg.0        
IL_0147: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCopy1
IL_014C: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_0151: pop            
IL_0152: ldarg.0        
IL_0153: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx1
IL_0158: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_015D: ldarg.0        
IL_015E: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCut1
IL_0163: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_0168: pop            
IL_0169: ldarg.0        
IL_016A: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx1
IL_016F: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_0174: ldarg.0        
IL_0175: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkPaste1
IL_017A: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_017F: pop            
IL_0180: ldarg.0        
IL_0181: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx1
IL_0186: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_018B: ldarg.0        
IL_018C: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInsertVariable1
IL_0191: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_0196: pop            
IL_0197: ldarg.0        
IL_0198: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx1
IL_019D: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_01A2: ldarg.0        
IL_01A3: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInsertColumn1
IL_01A8: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_01AD: pop            
IL_01AE: ldarg.0        
IL_01AF: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCopy2
IL_01B4: ldstr          "复制"
IL_01B9: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_01BE: ldarg.0        
IL_01BF: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCopy2
IL_01C4: ldarg.0        
IL_01C5: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdCopy_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_01CB: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_01D0: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_01D5: ldarg.0        
IL_01D6: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCopy2
IL_01DB: ldarg.0        
IL_01DC: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCopy2
IL_01E1: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_Command(C1.Win.C1Command.C1Command)
IL_01E6: ldarg.0        
IL_01E7: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCut2
IL_01EC: ldstr          "剪切"
IL_01F1: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_01F6: ldarg.0        
IL_01F7: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCut2
IL_01FC: ldarg.0        
IL_01FD: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdCut_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_0203: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_0208: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_020D: ldarg.0        
IL_020E: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCut2
IL_0213: ldarg.0        
IL_0214: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCut2
IL_0219: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_Command(C1.Win.C1Command.C1Command)
IL_021E: ldarg.0        
IL_021F: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdPaste2
IL_0224: ldstr          "粘贴"
IL_0229: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_022E: ldarg.0        
IL_022F: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdPaste2
IL_0234: ldarg.0        
IL_0235: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdPaste_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_023B: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_0240: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_0245: ldarg.0        
IL_0246: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkPaste2
IL_024B: ldarg.0        
IL_024C: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdPaste2
IL_0251: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_Command(C1.Win.C1Command.C1Command)
IL_0256: ldarg.0        
IL_0257: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertVariable2
IL_025C: ldstr          "插入变量"
IL_0261: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_0266: ldarg.0        
IL_0267: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertVariable2
IL_026C: ldarg.0        
IL_026D: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdInsertVariable_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_0273: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_0278: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_027D: ldarg.0        
IL_027E: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInsertVariable2
IL_0283: ldarg.0        
IL_0284: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertVariable2
IL_0289: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_Command(C1.Win.C1Command.C1Command)
IL_028E: ldarg.0        
IL_028F: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx2
IL_0294: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_0299: ldarg.0        
IL_029A: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCopy2
IL_029F: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_02A4: pop            
IL_02A5: ldarg.0        
IL_02A6: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx2
IL_02AB: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_02B0: ldarg.0        
IL_02B1: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCut2
IL_02B6: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_02BB: pop            
IL_02BC: ldarg.0        
IL_02BD: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx2
IL_02C2: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_02C7: ldarg.0        
IL_02C8: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkPaste2
IL_02CD: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_02D2: pop            
IL_02D3: ldarg.0        
IL_02D4: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx2
IL_02D9: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_02DE: ldarg.0        
IL_02DF: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInsertVariable2
IL_02E4: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_02E9: pop            
IL_02EA: ldarg.0        
IL_02EB: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCopy3
IL_02F0: ldstr          "复制"
IL_02F5: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_02FA: ldarg.0        
IL_02FB: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCopy3
IL_0300: ldarg.0        
IL_0301: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdCopy_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_0307: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_030C: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_0311: ldarg.0        
IL_0312: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCopy3
IL_0317: ldarg.0        
IL_0318: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCopy3
IL_031D: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_Command(C1.Win.C1Command.C1Command)
IL_0322: ldarg.0        
IL_0323: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCut3
IL_0328: ldstr          "剪切"
IL_032D: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_0332: ldarg.0        
IL_0333: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCut3
IL_0338: ldarg.0        
IL_0339: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdCut_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_033F: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_0344: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_0349: ldarg.0        
IL_034A: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCut3
IL_034F: ldarg.0        
IL_0350: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdCut3
IL_0355: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_Command(C1.Win.C1Command.C1Command)
IL_035A: ldarg.0        
IL_035B: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdPaste3
IL_0360: ldstr          "粘贴"
IL_0365: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_036A: ldarg.0        
IL_036B: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdPaste3
IL_0370: ldarg.0        
IL_0371: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdPaste_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_0377: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_037C: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_0381: ldarg.0        
IL_0382: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkPaste3
IL_0387: ldarg.0        
IL_0388: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdPaste3
IL_038D: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_Command(C1.Win.C1Command.C1Command)
IL_0392: ldarg.0        
IL_0393: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertVariable3
IL_0398: ldstr          "插入变量"
IL_039D: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_03A2: ldarg.0        
IL_03A3: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertVariable3
IL_03A8: ldarg.0        
IL_03A9: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdInsertVariable_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_03AF: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_03B4: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_03B9: ldarg.0        
IL_03BA: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInsertVariable3
IL_03BF: ldarg.0        
IL_03C0: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInsertVariable3
IL_03C5: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_Command(C1.Win.C1Command.C1Command)
IL_03CA: ldarg.0        
IL_03CB: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx3
IL_03D0: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_03D5: ldarg.0        
IL_03D6: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCopy3
IL_03DB: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_03E0: pop            
IL_03E1: ldarg.0        
IL_03E2: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx3
IL_03E7: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_03EC: ldarg.0        
IL_03ED: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkCut3
IL_03F2: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_03F7: pop            
IL_03F8: ldarg.0        
IL_03F9: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx3
IL_03FE: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_0403: ldarg.0        
IL_0404: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkPaste3
IL_0409: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_040E: pop            
IL_040F: ldarg.0        
IL_0410: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx3
IL_0415: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_041A: ldarg.0        
IL_041B: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInsertVariable3
IL_0420: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_0425: pop            
IL_0426: ldarg.0        
IL_0427: ldfld          C1.Win.C1Command.C1CommandHolder Leqisoft.UI.Platform.frmAuxEdit::c1CommandHolder1
IL_042C: ldarg.0        
IL_042D: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0432: ldarg.0        
IL_0433: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx1
IL_0438: callvirt       System.Void C1.Win.C1Command.C1CommandHolder::SetC1ContextMenu(System.Object,C1.Win.C1Command.C1ContextMenu)
IL_043D: ldarg.0        
IL_043E: ldfld          C1.Win.C1Command.C1CommandHolder Leqisoft.UI.Platform.frmAuxEdit::c1CommandHolder1
IL_0443: ldarg.0        
IL_0444: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_0449: ldarg.0        
IL_044A: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx2
IL_044F: callvirt       System.Void C1.Win.C1Command.C1CommandHolder::SetC1ContextMenu(System.Object,C1.Win.C1Command.C1ContextMenu)
IL_0454: ldarg.0        
IL_0455: ldfld          C1.Win.C1Command.C1CommandHolder Leqisoft.UI.Platform.frmAuxEdit::c1CommandHolder1
IL_045A: ldarg.0        
IL_045B: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_0460: ldarg.0        
IL_0461: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctx3
IL_0466: callvirt       System.Void C1.Win.C1Command.C1CommandHolder::SetC1ContextMenu(System.Object,C1.Win.C1Command.C1ContextMenu)
IL_046B: ldarg.0        
IL_046C: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::AttachEvent()
IL_0471: ldarg.0        
IL_0472: ldarg.0        
IL_0473: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::FrmAuxEdit_Shown(System.Object,System.EventArgs)
IL_0479: newobj         System.Void System.EventHandler::.ctor(System.Object,System.IntPtr)
IL_047E: call           System.Void System.Windows.Forms.Form::add_Shown(System.EventHandler)
IL_0483: ldarg.0        
IL_0484: ldarg.0        
IL_0485: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::FrmAuxEdit_FormClosing(System.Object,System.Windows.Forms.FormClosingEventArgs)
IL_048B: newobj         System.Void System.Windows.Forms.FormClosingEventHandler::.ctor(System.Object,System.IntPtr)
IL_0490: call           System.Void System.Windows.Forms.Form::add_FormClosing(System.Windows.Forms.FormClosingEventHandler)
IL_0495: ldarg.0        
IL_0496: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdTreeList
IL_049B: ldarg.0        
IL_049C: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdTreeList_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_04A2: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_04A7: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_04AC: ldarg.0        
IL_04AD: ldarg.0        
IL_04AE: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdTreeList
IL_04B3: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor(C1.Win.C1Command.C1Command)
IL_04B8: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkTreeList
IL_04BD: ldarg.0        
IL_04BE: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkTreeList
IL_04C3: ldc.i4.1       
IL_04C4: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_ButtonLook(C1.Win.C1Command.ButtonLookFlags)
IL_04C9: ldarg.0        
IL_04CA: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_04CF: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1ToolBar::get_CommandLinks()
IL_04D4: ldarg.0        
IL_04D5: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkTreeList
IL_04DA: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_04DF: pop            
IL_04E0: ldarg.0        
IL_04E1: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdTableList
IL_04E6: ldarg.0        
IL_04E7: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdTableList_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_04ED: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_04F2: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_04F7: ldarg.0        
IL_04F8: ldarg.0        
IL_04F9: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdTableList
IL_04FE: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor(C1.Win.C1Command.C1Command)
IL_0503: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkTableList
IL_0508: ldarg.0        
IL_0509: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkTableList
IL_050E: ldc.i4.1       
IL_050F: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_ButtonLook(C1.Win.C1Command.ButtonLookFlags)
IL_0514: ldarg.0        
IL_0515: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_051A: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1ToolBar::get_CommandLinks()
IL_051F: ldarg.0        
IL_0520: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkTableList
IL_0525: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_052A: pop            
IL_052B: ldarg.0        
IL_052C: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdMultiList
IL_0531: ldarg.0        
IL_0532: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdMultiList_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_0538: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_053D: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_0542: ldarg.0        
IL_0543: ldarg.0        
IL_0544: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdMultiList
IL_0549: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor(C1.Win.C1Command.C1Command)
IL_054E: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkMultiList
IL_0553: ldarg.0        
IL_0554: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkMultiList
IL_0559: ldc.i4.1       
IL_055A: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_ButtonLook(C1.Win.C1Command.ButtonLookFlags)
IL_055F: ldarg.0        
IL_0560: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_0565: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1ToolBar::get_CommandLinks()
IL_056A: ldarg.0        
IL_056B: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkMultiList
IL_0570: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_0575: pop            
IL_0576: ldarg.0        
IL_0577: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInputList
IL_057C: ldarg.0        
IL_057D: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdInputList_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_0583: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_0588: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_058D: ldarg.0        
IL_058E: ldarg.0        
IL_058F: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdInputList
IL_0594: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor(C1.Win.C1Command.C1Command)
IL_0599: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInputList
IL_059E: ldarg.0        
IL_059F: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInputList
IL_05A4: ldc.i4.1       
IL_05A5: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_ButtonLook(C1.Win.C1Command.ButtonLookFlags)
IL_05AA: ldarg.0        
IL_05AB: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_05B0: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1ToolBar::get_CommandLinks()
IL_05B5: ldarg.0        
IL_05B6: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkInputList
IL_05BB: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_05C0: pop            
IL_05C1: ldarg.0        
IL_05C2: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdOtherFunc
IL_05C7: ldarg.0        
IL_05C8: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::CmdOtherFunc_Click(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_05CE: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_05D3: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_05D8: ldarg.0        
IL_05D9: ldarg.0        
IL_05DA: ldfld          C1.Win.C1Command.C1Command Leqisoft.UI.Platform.frmAuxEdit::cmdOtherFunc
IL_05DF: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor(C1.Win.C1Command.C1Command)
IL_05E4: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkOtherFunc
IL_05E9: ldarg.0        
IL_05EA: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkOtherFunc
IL_05EF: ldc.i4.1       
IL_05F0: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_ButtonLook(C1.Win.C1Command.ButtonLookFlags)
IL_05F5: ldarg.0        
IL_05F6: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_05FB: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1ToolBar::get_CommandLinks()
IL_0600: ldarg.0        
IL_0601: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::lnkOtherFunc
IL_0606: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_060B: pop            
IL_060C: ldarg.0        
IL_060D: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::PopulateOtherFunctions()
IL_0612: ldarg.0        
IL_0613: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0618: ldarg.0        
IL_0619: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::TxbDropInput_KeyDown(System.Object,System.Windows.Forms.KeyEventArgs)
IL_061F: newobj         System.Void System.Windows.Forms.KeyEventHandler::.ctor(System.Object,System.IntPtr)
IL_0624: callvirt       System.Void System.Windows.Forms.Control::add_KeyDown(System.Windows.Forms.KeyEventHandler)
IL_0629: ldarg.0        
IL_062A: call           System.Void Leqisoft.UI.Controls.Theme::SetCurrentTree(System.Windows.Forms.Control)
IL_062F: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::InitializeComponent() ---
HasBody=True RVA=0x00029100 IL=1560
IL_0000: ldarg.0        
IL_0001: newobj         System.Void System.Windows.Forms.RichTextBoxEx::.ctor()
IL_0006: stfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_000B: ldarg.0        
IL_000C: newobj         System.Void C1.Win.C1SplitContainer.C1SplitContainer::.ctor()
IL_0011: stfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_0016: ldarg.0        
IL_0017: newobj         System.Void C1.Win.C1SplitContainer.C1SplitterPanel::.ctor()
IL_001C: stfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_0021: ldarg.0        
IL_0022: newobj         System.Void C1.Win.C1Input.C1Button::.ctor()
IL_0027: stfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnCancle
IL_002C: ldarg.0        
IL_002D: newobj         System.Void C1.Win.C1Input.C1Button::.ctor()
IL_0032: stfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnConfirm
IL_0037: ldarg.0        
IL_0038: newobj         System.Void C1.Win.C1SplitContainer.C1SplitterPanel::.ctor()
IL_003D: stfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInputBox
IL_0042: ldarg.0        
IL_0043: newobj         System.Void C1.Win.C1Command.C1DockingTab::.ctor()
IL_0048: stfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_004D: ldarg.0        
IL_004E: newobj         System.Void C1.Win.C1Command.C1DockingTabPage::.ctor()
IL_0053: stfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDropList
IL_0058: ldarg.0        
IL_0059: newobj         System.Void C1.Win.C1SplitContainer.C1SplitContainer::.ctor()
IL_005E: stfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_0063: ldarg.0        
IL_0064: newobj         System.Void C1.Win.C1SplitContainer.C1SplitterPanel::.ctor()
IL_0069: stfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_006E: ldarg.0        
IL_006F: newobj         System.Void C1.Win.C1Input.C1CheckBox::.ctor()
IL_0074: stfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_0079: ldarg.0        
IL_007A: newobj         System.Void C1.Win.C1Input.C1CheckBox::.ctor()
IL_007F: stfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_0084: ldarg.0        
IL_0085: newobj         System.Void C1.Win.C1SplitContainer.C1SplitterPanel::.ctor()
IL_008A: stfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctions
IL_008F: ldarg.0        
IL_0090: newobj         System.Void C1.Win.C1Command.C1ToolBar::.ctor()
IL_0095: stfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_009A: ldarg.0        
IL_009B: newobj         System.Void C1.Win.C1Command.C1CommandHolder::.ctor()
IL_00A0: stfld          C1.Win.C1Command.C1CommandHolder Leqisoft.UI.Platform.frmAuxEdit::c1CommandHolder1
IL_00A5: ldarg.0        
IL_00A6: newobj         System.Void C1.Win.C1Command.C1ContextMenu::.ctor()
IL_00AB: stfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::c1ContextMenu1
IL_00B0: ldarg.0        
IL_00B1: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor()
IL_00B6: stfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::c1CommandLink1
IL_00BB: ldarg.0        
IL_00BC: newobj         System.Void C1.Win.C1Command.C1ContextMenu::.ctor()
IL_00C1: stfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::txbDropInputContextMenu
IL_00C6: ldarg.0        
IL_00C7: newobj         System.Void C1.Win.C1SplitContainer.C1SplitterPanel::.ctor()
IL_00CC: stfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctionHint
IL_00D1: ldarg.0        
IL_00D2: newobj         System.Void C1.Win.C1Input.C1Label::.ctor()
IL_00D7: stfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_00DC: ldarg.0        
IL_00DD: newobj         System.Void C1.Win.C1SplitContainer.C1SplitterPanel::.ctor()
IL_00E2: stfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlCombo
IL_00E7: ldarg.0        
IL_00E8: newobj         System.Void C1.Win.C1Command.C1DockingTabPage::.ctor()
IL_00ED: stfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabEdit
IL_00F2: ldarg.0        
IL_00F3: newobj         System.Void C1.Win.C1SplitContainer.C1SplitContainer::.ctor()
IL_00F8: stfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_00FD: ldarg.0        
IL_00FE: newobj         System.Void C1.Win.C1SplitContainer.C1SplitterPanel::.ctor()
IL_0103: stfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel4
IL_0108: ldarg.0        
IL_0109: newobj         System.Void Leqisoft.UI.Controls.C1TextBoxEx::.ctor()
IL_010E: stfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_0113: ldarg.0        
IL_0114: newobj         System.Void C1.Win.C1Command.C1DockingTabPage::.ctor()
IL_0119: stfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDefault
IL_011E: ldarg.0        
IL_011F: newobj         System.Void C1.Win.C1SplitContainer.C1SplitContainer::.ctor()
IL_0124: stfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_0129: ldarg.0        
IL_012A: newobj         System.Void C1.Win.C1SplitContainer.C1SplitterPanel::.ctor()
IL_012F: stfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel2
IL_0134: ldarg.0        
IL_0135: newobj         System.Void Leqisoft.UI.Controls.C1TextBoxEx::.ctor()
IL_013A: stfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_013F: ldarg.0        
IL_0140: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_0145: callvirt       System.Void System.ComponentModel.ISupportInitialize::BeginInit()
IL_014A: ldarg.0        
IL_014B: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_0150: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_0155: ldarg.0        
IL_0156: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_015B: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_0160: ldarg.0        
IL_0161: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnCancle
IL_0166: callvirt       System.Void System.ComponentModel.ISupportInitialize::BeginInit()
IL_016B: ldarg.0        
IL_016C: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnConfirm
IL_0171: callvirt       System.Void System.ComponentModel.ISupportInitialize::BeginInit()
IL_0176: ldarg.0        
IL_0177: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInputBox
IL_017C: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_0181: ldarg.0        
IL_0182: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_0187: callvirt       System.Void System.ComponentModel.ISupportInitialize::BeginInit()
IL_018C: ldarg.0        
IL_018D: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_0192: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_0197: ldarg.0        
IL_0198: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDropList
IL_019D: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_01A2: ldarg.0        
IL_01A3: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_01A8: callvirt       System.Void System.ComponentModel.ISupportInitialize::BeginInit()
IL_01AD: ldarg.0        
IL_01AE: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_01B3: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_01B8: ldarg.0        
IL_01B9: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_01BE: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_01C3: ldarg.0        
IL_01C4: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_01C9: callvirt       System.Void System.ComponentModel.ISupportInitialize::BeginInit()
IL_01CE: ldarg.0        
IL_01CF: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_01D4: callvirt       System.Void System.ComponentModel.ISupportInitialize::BeginInit()
IL_01D9: ldarg.0        
IL_01DA: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctions
IL_01DF: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_01E4: ldarg.0        
IL_01E5: ldfld          C1.Win.C1Command.C1CommandHolder Leqisoft.UI.Platform.frmAuxEdit::c1CommandHolder1
IL_01EA: callvirt       System.Void System.ComponentModel.ISupportInitialize::BeginInit()
IL_01EF: ldarg.0        
IL_01F0: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctionHint
IL_01F5: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_01FA: ldarg.0        
IL_01FB: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_0200: callvirt       System.Void System.ComponentModel.ISupportInitialize::BeginInit()
IL_0205: ldarg.0        
IL_0206: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlCombo
IL_020B: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_0210: ldarg.0        
IL_0211: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabEdit
IL_0216: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_021B: ldarg.0        
IL_021C: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_0221: callvirt       System.Void System.ComponentModel.ISupportInitialize::BeginInit()
IL_0226: ldarg.0        
IL_0227: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_022C: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_0231: ldarg.0        
IL_0232: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel4
IL_0237: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_023C: ldarg.0        
IL_023D: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_0242: callvirt       System.Void System.ComponentModel.ISupportInitialize::BeginInit()
IL_0247: ldarg.0        
IL_0248: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDefault
IL_024D: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_0252: ldarg.0        
IL_0253: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_0258: callvirt       System.Void System.ComponentModel.ISupportInitialize::BeginInit()
IL_025D: ldarg.0        
IL_025E: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_0263: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_0268: ldarg.0        
IL_0269: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel2
IL_026E: callvirt       System.Void System.Windows.Forms.Control::SuspendLayout()
IL_0273: ldarg.0        
IL_0274: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_0279: callvirt       System.Void System.ComponentModel.ISupportInitialize::BeginInit()
IL_027E: ldarg.0        
IL_027F: call           System.Void System.Windows.Forms.Control::SuspendLayout()
IL_0284: ldarg.0        
IL_0285: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_028A: ldc.i4.1       
IL_028B: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_BorderStyle(System.Windows.Forms.BorderStyle)
IL_0290: ldarg.0        
IL_0291: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0296: ldc.i4.5       
IL_0297: callvirt       System.Void System.Windows.Forms.Control::set_Dock(System.Windows.Forms.DockStyle)
IL_029C: ldarg.0        
IL_029D: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_02A2: ldc.i4.0       
IL_02A3: ldc.i4.0       
IL_02A4: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_02A9: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_02AE: ldarg.0        
IL_02AF: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_02B4: ldc.i4.3       
IL_02B5: ldc.i4.4       
IL_02B6: ldc.i4.3       
IL_02B7: ldc.i4.4       
IL_02B8: newobj         System.Void System.Windows.Forms.Padding::.ctor(System.Int32,System.Int32,System.Int32,System.Int32)
IL_02BD: callvirt       System.Void System.Windows.Forms.Control::set_Margin(System.Windows.Forms.Padding)
IL_02C2: ldarg.0        
IL_02C3: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_02C8: ldstr          "rtbDropInput"
IL_02CD: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_02D2: ldarg.0        
IL_02D3: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_02D8: ldc.i4         792
IL_02DD: ldc.i4         354
IL_02E2: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_02E7: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_02EC: ldarg.0        
IL_02ED: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_02F2: ldc.i4.0       
IL_02F3: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_02F8: ldarg.0        
IL_02F9: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_02FE: ldstr          ""
IL_0303: callvirt       System.Void System.Windows.Forms.Control::set_Text(System.String)
IL_0308: ldarg.0        
IL_0309: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_030E: ldc.i4.0       
IL_030F: callvirt       System.Void System.Windows.Forms.RichTextBoxEx::set_VScrollPos(System.Int32)
IL_0314: ldarg.0        
IL_0315: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_031A: ldc.i4.3       
IL_031B: callvirt       System.Void C1.Framework.XView::set_AutoSizeElement(C1.Framework.AutoSizeElement)
IL_0320: ldarg.0        
IL_0321: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_0326: ldc.i4         164
IL_032B: ldc.i4         195
IL_0330: ldc.i4         235
IL_0335: call           System.Drawing.Color System.Drawing.Color::FromArgb(System.Int32,System.Int32,System.Int32)
IL_033A: callvirt       System.Void System.Windows.Forms.Control::set_BackColor(System.Drawing.Color)
IL_033F: ldarg.0        
IL_0340: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_0345: ldc.i4         221
IL_034A: ldc.i4         231
IL_034F: ldc.i4         238
IL_0354: call           System.Drawing.Color System.Drawing.Color::FromArgb(System.Int32,System.Int32,System.Int32)
IL_0359: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitContainer::set_CollapsingAreaColor(System.Drawing.Color)
IL_035E: ldarg.0        
IL_035F: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_0364: ldc.i4.5       
IL_0365: callvirt       System.Void System.Windows.Forms.Control::set_Dock(System.Windows.Forms.DockStyle)
IL_036A: ldarg.0        
IL_036B: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_0370: ldc.i4.s       119
IL_0372: ldc.i4         147
IL_0377: ldc.i4         185
IL_037C: call           System.Drawing.Color System.Drawing.Color::FromArgb(System.Int32,System.Int32,System.Int32)
IL_0381: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitContainer::set_FixedLineColor(System.Drawing.Color)
IL_0386: ldarg.0        
IL_0387: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_038C: ldc.i4.s       21
IL_038E: ldc.i4.s       66
IL_0390: ldc.i4         139
IL_0395: call           System.Drawing.Color System.Drawing.Color::FromArgb(System.Int32,System.Int32,System.Int32)
IL_039A: callvirt       System.Void System.Windows.Forms.Control::set_ForeColor(System.Drawing.Color)
IL_039F: ldarg.0        
IL_03A0: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_03A5: ldc.i4.s       27
IL_03A7: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitContainer::set_HeaderHeight(System.Int32)
IL_03AC: ldarg.0        
IL_03AD: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_03B2: ldc.i4.0       
IL_03B3: ldc.i4.0       
IL_03B4: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_03B9: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_03BE: ldarg.0        
IL_03BF: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_03C4: ldc.i4.3       
IL_03C5: ldc.i4.4       
IL_03C6: ldc.i4.3       
IL_03C7: ldc.i4.4       
IL_03C8: newobj         System.Void System.Windows.Forms.Padding::.ctor(System.Int32,System.Int32,System.Int32,System.Int32)
IL_03CD: callvirt       System.Void System.Windows.Forms.Control::set_Margin(System.Windows.Forms.Padding)
IL_03D2: ldarg.0        
IL_03D3: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_03D8: ldstr          "ctnDock"
IL_03DD: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_03E2: ldarg.0        
IL_03E3: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_03E8: callvirt       C1.Win.C1SplitContainer.PanelCollection C1.Win.C1SplitContainer.C1SplitContainer::get_Panels()
IL_03ED: ldarg.0        
IL_03EE: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_03F3: callvirt       System.Void System.Collections.ObjectModel.Collection`1<C1.Win.C1SplitContainer.C1SplitterPanel>::Add(!0)
IL_03F8: ldarg.0        
IL_03F9: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_03FE: callvirt       C1.Win.C1SplitContainer.PanelCollection C1.Win.C1SplitContainer.C1SplitContainer::get_Panels()
IL_0403: ldarg.0        
IL_0404: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInputBox
IL_0409: callvirt       System.Void System.Collections.ObjectModel.Collection`1<C1.Win.C1SplitContainer.C1SplitterPanel>::Add(!0)
IL_040E: ldarg.0        
IL_040F: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_0414: ldc.i4         792
IL_0419: ldc.i4         569
IL_041E: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_0423: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_0428: ldarg.0        
IL_0429: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_042E: ldc.i4.s       119
IL_0430: ldc.i4         147
IL_0435: ldc.i4         185
IL_043A: call           System.Drawing.Color System.Drawing.Color::FromArgb(System.Int32,System.Int32,System.Int32)
IL_043F: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitContainer::set_SplitterColor(System.Drawing.Color)
IL_0444: ldarg.0        
IL_0445: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_044A: ldc.i4.0       
IL_044B: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitContainer::set_SplitterWidth(System.Int32)
IL_0450: ldarg.0        
IL_0451: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_0456: ldc.i4.1       
IL_0457: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_045C: ldarg.0        
IL_045D: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_0462: ldc.i4.3       
IL_0463: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitContainer::set_ToolTipGradient(C1.Win.C1SplitContainer.ToolTipGradient)
IL_0468: ldarg.0        
IL_0469: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_046E: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_0473: ldarg.0        
IL_0474: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnCancle
IL_0479: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_047E: ldarg.0        
IL_047F: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_0484: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_0489: ldarg.0        
IL_048A: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnConfirm
IL_048F: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_0494: ldarg.0        
IL_0495: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_049A: ldc.i4.1       
IL_049B: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Dock(C1.Win.C1SplitContainer.PanelDockStyle)
IL_04A0: ldarg.0        
IL_04A1: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_04A6: ldc.i4.s       40
IL_04A8: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Height(System.Int32)
IL_04AD: ldarg.0        
IL_04AE: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_04B3: ldc.i4.0       
IL_04B4: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_KeepRelativeSize(System.Boolean)
IL_04B9: ldarg.0        
IL_04BA: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_04BF: ldc.i4.0       
IL_04C0: ldc.i4         529
IL_04C5: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_04CA: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Location(System.Drawing.Point)
IL_04CF: ldarg.0        
IL_04D0: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_04D5: ldstr          "pnlButtons"
IL_04DA: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_04DF: ldarg.0        
IL_04E0: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_04E5: ldc.i4.0       
IL_04E6: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Resizable(System.Boolean)
IL_04EB: ldarg.0        
IL_04EC: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_04F1: ldc.i4         792
IL_04F6: ldc.i4.s       40
IL_04F8: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_04FD: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Size(System.Drawing.Size)
IL_0502: ldarg.0        
IL_0503: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_0508: ldc.i4.2       
IL_0509: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_050E: ldarg.0        
IL_050F: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_0514: ldc.i4         792
IL_0519: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Width(System.Int32)
IL_051E: ldarg.0        
IL_051F: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnCancle
IL_0524: ldc.i4.s       10
IL_0526: callvirt       System.Void System.Windows.Forms.Control::set_Anchor(System.Windows.Forms.AnchorStyles)
IL_052B: ldarg.0        
IL_052C: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnCancle
IL_0531: ldc.i4         699
IL_0536: ldc.i4.7       
IL_0537: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_053C: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_0541: ldarg.0        
IL_0542: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnCancle
IL_0547: ldc.i4.3       
IL_0548: ldc.i4.4       
IL_0549: ldc.i4.3       
IL_054A: ldc.i4.4       
IL_054B: newobj         System.Void System.Windows.Forms.Padding::.ctor(System.Int32,System.Int32,System.Int32,System.Int32)
IL_0550: callvirt       System.Void System.Windows.Forms.Control::set_Margin(System.Windows.Forms.Padding)
IL_0555: ldarg.0        
IL_0556: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnCancle
IL_055B: ldstr          "btnCancle"
IL_0560: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_0565: ldarg.0        
IL_0566: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnCancle
IL_056B: ldc.i4.s       70
IL_056D: ldc.i4.s       26
IL_056F: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_0574: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_0579: ldarg.0        
IL_057A: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnCancle
IL_057F: ldc.i4.1       
IL_0580: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_0585: ldarg.0        
IL_0586: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnCancle
IL_058B: ldstr          "取消"
IL_0590: callvirt       System.Void System.Windows.Forms.Control::set_Text(System.String)
IL_0595: ldarg.0        
IL_0596: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnCancle
IL_059B: ldc.i4.1       
IL_059C: callvirt       System.Void C1.Win.C1Input.C1Button::set_UseVisualStyleBackColor(System.Boolean)
IL_05A1: ldarg.0        
IL_05A2: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnCancle
IL_05A7: ldc.i4.2       
IL_05A8: callvirt       System.Void C1.Win.C1Input.C1Button::set_VisualStyleBaseStyle(C1.Win.C1Input.VisualStyle)
IL_05AD: ldarg.0        
IL_05AE: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnCancle
IL_05B3: ldarg.0        
IL_05B4: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::btnCancle_Click(System.Object,System.EventArgs)
IL_05BA: newobj         System.Void System.EventHandler::.ctor(System.Object,System.IntPtr)
IL_05BF: callvirt       System.Void System.Windows.Forms.Control::add_Click(System.EventHandler)
IL_05C4: ldarg.0        
IL_05C5: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnConfirm
IL_05CA: ldc.i4.s       10
IL_05CC: callvirt       System.Void System.Windows.Forms.Control::set_Anchor(System.Windows.Forms.AnchorStyles)
IL_05D1: ldarg.0        
IL_05D2: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnConfirm
IL_05D7: ldc.i4         596
IL_05DC: ldc.i4.7       
IL_05DD: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_05E2: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_05E7: ldarg.0        
IL_05E8: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnConfirm
IL_05ED: ldc.i4.3       
IL_05EE: ldc.i4.4       
IL_05EF: ldc.i4.3       
IL_05F0: ldc.i4.4       
IL_05F1: newobj         System.Void System.Windows.Forms.Padding::.ctor(System.Int32,System.Int32,System.Int32,System.Int32)
IL_05F6: callvirt       System.Void System.Windows.Forms.Control::set_Margin(System.Windows.Forms.Padding)
IL_05FB: ldarg.0        
IL_05FC: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnConfirm
IL_0601: ldstr          "btnConfirm"
IL_0606: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_060B: ldarg.0        
IL_060C: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnConfirm
IL_0611: ldc.i4.s       70
IL_0613: ldc.i4.s       26
IL_0615: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_061A: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_061F: ldarg.0        
IL_0620: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnConfirm
IL_0625: ldc.i4.0       
IL_0626: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_062B: ldarg.0        
IL_062C: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnConfirm
IL_0631: ldstr          "确定"
IL_0636: callvirt       System.Void System.Windows.Forms.Control::set_Text(System.String)
IL_063B: ldarg.0        
IL_063C: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnConfirm
IL_0641: ldc.i4.1       
IL_0642: callvirt       System.Void C1.Win.C1Input.C1Button::set_UseVisualStyleBackColor(System.Boolean)
IL_0647: ldarg.0        
IL_0648: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnConfirm
IL_064D: ldarg.0        
IL_064E: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit::btnConfirm_Click(System.Object,System.EventArgs)
IL_0654: newobj         System.Void System.EventHandler::.ctor(System.Object,System.IntPtr)
IL_0659: callvirt       System.Void System.Windows.Forms.Control::add_Click(System.EventHandler)
IL_065E: ldarg.0        
IL_065F: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInputBox
IL_0664: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_0669: ldarg.0        
IL_066A: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_066F: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_0674: ldarg.0        
IL_0675: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInputBox
IL_067A: ldc.i4         528
IL_067F: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Height(System.Int32)
IL_0684: ldarg.0        
IL_0685: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInputBox
IL_068A: ldc.i4.0       
IL_068B: ldc.i4.0       
IL_068C: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_0691: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Location(System.Drawing.Point)
IL_0696: ldarg.0        
IL_0697: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInputBox
IL_069C: ldc.i4.0       
IL_069D: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_MinHeight(System.Int32)
IL_06A2: ldarg.0        
IL_06A3: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInputBox
IL_06A8: ldc.i4.s       52
IL_06AA: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_MinWidth(System.Int32)
IL_06AF: ldarg.0        
IL_06B0: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInputBox
IL_06B5: ldstr          "pnlInputBox"
IL_06BA: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_06BF: ldarg.0        
IL_06C0: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInputBox
IL_06C5: ldc.i4         792
IL_06CA: ldc.i4         528
IL_06CF: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_06D4: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Size(System.Drawing.Size)
IL_06D9: ldarg.0        
IL_06DA: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInputBox
IL_06DF: ldc.r8         100
IL_06E8: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_SizeRatio(System.Double)
IL_06ED: ldarg.0        
IL_06EE: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInputBox
IL_06F3: ldc.i4.1       
IL_06F4: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_06F9: ldarg.0        
IL_06FA: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInputBox
IL_06FF: ldc.i4         792
IL_0704: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Width(System.Int32)
IL_0709: ldarg.0        
IL_070A: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_070F: ldc.i4.0       
IL_0710: callvirt       System.Void C1.Win.C1Command.C1DockingTab::set_BorderStyle(System.Windows.Forms.BorderStyle)
IL_0715: ldarg.0        
IL_0716: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_071B: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_0720: ldarg.0        
IL_0721: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDropList
IL_0726: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_072B: ldarg.0        
IL_072C: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_0731: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_0736: ldarg.0        
IL_0737: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabEdit
IL_073C: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_0741: ldarg.0        
IL_0742: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_0747: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_074C: ldarg.0        
IL_074D: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDefault
IL_0752: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_0757: ldarg.0        
IL_0758: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_075D: ldc.i4.5       
IL_075E: callvirt       System.Void C1.Win.C1Command.C1DockingTab::set_Dock(System.Windows.Forms.DockStyle)
IL_0763: ldarg.0        
IL_0764: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_0769: ldc.i4.0       
IL_076A: ldc.i4.0       
IL_076B: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_0770: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_0775: ldarg.0        
IL_0776: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_077B: ldstr          "DockingTab"
IL_0780: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_0785: ldarg.0        
IL_0786: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_078B: ldc.i4         792
IL_0790: ldc.i4         528
IL_0795: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_079A: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_079F: ldarg.0        
IL_07A0: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_07A5: ldc.i4.1       
IL_07A6: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_07AB: ldarg.0        
IL_07AC: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_07B1: ldc.i4.0       
IL_07B2: callvirt       System.Void C1.Win.C1Command.C1DockingTab::set_TabsShowFocusCues(System.Boolean)
IL_07B7: ldarg.0        
IL_07B8: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_07BD: ldc.i4.5       
IL_07BE: callvirt       System.Void C1.Win.C1Command.C1DockingTab::set_TabsSpacing(System.Int32)
IL_07C3: ldarg.0        
IL_07C4: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_07C9: ldc.i4.6       
IL_07CA: callvirt       System.Void C1.Win.C1Command.C1DockingTab::set_TabStyle(C1.Win.C1Command.TabStyleEnum)
IL_07CF: ldarg.0        
IL_07D0: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_07D5: ldc.i4.0       
IL_07D6: callvirt       System.Void C1.Win.C1Command.C1DockingTab::set_VisualStyle(C1.Win.C1Command.VisualStyle)
IL_07DB: ldarg.0        
IL_07DC: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_07E1: ldc.i4.5       
IL_07E2: callvirt       System.Void C1.Win.C1Command.C1DockingTab::set_VisualStyleBase(C1.Win.C1Command.VisualStyle)
IL_07E7: ldarg.0        
IL_07E8: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDropList
IL_07ED: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_07F2: ldarg.0        
IL_07F3: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_07F8: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_07FD: ldarg.0        
IL_07FE: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDropList
IL_0803: ldc.i4.0       
IL_0804: ldc.i4.s       27
IL_0806: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_080B: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_0810: ldarg.0        
IL_0811: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDropList
IL_0816: ldstr          "tabDropList"
IL_081B: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_0820: ldarg.0        
IL_0821: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDropList
IL_0826: ldc.i4         792
IL_082B: ldc.i4         501
IL_0830: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_0835: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_083A: ldarg.0        
IL_083B: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDropList
IL_0840: ldc.i4.0       
IL_0841: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_0846: ldarg.0        
IL_0847: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDropList
IL_084C: ldstr          "下拉列表"
IL_0851: callvirt       System.Void System.Windows.Forms.Control::set_Text(System.String)
IL_0856: ldarg.0        
IL_0857: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_085C: ldc.i4.3       
IL_085D: callvirt       System.Void C1.Framework.XView::set_AutoSizeElement(C1.Framework.AutoSizeElement)
IL_0862: ldarg.0        
IL_0863: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_0868: ldc.i4         240
IL_086D: ldc.i4         240
IL_0872: ldc.i4         240
IL_0877: call           System.Drawing.Color System.Drawing.Color::FromArgb(System.Int32,System.Int32,System.Int32)
IL_087C: callvirt       System.Void System.Windows.Forms.Control::set_BackColor(System.Drawing.Color)
IL_0881: ldarg.0        
IL_0882: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_0887: ldc.i4         133
IL_088C: ldc.i4         133
IL_0891: ldc.i4         150
IL_0896: call           System.Drawing.Color System.Drawing.Color::FromArgb(System.Int32,System.Int32,System.Int32)
IL_089B: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitContainer::set_CollapsingCueColor(System.Drawing.Color)
IL_08A0: ldarg.0        
IL_08A1: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_08A6: ldc.i4.5       
IL_08A7: callvirt       System.Void System.Windows.Forms.Control::set_Dock(System.Windows.Forms.DockStyle)
IL_08AC: ldarg.0        
IL_08AD: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_08B2: ldc.i4.0       
IL_08B3: ldc.i4.0       
IL_08B4: ldc.i4.0       
IL_08B5: call           System.Drawing.Color System.Drawing.Color::FromArgb(System.Int32,System.Int32,System.Int32)
IL_08BA: callvirt       System.Void System.Windows.Forms.Control::set_ForeColor(System.Drawing.Color)
IL_08BF: ldarg.0        
IL_08C0: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_08C5: ldc.i4.0       
IL_08C6: ldc.i4.0       
IL_08C7: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_08CC: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_08D1: ldarg.0        
IL_08D2: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_08D7: ldstr          "ctnDropInput"
IL_08DC: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_08E1: ldarg.0        
IL_08E2: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_08E7: callvirt       C1.Win.C1SplitContainer.PanelCollection C1.Win.C1SplitContainer.C1SplitContainer::get_Panels()
IL_08EC: ldarg.0        
IL_08ED: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_08F2: callvirt       System.Void System.Collections.ObjectModel.Collection`1<C1.Win.C1SplitContainer.C1SplitterPanel>::Add(!0)
IL_08F7: ldarg.0        
IL_08F8: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_08FD: callvirt       C1.Win.C1SplitContainer.PanelCollection C1.Win.C1SplitContainer.C1SplitContainer::get_Panels()
IL_0902: ldarg.0        
IL_0903: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctions
IL_0908: callvirt       System.Void System.Collections.ObjectModel.Collection`1<C1.Win.C1SplitContainer.C1SplitterPanel>::Add(!0)
IL_090D: ldarg.0        
IL_090E: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_0913: callvirt       C1.Win.C1SplitContainer.PanelCollection C1.Win.C1SplitContainer.C1SplitContainer::get_Panels()
IL_0918: ldarg.0        
IL_0919: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctionHint
IL_091E: callvirt       System.Void System.Collections.ObjectModel.Collection`1<C1.Win.C1SplitContainer.C1SplitterPanel>::Add(!0)
IL_0923: ldarg.0        
IL_0924: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_0929: callvirt       C1.Win.C1SplitContainer.PanelCollection C1.Win.C1SplitContainer.C1SplitContainer::get_Panels()
IL_092E: ldarg.0        
IL_092F: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlCombo
IL_0934: callvirt       System.Void System.Collections.ObjectModel.Collection`1<C1.Win.C1SplitContainer.C1SplitterPanel>::Add(!0)
IL_0939: ldarg.0        
IL_093A: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_093F: ldc.i4         792
IL_0944: ldc.i4         501
IL_0949: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_094E: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_0953: ldarg.0        
IL_0954: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_0959: ldc.i4.0       
IL_095A: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitContainer::set_SplitterWidth(System.Int32)
IL_095F: ldarg.0        
IL_0960: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_0965: ldc.i4.0       
IL_0966: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_096B: ldarg.0        
IL_096C: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_0971: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_0976: ldarg.0        
IL_0977: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_097C: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_0981: ldarg.0        
IL_0982: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_0987: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_098C: ldarg.0        
IL_098D: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_0992: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_0997: ldarg.0        
IL_0998: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_099D: ldc.i4.1       
IL_099E: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Dock(C1.Win.C1SplitContainer.PanelDockStyle)
IL_09A3: ldarg.0        
IL_09A4: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_09A9: ldc.i4.s       40
IL_09AB: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Height(System.Int32)
IL_09B0: ldarg.0        
IL_09B1: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_09B6: ldc.i4.0       
IL_09B7: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_KeepRelativeSize(System.Boolean)
IL_09BC: ldarg.0        
IL_09BD: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_09C2: ldc.i4.0       
IL_09C3: ldc.i4         461
IL_09C8: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_09CD: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Location(System.Drawing.Point)
IL_09D2: ldarg.0        
IL_09D3: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_09D8: ldstr          "pnlInput"
IL_09DD: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_09E2: ldarg.0        
IL_09E3: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_09E8: ldc.i4.0       
IL_09E9: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Resizable(System.Boolean)
IL_09EE: ldarg.0        
IL_09EF: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_09F4: ldc.i4         792
IL_09F9: ldc.i4.s       40
IL_09FB: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_0A00: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Size(System.Drawing.Size)
IL_0A05: ldarg.0        
IL_0A06: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_0A0B: ldc.r8         11.396
IL_0A14: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_SizeRatio(System.Double)
IL_0A19: ldarg.0        
IL_0A1A: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_0A1F: ldc.i4.1       
IL_0A20: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_0A25: ldarg.0        
IL_0A26: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_0A2B: call           System.Drawing.Color System.Drawing.SystemColors::get_Control()
IL_0A30: callvirt       System.Void System.Windows.Forms.Control::set_BackColor(System.Drawing.Color)
IL_0A35: ldarg.0        
IL_0A36: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_0A3B: ldc.i4.0       
IL_0A3C: callvirt       System.Void C1.Win.C1Input.C1CheckBox::set_BorderStyle(System.Windows.Forms.BorderStyle)
IL_0A41: ldarg.0        
IL_0A42: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_0A47: call           System.Drawing.Color System.Drawing.SystemColors::get_ControlText()
IL_0A4C: callvirt       System.Void System.Windows.Forms.Control::set_ForeColor(System.Drawing.Color)
IL_0A51: ldarg.0        
IL_0A52: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_0A57: ldc.i4.s       105
IL_0A59: ldc.i4.7       
IL_0A5A: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_0A5F: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_0A64: ldarg.0        
IL_0A65: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_0A6A: ldstr          "ckbFreeInput"
IL_0A6F: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_0A74: ldarg.0        
IL_0A75: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_0A7A: ldc.i4.s       104
IL_0A7C: ldc.i4.s       24
IL_0A7E: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_0A83: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_0A88: ldarg.0        
IL_0A89: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_0A8E: ldc.i4.3       
IL_0A8F: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_0A94: ldarg.0        
IL_0A95: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_0A9A: ldstr          "允许自由输入"
IL_0A9F: callvirt       System.Void System.Windows.Forms.Control::set_Text(System.String)
IL_0AA4: ldarg.0        
IL_0AA5: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_0AAA: ldc.i4.1       
IL_0AAB: callvirt       System.Void C1.Win.C1Input.C1CheckBox::set_UseVisualStyleBackColor(System.Boolean)
IL_0AB0: ldarg.0        
IL_0AB1: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_0AB6: ldnull         
IL_0AB7: callvirt       System.Void C1.Win.C1Input.C1CheckBox::set_Value(System.Object)
IL_0ABC: ldarg.0        
IL_0ABD: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_0AC2: call           System.Drawing.Color System.Drawing.SystemColors::get_Control()
IL_0AC7: callvirt       System.Void System.Windows.Forms.Control::set_BackColor(System.Drawing.Color)
IL_0ACC: ldarg.0        
IL_0ACD: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_0AD2: ldc.i4.0       
IL_0AD3: callvirt       System.Void C1.Win.C1Input.C1CheckBox::set_BorderStyle(System.Windows.Forms.BorderStyle)
IL_0AD8: ldarg.0        
IL_0AD9: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_0ADE: call           System.Drawing.Color System.Drawing.SystemColors::get_ControlText()
IL_0AE3: callvirt       System.Void System.Windows.Forms.Control::set_ForeColor(System.Drawing.Color)
IL_0AE8: ldarg.0        
IL_0AE9: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_0AEE: ldc.i4.s       12
IL_0AF0: ldc.i4.7       
IL_0AF1: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_0AF6: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_0AFB: ldarg.0        
IL_0AFC: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_0B01: ldstr          "ckbMultiSelect"
IL_0B06: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_0B0B: ldarg.0        
IL_0B0C: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_0B11: ldc.i4.s       104
IL_0B13: ldc.i4.s       24
IL_0B15: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_0B1A: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_0B1F: ldarg.0        
IL_0B20: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_0B25: ldc.i4.2       
IL_0B26: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_0B2B: ldarg.0        
IL_0B2C: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_0B31: ldstr          "允许复选"
IL_0B36: callvirt       System.Void System.Windows.Forms.Control::set_Text(System.String)
IL_0B3B: ldarg.0        
IL_0B3C: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_0B41: ldc.i4.1       
IL_0B42: callvirt       System.Void C1.Win.C1Input.C1CheckBox::set_UseVisualStyleBackColor(System.Boolean)
IL_0B47: ldarg.0        
IL_0B48: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_0B4D: ldnull         
IL_0B4E: callvirt       System.Void C1.Win.C1Input.C1CheckBox::set_Value(System.Object)
IL_0B53: ldarg.0        
IL_0B54: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctions
IL_0B59: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_0B5E: ldarg.0        
IL_0B5F: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_0B64: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_0B69: ldarg.0        
IL_0B6A: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctions
IL_0B6F: ldc.i4.s       24
IL_0B71: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Height(System.Int32)
IL_0B76: ldarg.0        
IL_0B77: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctions
IL_0B7C: ldc.i4.0       
IL_0B7D: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_KeepRelativeSize(System.Boolean)
IL_0B82: ldarg.0        
IL_0B83: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctions
IL_0B88: ldc.i4.0       
IL_0B89: ldc.i4.0       
IL_0B8A: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_0B8F: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Location(System.Drawing.Point)
IL_0B94: ldarg.0        
IL_0B95: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctions
IL_0B9A: ldc.i4.s       24
IL_0B9C: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_MinHeight(System.Int32)
IL_0BA1: ldarg.0        
IL_0BA2: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctions
IL_0BA7: ldstr          "pnlFunctions"
IL_0BAC: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_0BB1: ldarg.0        
IL_0BB2: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctions
IL_0BB7: ldc.i4.0       
IL_0BB8: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Resizable(System.Boolean)
IL_0BBD: ldarg.0        
IL_0BBE: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctions
IL_0BC3: ldc.i4         792
IL_0BC8: ldc.i4.s       24
IL_0BCA: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_0BCF: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Size(System.Drawing.Size)
IL_0BD4: ldarg.0        
IL_0BD5: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctions
IL_0BDA: ldc.r8         7.767
IL_0BE3: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_SizeRatio(System.Double)
IL_0BE8: ldarg.0        
IL_0BE9: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctions
IL_0BEE: ldc.i4.3       
IL_0BEF: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_0BF4: ldarg.0        
IL_0BF5: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_0BFA: ldc.i4.0       
IL_0BFB: callvirt       System.Void C1.Win.C1Command.C1ToolBar::set_AutoSize(System.Boolean)
IL_0C00: ldarg.0        
IL_0C01: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_0C06: ldarg.0        
IL_0C07: ldfld          C1.Win.C1Command.C1CommandHolder Leqisoft.UI.Platform.frmAuxEdit::c1CommandHolder1
IL_0C0C: callvirt       System.Void C1.Win.C1Command.C1ToolBar::set_CommandHolder(C1.Win.C1Command.C1CommandHolder)
IL_0C11: ldarg.0        
IL_0C12: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_0C17: ldc.i4.5       
IL_0C18: callvirt       System.Void System.Windows.Forms.Control::set_Dock(System.Windows.Forms.DockStyle)
IL_0C1D: ldarg.0        
IL_0C1E: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_0C23: ldc.i4.0       
IL_0C24: ldc.i4.0       
IL_0C25: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_0C2A: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_0C2F: ldarg.0        
IL_0C30: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_0C35: ldc.i4.0       
IL_0C36: callvirt       System.Void C1.Win.C1Command.C1ToolBar::set_Movable(System.Boolean)
IL_0C3B: ldarg.0        
IL_0C3C: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_0C41: ldstr          "tbrFunctions"
IL_0C46: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_0C4B: ldarg.0        
IL_0C4C: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_0C51: ldc.i4         792
IL_0C56: ldc.i4.s       24
IL_0C58: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_0C5D: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_0C62: ldarg.0        
IL_0C63: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_0C68: ldstr          "c1ToolBar1"
IL_0C6D: callvirt       System.Void System.Windows.Forms.Control::set_Text(System.String)
IL_0C72: ldarg.0        
IL_0C73: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_0C78: ldc.i4.0       
IL_0C79: callvirt       System.Void C1.Win.C1Command.C1ToolBar::set_VisualStyle(C1.Win.C1Command.VisualStyle)
IL_0C7E: ldarg.0        
IL_0C7F: ldfld          C1.Win.C1Command.C1ToolBar Leqisoft.UI.Platform.frmAuxEdit::tbrFunctions
IL_0C84: ldc.i4.1       
IL_0C85: callvirt       System.Void C1.Win.C1Command.C1ToolBar::set_VisualStyleBase(C1.Win.C1Command.VisualStyle)
IL_0C8A: ldarg.0        
IL_0C8B: ldfld          C1.Win.C1Command.C1CommandHolder Leqisoft.UI.Platform.frmAuxEdit::c1CommandHolder1
IL_0C90: callvirt       C1.Win.C1Command.C1Commands C1.Win.C1Command.C1CommandHolder::get_Commands()
IL_0C95: ldarg.0        
IL_0C96: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::c1ContextMenu1
IL_0C9B: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_0CA0: pop            
IL_0CA1: ldarg.0        
IL_0CA2: ldfld          C1.Win.C1Command.C1CommandHolder Leqisoft.UI.Platform.frmAuxEdit::c1CommandHolder1
IL_0CA7: callvirt       C1.Win.C1Command.C1Commands C1.Win.C1Command.C1CommandHolder::get_Commands()
IL_0CAC: ldarg.0        
IL_0CAD: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::txbDropInputContextMenu
IL_0CB2: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_0CB7: pop            
IL_0CB8: ldarg.0        
IL_0CB9: ldfld          C1.Win.C1Command.C1CommandHolder Leqisoft.UI.Platform.frmAuxEdit::c1CommandHolder1
IL_0CBE: ldarg.0        
IL_0CBF: callvirt       System.Void C1.Win.C1Command.C1CommandHolder::set_Owner(System.Windows.Forms.Control)
IL_0CC4: ldarg.0        
IL_0CC5: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::c1ContextMenu1
IL_0CCA: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_0CCF: ldc.i4.1       
IL_0CD0: newarr         C1.Win.C1Command.C1CommandLink
IL_0CD5: dup            
IL_0CD6: ldc.i4.0       
IL_0CD7: ldarg.0        
IL_0CD8: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::c1CommandLink1
IL_0CDD: stelem.ref     
IL_0CDE: callvirt       System.Void C1.Win.C1Command.C1CommandLinks::AddRange(C1.Win.C1Command.C1CommandLink[])
IL_0CE3: ldarg.0        
IL_0CE4: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::c1ContextMenu1
IL_0CE9: ldstr          "c1ContextMenu1"
IL_0CEE: callvirt       System.Void C1.Win.C1Command.C1Command::set_Name(System.String)
IL_0CF3: ldarg.0        
IL_0CF4: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::c1ContextMenu1
IL_0CF9: ldstr          ""
IL_0CFE: callvirt       System.Void C1.Win.C1Command.C1Command::set_ShortcutText(System.String)
IL_0D03: ldarg.0        
IL_0D04: ldfld          C1.Win.C1Command.C1CommandLink Leqisoft.UI.Platform.frmAuxEdit::c1CommandLink1
IL_0D09: ldstr          "新命令"
IL_0D0E: callvirt       System.Void C1.Win.C1Command.C1CommandLink::set_Text(System.String)
IL_0D13: ldarg.0        
IL_0D14: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::txbDropInputContextMenu
IL_0D19: ldstr          "txbDropInputContextMenu"
IL_0D1E: callvirt       System.Void C1.Win.C1Command.C1Command::set_Name(System.String)
IL_0D23: ldarg.0        
IL_0D24: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::txbDropInputContextMenu
IL_0D29: ldstr          ""
IL_0D2E: callvirt       System.Void C1.Win.C1Command.C1Command::set_ShortcutText(System.String)
IL_0D33: ldarg.0        
IL_0D34: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctionHint
IL_0D39: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_0D3E: ldarg.0        
IL_0D3F: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_0D44: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_0D49: ldarg.0        
IL_0D4A: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctionHint
IL_0D4F: ldc.i4.1       
IL_0D50: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Dock(C1.Win.C1SplitContainer.PanelDockStyle)
IL_0D55: ldarg.0        
IL_0D56: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctionHint
IL_0D5B: ldc.i4.s       80
IL_0D5D: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Height(System.Int32)
IL_0D62: ldarg.0        
IL_0D63: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctionHint
IL_0D68: ldc.i4.0       
IL_0D69: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_KeepRelativeSize(System.Boolean)
IL_0D6E: ldarg.0        
IL_0D6F: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctionHint
IL_0D74: ldc.i4.0       
IL_0D75: ldc.i4         380
IL_0D7A: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_0D7F: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Location(System.Drawing.Point)
IL_0D84: ldarg.0        
IL_0D85: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctionHint
IL_0D8A: ldstr          "pnlFunctionHint"
IL_0D8F: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_0D94: ldarg.0        
IL_0D95: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctionHint
IL_0D9A: ldc.i4.0       
IL_0D9B: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Resizable(System.Boolean)
IL_0DA0: ldarg.0        
IL_0DA1: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctionHint
IL_0DA6: ldc.i4         792
IL_0DAB: ldc.i4.s       80
IL_0DAD: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_0DB2: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Size(System.Drawing.Size)
IL_0DB7: ldarg.0        
IL_0DB8: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctionHint
IL_0DBD: ldc.r8         16.194
IL_0DC6: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_SizeRatio(System.Double)
IL_0DCB: ldarg.0        
IL_0DCC: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctionHint
IL_0DD1: ldc.i4.4       
IL_0DD2: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_0DD7: ldarg.0        
IL_0DD8: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_0DDD: ldc.i4.0       
IL_0DDE: callvirt       System.Void System.Windows.Forms.Label::set_BorderStyle(System.Windows.Forms.BorderStyle)
IL_0DE3: ldarg.0        
IL_0DE4: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_0DE9: ldc.i4.5       
IL_0DEA: callvirt       System.Void System.Windows.Forms.Control::set_Dock(System.Windows.Forms.DockStyle)
IL_0DEF: ldarg.0        
IL_0DF0: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_0DF5: ldc.i4.0       
IL_0DF6: ldc.i4.0       
IL_0DF7: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_0DFC: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_0E01: ldarg.0        
IL_0E02: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_0E07: ldstr          "lblFunctionHint"
IL_0E0C: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_0E11: ldarg.0        
IL_0E12: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_0E17: ldc.i4         792
IL_0E1C: ldc.i4.s       80
IL_0E1E: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_0E23: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_0E28: ldarg.0        
IL_0E29: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_0E2E: ldc.i4.0       
IL_0E2F: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_0E34: ldarg.0        
IL_0E35: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_0E3A: ldnull         
IL_0E3B: callvirt       System.Void C1.Win.C1Input.C1Label::set_Tag(System.Object)
IL_0E40: ldarg.0        
IL_0E41: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_0E46: ldc.i4.s       16
IL_0E48: callvirt       System.Void System.Windows.Forms.Label::set_TextAlign(System.Drawing.ContentAlignment)
IL_0E4D: ldarg.0        
IL_0E4E: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlCombo
IL_0E53: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_0E58: ldarg.0        
IL_0E59: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0E5E: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_0E63: ldarg.0        
IL_0E64: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlCombo
IL_0E69: ldc.i4         354
IL_0E6E: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Height(System.Int32)
IL_0E73: ldarg.0        
IL_0E74: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlCombo
IL_0E79: ldc.i4.0       
IL_0E7A: ldc.i4.s       25
IL_0E7C: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_0E81: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Location(System.Drawing.Point)
IL_0E86: ldarg.0        
IL_0E87: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlCombo
IL_0E8C: ldstr          "pnlCombo"
IL_0E91: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_0E96: ldarg.0        
IL_0E97: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlCombo
IL_0E9C: ldc.i4         792
IL_0EA1: ldc.i4         354
IL_0EA6: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_0EAB: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Size(System.Drawing.Size)
IL_0EB0: ldarg.0        
IL_0EB1: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlCombo
IL_0EB6: ldc.i4.2       
IL_0EB7: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_0EBC: ldarg.0        
IL_0EBD: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabEdit
IL_0EC2: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_0EC7: ldarg.0        
IL_0EC8: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_0ECD: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_0ED2: ldarg.0        
IL_0ED3: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabEdit
IL_0ED8: ldc.i4.0       
IL_0ED9: ldc.i4.s       27
IL_0EDB: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_0EE0: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_0EE5: ldarg.0        
IL_0EE6: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabEdit
IL_0EEB: ldstr          "tabEdit"
IL_0EF0: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_0EF5: ldarg.0        
IL_0EF6: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabEdit
IL_0EFB: ldc.i4         792
IL_0F00: ldc.i4         501
IL_0F05: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_0F0A: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_0F0F: ldarg.0        
IL_0F10: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabEdit
IL_0F15: ldc.i4.2       
IL_0F16: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_0F1B: ldarg.0        
IL_0F1C: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabEdit
IL_0F21: ldstr          "编辑注释"
IL_0F26: callvirt       System.Void System.Windows.Forms.Control::set_Text(System.String)
IL_0F2B: ldarg.0        
IL_0F2C: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_0F31: ldc.i4.3       
IL_0F32: callvirt       System.Void C1.Framework.XView::set_AutoSizeElement(C1.Framework.AutoSizeElement)
IL_0F37: ldarg.0        
IL_0F38: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_0F3D: ldc.i4         240
IL_0F42: ldc.i4         240
IL_0F47: ldc.i4         240
IL_0F4C: call           System.Drawing.Color System.Drawing.Color::FromArgb(System.Int32,System.Int32,System.Int32)
IL_0F51: callvirt       System.Void System.Windows.Forms.Control::set_BackColor(System.Drawing.Color)
IL_0F56: ldarg.0        
IL_0F57: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_0F5C: ldc.i4         133
IL_0F61: ldc.i4         133
IL_0F66: ldc.i4         150
IL_0F6B: call           System.Drawing.Color System.Drawing.Color::FromArgb(System.Int32,System.Int32,System.Int32)
IL_0F70: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitContainer::set_CollapsingCueColor(System.Drawing.Color)
IL_0F75: ldarg.0        
IL_0F76: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_0F7B: ldc.i4.5       
IL_0F7C: callvirt       System.Void System.Windows.Forms.Control::set_Dock(System.Windows.Forms.DockStyle)
IL_0F81: ldarg.0        
IL_0F82: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_0F87: ldc.i4.0       
IL_0F88: ldc.i4.0       
IL_0F89: ldc.i4.0       
IL_0F8A: call           System.Drawing.Color System.Drawing.Color::FromArgb(System.Int32,System.Int32,System.Int32)
IL_0F8F: callvirt       System.Void System.Windows.Forms.Control::set_ForeColor(System.Drawing.Color)
IL_0F94: ldarg.0        
IL_0F95: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_0F9A: ldc.i4.0       
IL_0F9B: ldc.i4.0       
IL_0F9C: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_0FA1: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_0FA6: ldarg.0        
IL_0FA7: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_0FAC: ldstr          "ctnCommentInput"
IL_0FB1: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_0FB6: ldarg.0        
IL_0FB7: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_0FBC: callvirt       C1.Win.C1SplitContainer.PanelCollection C1.Win.C1SplitContainer.C1SplitContainer::get_Panels()
IL_0FC1: ldarg.0        
IL_0FC2: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel4
IL_0FC7: callvirt       System.Void System.Collections.ObjectModel.Collection`1<C1.Win.C1SplitContainer.C1SplitterPanel>::Add(!0)
IL_0FCC: ldarg.0        
IL_0FCD: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_0FD2: ldc.i4         792
IL_0FD7: ldc.i4         501
IL_0FDC: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_0FE1: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_0FE6: ldarg.0        
IL_0FE7: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_0FEC: ldc.i4.0       
IL_0FED: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitContainer::set_SplitterWidth(System.Int32)
IL_0FF2: ldarg.0        
IL_0FF3: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_0FF8: ldc.i4.1       
IL_0FF9: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_0FFE: ldarg.0        
IL_0FFF: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel4
IL_1004: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_1009: ldarg.0        
IL_100A: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_100F: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_1014: ldarg.0        
IL_1015: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel4
IL_101A: ldc.i4         501
IL_101F: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Height(System.Int32)
IL_1024: ldarg.0        
IL_1025: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel4
IL_102A: ldc.i4.0       
IL_102B: ldc.i4.0       
IL_102C: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_1031: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Location(System.Drawing.Point)
IL_1036: ldarg.0        
IL_1037: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel4
IL_103C: ldstr          "c1SplitterPanel4"
IL_1041: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_1046: ldarg.0        
IL_1047: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel4
IL_104C: ldc.i4         792
IL_1051: ldc.i4         501
IL_1056: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_105B: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Size(System.Drawing.Size)
IL_1060: ldarg.0        
IL_1061: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel4
IL_1066: ldc.i4.1       
IL_1067: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_106C: ldarg.0        
IL_106D: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_1072: ldc.i4.5       
IL_1073: callvirt       System.Void System.Windows.Forms.Control::set_Dock(System.Windows.Forms.DockStyle)
IL_1078: ldarg.0        
IL_1079: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_107E: ldc.i4.0       
IL_107F: ldc.i4.0       
IL_1080: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_1085: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_108A: ldarg.0        
IL_108B: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_1090: ldc.i4.3       
IL_1091: ldc.i4.4       
IL_1092: ldc.i4.3       
IL_1093: ldc.i4.4       
IL_1094: newobj         System.Void System.Windows.Forms.Padding::.ctor(System.Int32,System.Int32,System.Int32,System.Int32)
IL_1099: callvirt       System.Void System.Windows.Forms.Control::set_Margin(System.Windows.Forms.Padding)
IL_109E: ldarg.0        
IL_109F: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_10A4: ldc.i4.1       
IL_10A5: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_Multiline(System.Boolean)
IL_10AA: ldarg.0        
IL_10AB: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_10B0: ldstr          "txbCommentInput"
IL_10B5: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_10BA: ldarg.0        
IL_10BB: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_10C0: ldc.i4.2       
IL_10C1: callvirt       System.Void System.Windows.Forms.TextBox::set_ScrollBars(System.Windows.Forms.ScrollBars)
IL_10C6: ldarg.0        
IL_10C7: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_10CC: ldc.i4         792
IL_10D1: ldc.i4         501
IL_10D6: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_10DB: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_10E0: ldarg.0        
IL_10E1: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_10E6: ldc.i4.0       
IL_10E7: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_10EC: ldarg.0        
IL_10ED: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_10F2: ldnull         
IL_10F3: callvirt       System.Void C1.Win.C1Input.C1TextBox::set_Tag(System.Object)
IL_10F8: ldarg.0        
IL_10F9: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_10FE: ldc.i4.1       
IL_10FF: callvirt       System.Void C1.Win.C1Input.C1TextBox::set_TextDetached(System.Boolean)
IL_1104: ldarg.0        
IL_1105: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDefault
IL_110A: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_110F: ldarg.0        
IL_1110: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_1115: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_111A: ldarg.0        
IL_111B: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDefault
IL_1120: ldc.i4.0       
IL_1121: ldc.i4.s       27
IL_1123: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_1128: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_112D: ldarg.0        
IL_112E: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDefault
IL_1133: ldstr          "tabDefault"
IL_1138: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_113D: ldarg.0        
IL_113E: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDefault
IL_1143: ldc.i4         792
IL_1148: ldc.i4         501
IL_114D: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_1152: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_1157: ldarg.0        
IL_1158: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDefault
IL_115D: ldc.i4.1       
IL_115E: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_1163: ldarg.0        
IL_1164: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDefault
IL_1169: ldstr          "默认内容"
IL_116E: callvirt       System.Void System.Windows.Forms.Control::set_Text(System.String)
IL_1173: ldarg.0        
IL_1174: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_1179: ldc.i4.3       
IL_117A: callvirt       System.Void C1.Framework.XView::set_AutoSizeElement(C1.Framework.AutoSizeElement)
IL_117F: ldarg.0        
IL_1180: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_1185: ldc.i4         240
IL_118A: ldc.i4         240
IL_118F: ldc.i4         240
IL_1194: call           System.Drawing.Color System.Drawing.Color::FromArgb(System.Int32,System.Int32,System.Int32)
IL_1199: callvirt       System.Void System.Windows.Forms.Control::set_BackColor(System.Drawing.Color)
IL_119E: ldarg.0        
IL_119F: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_11A4: ldc.i4         133
IL_11A9: ldc.i4         133
IL_11AE: ldc.i4         150
IL_11B3: call           System.Drawing.Color System.Drawing.Color::FromArgb(System.Int32,System.Int32,System.Int32)
IL_11B8: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitContainer::set_CollapsingCueColor(System.Drawing.Color)
IL_11BD: ldarg.0        
IL_11BE: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_11C3: ldc.i4.5       
IL_11C4: callvirt       System.Void System.Windows.Forms.Control::set_Dock(System.Windows.Forms.DockStyle)
IL_11C9: ldarg.0        
IL_11CA: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_11CF: ldc.i4.0       
IL_11D0: ldc.i4.0       
IL_11D1: ldc.i4.0       
IL_11D2: call           System.Drawing.Color System.Drawing.Color::FromArgb(System.Int32,System.Int32,System.Int32)
IL_11D7: callvirt       System.Void System.Windows.Forms.Control::set_ForeColor(System.Drawing.Color)
IL_11DC: ldarg.0        
IL_11DD: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_11E2: ldc.i4.0       
IL_11E3: ldc.i4.0       
IL_11E4: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_11E9: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_11EE: ldarg.0        
IL_11EF: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_11F4: ldstr          "ctnDefaultInput"
IL_11F9: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_11FE: ldarg.0        
IL_11FF: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_1204: callvirt       C1.Win.C1SplitContainer.PanelCollection C1.Win.C1SplitContainer.C1SplitContainer::get_Panels()
IL_1209: ldarg.0        
IL_120A: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel2
IL_120F: callvirt       System.Void System.Collections.ObjectModel.Collection`1<C1.Win.C1SplitContainer.C1SplitterPanel>::Add(!0)
IL_1214: ldarg.0        
IL_1215: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_121A: ldc.i4         792
IL_121F: ldc.i4         501
IL_1224: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_1229: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_122E: ldarg.0        
IL_122F: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_1234: ldc.i4.0       
IL_1235: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitContainer::set_SplitterWidth(System.Int32)
IL_123A: ldarg.0        
IL_123B: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_1240: ldc.i4.1       
IL_1241: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_1246: ldarg.0        
IL_1247: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel2
IL_124C: callvirt       System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_1251: ldarg.0        
IL_1252: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_1257: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_125C: ldarg.0        
IL_125D: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel2
IL_1262: ldc.i4         501
IL_1267: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Height(System.Int32)
IL_126C: ldarg.0        
IL_126D: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel2
IL_1272: ldc.i4.0       
IL_1273: ldc.i4.0       
IL_1274: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_1279: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Location(System.Drawing.Point)
IL_127E: ldarg.0        
IL_127F: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel2
IL_1284: ldstr          "c1SplitterPanel2"
IL_1289: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_128E: ldarg.0        
IL_128F: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel2
IL_1294: ldc.i4         792
IL_1299: ldc.i4         501
IL_129E: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_12A3: callvirt       System.Void C1.Win.C1SplitContainer.C1SplitterPanel::set_Size(System.Drawing.Size)
IL_12A8: ldarg.0        
IL_12A9: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel2
IL_12AE: ldc.i4.1       
IL_12AF: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_12B4: ldarg.0        
IL_12B5: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_12BA: ldc.i4.5       
IL_12BB: callvirt       System.Void System.Windows.Forms.Control::set_Dock(System.Windows.Forms.DockStyle)
IL_12C0: ldarg.0        
IL_12C1: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_12C6: ldc.i4.0       
IL_12C7: ldc.i4.0       
IL_12C8: newobj         System.Void System.Drawing.Point::.ctor(System.Int32,System.Int32)
IL_12CD: callvirt       System.Void System.Windows.Forms.Control::set_Location(System.Drawing.Point)
IL_12D2: ldarg.0        
IL_12D3: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_12D8: ldc.i4.3       
IL_12D9: ldc.i4.4       
IL_12DA: ldc.i4.3       
IL_12DB: ldc.i4.4       
IL_12DC: newobj         System.Void System.Windows.Forms.Padding::.ctor(System.Int32,System.Int32,System.Int32,System.Int32)
IL_12E1: callvirt       System.Void System.Windows.Forms.Control::set_Margin(System.Windows.Forms.Padding)
IL_12E6: ldarg.0        
IL_12E7: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_12EC: ldc.i4.1       
IL_12ED: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_Multiline(System.Boolean)
IL_12F2: ldarg.0        
IL_12F3: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_12F8: ldstr          "txbDefaultInput"
IL_12FD: callvirt       System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_1302: ldarg.0        
IL_1303: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_1308: ldc.i4.2       
IL_1309: callvirt       System.Void System.Windows.Forms.TextBox::set_ScrollBars(System.Windows.Forms.ScrollBars)
IL_130E: ldarg.0        
IL_130F: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_1314: ldc.i4         792
IL_1319: ldc.i4         501
IL_131E: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_1323: callvirt       System.Void System.Windows.Forms.Control::set_Size(System.Drawing.Size)
IL_1328: ldarg.0        
IL_1329: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_132E: ldc.i4.0       
IL_132F: callvirt       System.Void System.Windows.Forms.Control::set_TabIndex(System.Int32)
IL_1334: ldarg.0        
IL_1335: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_133A: ldnull         
IL_133B: callvirt       System.Void C1.Win.C1Input.C1TextBox::set_Tag(System.Object)
IL_1340: ldarg.0        
IL_1341: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_1346: ldc.i4.1       
IL_1347: callvirt       System.Void C1.Win.C1Input.C1TextBox::set_TextDetached(System.Boolean)
IL_134C: ldarg.0        
IL_134D: ldc.r4         7
IL_1352: ldc.r4         17
IL_1357: newobj         System.Void System.Drawing.SizeF::.ctor(System.Single,System.Single)
IL_135C: call           System.Void System.Windows.Forms.ContainerControl::set_AutoScaleDimensions(System.Drawing.SizeF)
IL_1361: ldarg.0        
IL_1362: ldc.i4.1       
IL_1363: call           System.Void System.Windows.Forms.ContainerControl::set_AutoScaleMode(System.Windows.Forms.AutoScaleMode)
IL_1368: ldarg.0        
IL_1369: ldc.i4         792
IL_136E: ldc.i4         569
IL_1373: newobj         System.Void System.Drawing.Size::.ctor(System.Int32,System.Int32)
IL_1378: call           System.Void System.Windows.Forms.Form::set_ClientSize(System.Drawing.Size)
IL_137D: ldarg.0        
IL_137E: call           System.Windows.Forms.Control/ControlCollection System.Windows.Forms.Control::get_Controls()
IL_1383: ldarg.0        
IL_1384: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_1389: callvirt       System.Void System.Windows.Forms.Control/ControlCollection::Add(System.Windows.Forms.Control)
IL_138E: ldarg.0        
IL_138F: ldstr          "Microsoft YaHei"
IL_1394: ldc.r4         9
IL_1399: ldc.i4.0       
IL_139A: ldc.i4.3       
IL_139B: ldc.i4         134
IL_13A0: newobj         System.Void System.Drawing.Font::.ctor(System.String,System.Single,System.Drawing.FontStyle,System.Drawing.GraphicsUnit,System.Byte)
IL_13A5: callvirt       System.Void System.Windows.Forms.Control::set_Font(System.Drawing.Font)
IL_13AA: ldarg.0        
IL_13AB: ldc.i4.3       
IL_13AC: ldc.i4.4       
IL_13AD: ldc.i4.3       
IL_13AE: ldc.i4.4       
IL_13AF: newobj         System.Void System.Windows.Forms.Padding::.ctor(System.Int32,System.Int32,System.Int32,System.Int32)
IL_13B4: call           System.Void System.Windows.Forms.Form::set_Margin(System.Windows.Forms.Padding)
IL_13B9: ldarg.0        
IL_13BA: ldstr          "frmAuxEdit"
IL_13BF: call           System.Void System.Windows.Forms.Control::set_Name(System.String)
IL_13C4: ldarg.0        
IL_13C5: ldc.i4.0       
IL_13C6: call           System.Void System.Windows.Forms.Form::set_ShowInTaskbar(System.Boolean)
IL_13CB: ldarg.0        
IL_13CC: ldc.i4.1       
IL_13CD: call           System.Void System.Windows.Forms.Form::set_StartPosition(System.Windows.Forms.FormStartPosition)
IL_13D2: ldarg.0        
IL_13D3: ldstr          "辅助编辑"
IL_13D8: callvirt       System.Void System.Windows.Forms.Control::set_Text(System.String)
IL_13DD: ldarg.0        
IL_13DE: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_13E3: callvirt       System.Void System.ComponentModel.ISupportInitialize::EndInit()
IL_13E8: ldarg.0        
IL_13E9: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDock
IL_13EE: ldc.i4.0       
IL_13EF: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_13F4: ldarg.0        
IL_13F5: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlButtons
IL_13FA: ldc.i4.0       
IL_13FB: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_1400: ldarg.0        
IL_1401: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnCancle
IL_1406: callvirt       System.Void System.ComponentModel.ISupportInitialize::EndInit()
IL_140B: ldarg.0        
IL_140C: ldfld          C1.Win.C1Input.C1Button Leqisoft.UI.Platform.frmAuxEdit::btnConfirm
IL_1411: callvirt       System.Void System.ComponentModel.ISupportInitialize::EndInit()
IL_1416: ldarg.0        
IL_1417: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInputBox
IL_141C: ldc.i4.0       
IL_141D: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_1422: ldarg.0        
IL_1423: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_1428: callvirt       System.Void System.ComponentModel.ISupportInitialize::EndInit()
IL_142D: ldarg.0        
IL_142E: ldfld          C1.Win.C1Command.C1DockingTab Leqisoft.UI.Platform.frmAuxEdit::DockingTab
IL_1433: ldc.i4.0       
IL_1434: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_1439: ldarg.0        
IL_143A: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDropList
IL_143F: ldc.i4.0       
IL_1440: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_1445: ldarg.0        
IL_1446: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_144B: callvirt       System.Void System.ComponentModel.ISupportInitialize::EndInit()
IL_1450: ldarg.0        
IL_1451: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDropInput
IL_1456: ldc.i4.0       
IL_1457: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_145C: ldarg.0        
IL_145D: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlInput
IL_1462: ldc.i4.0       
IL_1463: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_1468: ldarg.0        
IL_1469: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_146E: callvirt       System.Void System.ComponentModel.ISupportInitialize::EndInit()
IL_1473: ldarg.0        
IL_1474: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_1479: callvirt       System.Void System.ComponentModel.ISupportInitialize::EndInit()
IL_147E: ldarg.0        
IL_147F: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctions
IL_1484: ldc.i4.0       
IL_1485: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_148A: ldarg.0        
IL_148B: ldfld          C1.Win.C1Command.C1CommandHolder Leqisoft.UI.Platform.frmAuxEdit::c1CommandHolder1
IL_1490: callvirt       System.Void System.ComponentModel.ISupportInitialize::EndInit()
IL_1495: ldarg.0        
IL_1496: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlFunctionHint
IL_149B: ldc.i4.0       
IL_149C: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_14A1: ldarg.0        
IL_14A2: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_14A7: callvirt       System.Void System.ComponentModel.ISupportInitialize::EndInit()
IL_14AC: ldarg.0        
IL_14AD: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::pnlCombo
IL_14B2: ldc.i4.0       
IL_14B3: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_14B8: ldarg.0        
IL_14B9: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabEdit
IL_14BE: ldc.i4.0       
IL_14BF: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_14C4: ldarg.0        
IL_14C5: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_14CA: callvirt       System.Void System.ComponentModel.ISupportInitialize::EndInit()
IL_14CF: ldarg.0        
IL_14D0: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnCommentInput
IL_14D5: ldc.i4.0       
IL_14D6: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_14DB: ldarg.0        
IL_14DC: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel4
IL_14E1: ldc.i4.0       
IL_14E2: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_14E7: ldarg.0        
IL_14E8: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_14ED: callvirt       System.Void System.ComponentModel.ISupportInitialize::EndInit()
IL_14F2: ldarg.0        
IL_14F3: ldfld          C1.Win.C1Command.C1DockingTabPage Leqisoft.UI.Platform.frmAuxEdit::tabDefault
IL_14F8: ldc.i4.0       
IL_14F9: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_14FE: ldarg.0        
IL_14FF: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_1504: callvirt       System.Void System.ComponentModel.ISupportInitialize::EndInit()
IL_1509: ldarg.0        
IL_150A: ldfld          C1.Win.C1SplitContainer.C1SplitContainer Leqisoft.UI.Platform.frmAuxEdit::ctnDefaultInput
IL_150F: ldc.i4.0       
IL_1510: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_1515: ldarg.0        
IL_1516: ldfld          C1.Win.C1SplitContainer.C1SplitterPanel Leqisoft.UI.Platform.frmAuxEdit::c1SplitterPanel2
IL_151B: ldc.i4.0       
IL_151C: callvirt       System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_1521: ldarg.0        
IL_1522: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_1527: callvirt       System.Void System.ComponentModel.ISupportInitialize::EndInit()
IL_152C: ldarg.0        
IL_152D: ldc.i4.0       
IL_152E: call           System.Void System.Windows.Forms.Control::ResumeLayout(System.Boolean)
IL_1533: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::PopulateOtherFunctions() ---
HasBody=True RVA=0x00028830 IL=93
Locals:
  [0] System.Collections.Generic.List`1<Leqisoft.Model.FunctionInfo>
  [1] System.Collections.Generic.IEnumerator`1<System.Linq.IGrouping`2<System.String,Leqisoft.Model.FunctionInfo>>
  [2] System.Linq.IGrouping`2<System.String,Leqisoft.Model.FunctionInfo>
  [3] C1.Win.C1Command.C1CommandMenu
  [4] C1.Win.C1Command.C1CommandLink
  [5] System.Collections.Generic.IEnumerator`1<Leqisoft.Model.FunctionInfo>
  [6] Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass101_0
  [7] C1.Win.C1Command.C1Command
  [8] C1.Win.C1Command.C1CommandLink
ExceptionHandlers:
  Finally try=IL_0072..IL_00F5 handler=IL_00F5..IL_0101
  Finally try=IL_0031..IL_010E handler=IL_010E..IL_0118
IL_0000: call           System.Collections.Generic.List`1<Leqisoft.Model.FunctionInfo> Leqisoft.Model.FunctionInfo::get_PublicFunctionInfos()
IL_0005: stloc.0        
IL_0006: ldloc.0        
IL_0007: ldsfld         System.Func`2<Leqisoft.Model.FunctionInfo,System.String> Leqisoft.UI.Platform.frmAuxEdit/<>c::<>9__101_0
IL_000C: dup            
IL_000D: brtrue.s       IL_0026: call System.Collections.Generic.IEnumerable`1<System.Linq.IGrouping`2<!!1,!!0>> System.Linq.Enumerable::GroupBy<Leqisoft.Model.FunctionInfo,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
IL_000F: pop            
IL_0010: ldsfld         Leqisoft.UI.Platform.frmAuxEdit/<>c Leqisoft.UI.Platform.frmAuxEdit/<>c::<>9
IL_0015: ldftn          System.String Leqisoft.UI.Platform.frmAuxEdit/<>c::<PopulateOtherFunctions>b__101_0(Leqisoft.Model.FunctionInfo)
IL_001B: newobj         System.Void System.Func`2<Leqisoft.Model.FunctionInfo,System.String>::.ctor(System.Object,System.IntPtr)
IL_0020: dup            
IL_0021: stsfld         System.Func`2<Leqisoft.Model.FunctionInfo,System.String> Leqisoft.UI.Platform.frmAuxEdit/<>c::<>9__101_0
IL_0026: call           System.Collections.Generic.IEnumerable`1<System.Linq.IGrouping`2<!!1,!!0>> System.Linq.Enumerable::GroupBy<Leqisoft.Model.FunctionInfo,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
IL_002B: callvirt       System.Collections.Generic.IEnumerator`1<!0> System.Collections.Generic.IEnumerable`1<System.Linq.IGrouping`2<System.String,Leqisoft.Model.FunctionInfo>>::GetEnumerator()
IL_0030: stloc.1        
IL_0031: br             IL_0101: ldloc.1
IL_0036: ldloc.1        
IL_0037: callvirt       !0 System.Collections.Generic.IEnumerator`1<System.Linq.IGrouping`2<System.String,Leqisoft.Model.FunctionInfo>>::get_Current()
IL_003C: stloc.2        
IL_003D: newobj         System.Void C1.Win.C1Command.C1CommandMenu::.ctor()
IL_0042: dup            
IL_0043: ldloc.2        
IL_0044: callvirt       !0 System.Linq.IGrouping`2<System.String,Leqisoft.Model.FunctionInfo>::get_Key()
IL_0049: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_004E: stloc.3        
IL_004F: ldloc.3        
IL_0050: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor(C1.Win.C1Command.C1Command)
IL_0055: stloc.s        V_4
IL_0057: ldarg.0        
IL_0058: ldfld          C1.Win.C1Command.C1ContextMenu Leqisoft.UI.Platform.frmAuxEdit::ctxOtherFunc
IL_005D: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_0062: ldloc.s        V_4
IL_0064: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_0069: pop            
IL_006A: ldloc.2        
IL_006B: callvirt       System.Collections.Generic.IEnumerator`1<!0> System.Collections.Generic.IEnumerable`1<Leqisoft.Model.FunctionInfo>::GetEnumerator()
IL_0070: stloc.s        V_5
IL_0072: br.s           IL_00ea: ldloc.s V_5
IL_0074: newobj         System.Void Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass101_0::.ctor()
IL_0079: stloc.s        V_6
IL_007B: ldloc.s        V_6
IL_007D: ldarg.0        
IL_007E: stfld          Leqisoft.UI.Platform.frmAuxEdit Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass101_0::<>4__this
IL_0083: ldloc.s        V_6
IL_0085: ldloc.s        V_5
IL_0087: callvirt       !0 System.Collections.Generic.IEnumerator`1<Leqisoft.Model.FunctionInfo>::get_Current()
IL_008C: stfld          Leqisoft.Model.FunctionInfo Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass101_0::f
IL_0091: call           Leqisoft.UI.Platform.MainForm Leqisoft.UI.Platform.Program::get_MainForm()
IL_0096: ldloc.s        V_6
IL_0098: ldfld          Leqisoft.Model.FunctionInfo Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass101_0::f
IL_009D: ldc.i4.0       
IL_009E: ldc.i4.0       
IL_009F: callvirt       System.Boolean Leqisoft.UI.Platform.MainForm::IsAllowShowFunctionInfoAtCurrentView(Leqisoft.Model.FunctionInfo,System.Boolean,System.Boolean)
IL_00A4: brfalse.s      IL_00ea: ldloc.s V_5
IL_00A6: newobj         System.Void C1.Win.C1Command.C1Command::.ctor()
IL_00AB: dup            
IL_00AC: ldloc.s        V_6
IL_00AE: ldfld          Leqisoft.Model.FunctionInfo Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass101_0::f
IL_00B3: callvirt       System.String Leqisoft.Model.FunctionInfo::get_Name()
IL_00B8: callvirt       System.Void C1.Win.C1Command.C1Command::set_Text(System.String)
IL_00BD: stloc.s        V_7
IL_00BF: ldloc.s        V_7
IL_00C1: newobj         System.Void C1.Win.C1Command.C1CommandLink::.ctor(C1.Win.C1Command.C1Command)
IL_00C6: stloc.s        V_8
IL_00C8: ldloc.s        V_7
IL_00CA: ldloc.s        V_6
IL_00CC: ldftn          System.Void Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass101_0::<PopulateOtherFunctions>b__1(System.Object,C1.Win.C1Command.ClickEventArgs)
IL_00D2: newobj         System.Void C1.Win.C1Command.ClickEventHandler::.ctor(System.Object,System.IntPtr)
IL_00D7: callvirt       System.Void C1.Win.C1Command.C1Command::add_Click(C1.Win.C1Command.ClickEventHandler)
IL_00DC: ldloc.3        
IL_00DD: callvirt       C1.Win.C1Command.C1CommandLinks C1.Win.C1Command.C1CommandMenu::get_CommandLinks()
IL_00E2: ldloc.s        V_8
IL_00E4: callvirt       System.Int32 System.Collections.ArrayList::Add(System.Object)
IL_00E9: pop            
IL_00EA: ldloc.s        V_5
IL_00EC: callvirt       System.Boolean System.Collections.IEnumerator::MoveNext()
IL_00F1: brtrue.s       IL_0074: newobj System.Void Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass101_0::.ctor()
IL_00F3: leave.s        IL_0101: ldloc.1
IL_00F5: ldloc.s        V_5
IL_00F7: brfalse.s      IL_0100: endfinally
IL_00F9: ldloc.s        V_5
IL_00FB: callvirt       System.Void System.IDisposable::Dispose()
IL_0100: endfinally     
IL_0101: ldloc.1        
IL_0102: callvirt       System.Boolean System.Collections.IEnumerator::MoveNext()
IL_0107: brtrue         IL_0036: ldloc.1
IL_010C: leave.s        IL_0118: ret
IL_010E: ldloc.1        
IL_010F: brfalse.s      IL_0117: endfinally
IL_0111: ldloc.1        
IL_0112: callvirt       System.Void System.IDisposable::Dispose()
IL_0117: endfinally     
IL_0118: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::ResetQueryStatus() ---
HasBody=True RVA=0x0002805D IL=10
IL_0000: ldarg.0        
IL_0001: ldc.i4.0       
IL_0002: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::set_QueryValueChanged(System.Boolean)
IL_0007: ldarg.0        
IL_0008: ldc.i4.0       
IL_0009: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::set_QueryDefaultValueChanged(System.Boolean)
IL_000E: ldarg.0        
IL_000F: ldc.i4.0       
IL_0010: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::set_QueryCommentValueChanged(System.Boolean)
IL_0015: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::set_AllowFreeInput(System.Boolean) ---
HasBody=True RVA=0x00027C06 IL=8
IL_0000: ldarg.0        
IL_0001: ldarg.1        
IL_0002: stfld          System.Boolean Leqisoft.UI.Platform.frmAuxEdit::_allowFreeInput
IL_0007: ldarg.0        
IL_0008: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_000D: ldarg.1        
IL_000E: callvirt       System.Void System.Windows.Forms.CheckBox::set_Checked(System.Boolean)
IL_0013: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::set_AllowMultiSelect(System.Boolean) ---
HasBody=True RVA=0x00027C23 IL=18
IL_0000: ldarg.0        
IL_0001: ldarg.1        
IL_0002: stfld          System.Boolean Leqisoft.UI.Platform.frmAuxEdit::_allowMultiSelect
IL_0007: ldarg.0        
IL_0008: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbMultiSelect
IL_000D: ldarg.1        
IL_000E: callvirt       System.Void System.Windows.Forms.CheckBox::set_Checked(System.Boolean)
IL_0013: ldarg.1        
IL_0014: brfalse.s      IL_002e: ret
IL_0016: ldarg.0        
IL_0017: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_001C: ldc.i4.0       
IL_001D: callvirt       System.Void System.Windows.Forms.CheckBox::set_Checked(System.Boolean)
IL_0022: ldarg.0        
IL_0023: ldfld          C1.Win.C1Input.C1CheckBox Leqisoft.UI.Platform.frmAuxEdit::ckbFreeInput
IL_0028: ldc.i4.0       
IL_0029: callvirt       System.Void System.Windows.Forms.Control::set_Enabled(System.Boolean)
IL_002E: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::set_CommentNotice(System.String) ---
HasBody=True RVA=0x00027B8F IL=4
IL_0000: ldarg.0        
IL_0001: ldarg.1        
IL_0002: stfld          System.String Leqisoft.UI.Platform.frmAuxEdit::_commentNotice
IL_0007: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::set_CommentValue(System.String) ---
HasBody=True RVA=0x00027BE4 IL=9
IL_0000: ldarg.0        
IL_0001: ldarg.1        
IL_0002: stfld          System.String Leqisoft.UI.Platform.frmAuxEdit::_commentValue
IL_0007: ldarg.0        
IL_0008: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput
IL_000D: ldarg.0        
IL_000E: call           System.String Leqisoft.UI.Platform.frmAuxEdit::get_CommentValue()
IL_0013: callvirt       System.Void System.Windows.Forms.Control::set_Text(System.String)
IL_0018: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::set_DefaultNotice(System.String) ---
HasBody=True RVA=0x00027B7E IL=4
IL_0000: ldarg.0        
IL_0001: ldarg.1        
IL_0002: stfld          System.String Leqisoft.UI.Platform.frmAuxEdit::_defaultNotice
IL_0007: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::set_DefaultValue(System.String) ---
HasBody=True RVA=0x00027BC2 IL=9
IL_0000: ldarg.0        
IL_0001: ldarg.1        
IL_0002: stfld          System.String Leqisoft.UI.Platform.frmAuxEdit::_defaultValue
IL_0007: ldarg.0        
IL_0008: ldfld          Leqisoft.UI.Controls.C1TextBoxEx Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput
IL_000D: ldarg.0        
IL_000E: call           System.String Leqisoft.UI.Platform.frmAuxEdit::get_DefaultValue()
IL_0013: callvirt       System.Void System.Windows.Forms.Control::set_Text(System.String)
IL_0018: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::set_Notice(System.String) ---
HasBody=True RVA=0x00027B6D IL=4
IL_0000: ldarg.0        
IL_0001: ldarg.1        
IL_0002: stfld          System.String Leqisoft.UI.Platform.frmAuxEdit::_dropNotice
IL_0007: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::set_QueryCommentValueChanged(System.Boolean) ---
HasBody=True RVA=0x00027C85 IL=4
IL_0000: ldarg.0        
IL_0001: ldarg.1        
IL_0002: stfld          System.Boolean Leqisoft.UI.Platform.frmAuxEdit::<QueryCommentValueChanged>k__BackingField
IL_0007: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::set_QueryDefaultValueChanged(System.Boolean) ---
HasBody=True RVA=0x00027C74 IL=4
IL_0000: ldarg.0        
IL_0001: ldarg.1        
IL_0002: stfld          System.Boolean Leqisoft.UI.Platform.frmAuxEdit::<QueryDefaultValueChanged>k__BackingField
IL_0007: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::set_QueryValueChanged(System.Boolean) ---
HasBody=True RVA=0x00027C63 IL=4
IL_0000: ldarg.0        
IL_0001: ldarg.1        
IL_0002: stfld          System.Boolean Leqisoft.UI.Platform.frmAuxEdit::<QueryValueChanged>k__BackingField
IL_0007: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::set_Value(System.String) ---
HasBody=True RVA=0x00027BA0 IL=9
IL_0000: ldarg.0        
IL_0001: ldarg.1        
IL_0002: stfld          System.String Leqisoft.UI.Platform.frmAuxEdit::_dropValue
IL_0007: ldarg.0        
IL_0008: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_000D: ldarg.0        
IL_000E: call           System.String Leqisoft.UI.Platform.frmAuxEdit::get_Value()
IL_0013: callvirt       System.Void System.Windows.Forms.Control::set_Text(System.String)
IL_0018: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::ShowFunctionHint(System.String,System.Int32) ---
HasBody=True RVA=0x00027E70 IL=113
Locals:
  [0] Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass86_0
  [1] Leqisoft.Model.FormulaDisplay
  [2] Leqisoft.Model.FunctionInfo
  [3] System.String
ExceptionHandlers:
  Catch try=IL_0000..IL_0143 handler=IL_0143..IL_0156
IL_0000: newobj         System.Void Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass86_0::.ctor()
IL_0005: stloc.0        
IL_0006: ldarg.1        
IL_0007: newobj         System.Void Leqisoft.Model.FormulaDisplay::.ctor(System.String)
IL_000C: stloc.1        
IL_000D: ldloc.0        
IL_000E: ldloc.1        
IL_000F: ldarg.2        
IL_0010: callvirt       System.Tuple`2<System.String,System.Int32> Leqisoft.Model.FormulaDisplay::GetFuncNameAtPos(System.Int32)
IL_0015: stfld          System.Tuple`2<System.String,System.Int32> Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass86_0::tup
IL_001A: ldloc.0        
IL_001B: ldfld          System.Tuple`2<System.String,System.Int32> Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass86_0::tup
IL_0020: callvirt       !0 System.Tuple`2<System.String,System.Int32>::get_Item1()
IL_0025: brtrue.s       IL_003c: call System.Collections.Generic.List`1<Leqisoft.Model.FunctionInfo> Leqisoft.Model.FunctionInfo::get_AllFunctionInfos()
IL_0027: ldarg.0        
IL_0028: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_002D: ldstr          ""
IL_0032: callvirt       System.Void C1.Win.C1Input.C1Label::set_Value(System.Object)
IL_0037: br             IL_0141: leave.s IL_0156
IL_003C: call           System.Collections.Generic.List`1<Leqisoft.Model.FunctionInfo> Leqisoft.Model.FunctionInfo::get_AllFunctionInfos()
IL_0041: ldloc.0        
IL_0042: ldftn          System.Boolean Leqisoft.UI.Platform.frmAuxEdit/<>c__DisplayClass86_0::<ShowFunctionHint>b__0(Leqisoft.Model.FunctionInfo)
IL_0048: newobj         System.Void System.Func`2<Leqisoft.Model.FunctionInfo,System.Boolean>::.ctor(System.Object,System.IntPtr)
IL_004D: call           !!0 System.Linq.Enumerable::FirstOrDefault<Leqisoft.Model.FunctionInfo>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,System.Boolean>)
IL_0052: stloc.2        
IL_0053: ldloc.2        
IL_0054: brfalse.s      IL_0065: ldarg.0
IL_0056: call           Leqisoft.UI.Platform.MainForm Leqisoft.UI.Platform.Program::get_MainForm()
IL_005B: ldloc.2        
IL_005C: ldc.i4.0       
IL_005D: ldc.i4.0       
IL_005E: callvirt       System.Boolean Leqisoft.UI.Platform.MainForm::IsAllowShowFunctionInfoAtCurrentView(Leqisoft.Model.FunctionInfo,System.Boolean,System.Boolean)
IL_0063: brtrue.s       IL_007a: ldc.i4.5
IL_0065: ldarg.0        
IL_0066: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_006B: ldstr          ""
IL_0070: callvirt       System.Void C1.Win.C1Input.C1Label::set_Value(System.Object)
IL_0075: br             IL_0141: leave.s IL_0156
IL_007A: ldc.i4.5       
IL_007B: newarr         System.String
IL_0080: dup            
IL_0081: ldc.i4.0       
IL_0082: ldstr          "函数语法："
IL_0087: stelem.ref     
IL_0088: dup            
IL_0089: ldc.i4.1       
IL_008A: ldloc.2        
IL_008B: callvirt       System.String Leqisoft.Model.FunctionInfo::get_Name()
IL_0090: stelem.ref     
IL_0091: dup            
IL_0092: ldc.i4.2       
IL_0093: ldstr          "("
IL_0098: stelem.ref     
IL_0099: dup            
IL_009A: ldc.i4.3       
IL_009B: ldstr          ", "
IL_00A0: ldloc.2        
IL_00A1: callvirt       System.Collections.Generic.List`1<Leqisoft.Model.ParameterInfo> Leqisoft.Model.FunctionInfo::get_Parameters()
IL_00A6: ldsfld         System.Func`2<Leqisoft.Model.ParameterInfo,System.String> Leqisoft.UI.Platform.frmAuxEdit/<>c::<>9__86_1
IL_00AB: dup            
IL_00AC: brtrue.s       IL_00c5: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<Leqisoft.Model.ParameterInfo,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
IL_00AE: pop            
IL_00AF: ldsfld         Leqisoft.UI.Platform.frmAuxEdit/<>c Leqisoft.UI.Platform.frmAuxEdit/<>c::<>9
IL_00B4: ldftn          System.String Leqisoft.UI.Platform.frmAuxEdit/<>c::<ShowFunctionHint>b__86_1(Leqisoft.Model.ParameterInfo)
IL_00BA: newobj         System.Void System.Func`2<Leqisoft.Model.ParameterInfo,System.String>::.ctor(System.Object,System.IntPtr)
IL_00BF: dup            
IL_00C0: stsfld         System.Func`2<Leqisoft.Model.ParameterInfo,System.String> Leqisoft.UI.Platform.frmAuxEdit/<>c::<>9__86_1
IL_00C5: call           System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<Leqisoft.Model.ParameterInfo,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
IL_00CA: call           System.String System.String::Join(System.String,System.Collections.Generic.IEnumerable`1<System.String>)
IL_00CF: stelem.ref     
IL_00D0: dup            
IL_00D1: ldc.i4.4       
IL_00D2: ldstr          ")\n"
IL_00D7: stelem.ref     
IL_00D8: call           System.String System.String::Concat(System.String[])
IL_00DD: stloc.3        
IL_00DE: ldloc.3        
IL_00DF: ldstr          "函数功能："
IL_00E4: ldloc.2        
IL_00E5: callvirt       System.String Leqisoft.Model.FunctionInfo::get_Description()
IL_00EA: ldstr          "\n"
IL_00EF: call           System.String System.String::Concat(System.String,System.String,System.String,System.String)
IL_00F4: stloc.3        
IL_00F5: ldloc.3        
IL_00F6: ldstr          "参数说明："
IL_00FB: ldstr          "；"
IL_0100: ldloc.2        
IL_0101: callvirt       System.Collections.Generic.List`1<Leqisoft.Model.ParameterInfo> Leqisoft.Model.FunctionInfo::get_Parameters()
IL_0106: ldsfld         System.Func`2<Leqisoft.Model.ParameterInfo,System.String> Leqisoft.UI.Platform.frmAuxEdit/<>c::<>9__86_2
IL_010B: dup            
IL_010C: brtrue.s       IL_0125: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<Leqisoft.Model.ParameterInfo,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
IL_010E: pop            
IL_010F: ldsfld         Leqisoft.UI.Platform.frmAuxEdit/<>c Leqisoft.UI.Platform.frmAuxEdit/<>c::<>9
IL_0114: ldftn          System.String Leqisoft.UI.Platform.frmAuxEdit/<>c::<ShowFunctionHint>b__86_2(Leqisoft.Model.ParameterInfo)
IL_011A: newobj         System.Void System.Func`2<Leqisoft.Model.ParameterInfo,System.String>::.ctor(System.Object,System.IntPtr)
IL_011F: dup            
IL_0120: stsfld         System.Func`2<Leqisoft.Model.ParameterInfo,System.String> Leqisoft.UI.Platform.frmAuxEdit/<>c::<>9__86_2
IL_0125: call           System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<Leqisoft.Model.ParameterInfo,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
IL_012A: call           System.String System.String::Join(System.String,System.Collections.Generic.IEnumerable`1<System.String>)
IL_012F: call           System.String System.String::Concat(System.String,System.String,System.String)
IL_0134: stloc.3        
IL_0135: ldarg.0        
IL_0136: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_013B: ldloc.3        
IL_013C: callvirt       System.Void C1.Win.C1Input.C1Label::set_Value(System.Object)
IL_0141: leave.s        IL_0156: ret
IL_0143: pop            
IL_0144: ldarg.0        
IL_0145: ldfld          C1.Win.C1Input.C1Label Leqisoft.UI.Platform.frmAuxEdit::lblFunctionHint
IL_014A: ldstr          ""
IL_014F: callvirt       System.Void C1.Win.C1Input.C1Label::set_Value(System.Object)
IL_0154: leave.s        IL_0156: ret
IL_0156: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::txbCommentInput_TextChanged(System.Object,System.EventArgs) ---
HasBody=True RVA=0x00028DE0 IL=4
IL_0000: ldarg.0        
IL_0001: ldc.i4.1       
IL_0002: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::set_QueryCommentValueChanged(System.Boolean)
IL_0007: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::txbDefaultInput_TextChanged(System.Object,System.EventArgs) ---
HasBody=True RVA=0x00028DE9 IL=4
IL_0000: ldarg.0        
IL_0001: ldc.i4.1       
IL_0002: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::set_QueryDefaultValueChanged(System.Boolean)
IL_0007: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::TxbDropInput_KeyDown(System.Object,System.Windows.Forms.KeyEventArgs) ---
HasBody=True RVA=0x000286CA IL=15
IL_0000: ldarg.2        
IL_0001: callvirt       System.Boolean System.Windows.Forms.KeyEventArgs::get_Control()
IL_0006: brfalse.s      IL_0029: ret
IL_0008: ldarg.2        
IL_0009: callvirt       System.Windows.Forms.Keys System.Windows.Forms.KeyEventArgs::get_KeyCode()
IL_000E: ldc.i4.s       86
IL_0010: bne.un.s       IL_0029: ret
IL_0012: ldarg.2        
IL_0013: ldc.i4.1       
IL_0014: callvirt       System.Void System.Windows.Forms.KeyEventArgs::set_Handled(System.Boolean)
IL_0019: ldarg.0        
IL_001A: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_001F: call           System.String System.Windows.Forms.Clipboard::GetText()
IL_0024: callvirt       System.Void System.Windows.Forms.TextBoxBase::set_SelectedText(System.String)
IL_0029: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::TxbDropInput_SelectionChanged(System.Object,System.EventArgs) ---
HasBody=True RVA=0x000286F5 IL=13
IL_0000: ldarg.0        
IL_0001: ldfld          System.Boolean Leqisoft.UI.Platform.frmAuxEdit::_isTextChanging
IL_0006: brfalse.s      IL_0009: ldarg.0
IL_0008: ret            
IL_0009: ldarg.0        
IL_000A: ldarg.0        
IL_000B: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0010: callvirt       System.String System.Windows.Forms.Control::get_Text()
IL_0015: ldarg.0        
IL_0016: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_001B: callvirt       System.Int32 System.Windows.Forms.TextBoxBase::get_SelectionStart()
IL_0020: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::ShowFunctionHint(System.String,System.Int32)
IL_0025: ret            

--- System.Void Leqisoft.UI.Platform.frmAuxEdit::txbDropInput_TextChanged(System.Object,System.EventArgs) ---
HasBody=True RVA=0x00028B74 IL=146
Locals:
  [0] System.Int32
  [1] System.Int32
  [2] System.Int32
  [3] Leqisoft.Model.FormulaDisplay
  [4] System.Tuple`2<System.Collections.Generic.List`1<Leqisoft.Model.FormulaDisplayRef>,System.Drawing.Color>
  [5] System.Collections.Generic.List`1/Enumerator<System.Tuple`3<System.Int32,System.Int32,System.Drawing.Color>>
  [6] System.Tuple`3<System.Int32,System.Int32,System.Drawing.Color>
  [7] System.Collections.Generic.List`1/Enumerator<Leqisoft.Model.FormulaDisplayRef>
  [8] Leqisoft.Model.FormulaDisplayRef
  [9] System.Collections.Generic.List`1/Enumerator<System.Tuple`2<System.Int32,System.Int32>>
  [10] System.Tuple`2<System.Int32,System.Int32>
ExceptionHandlers:
  Finally try=IL_0086..IL_00C7 handler=IL_00C7..IL_00D5
  Finally try=IL_0147..IL_0188 handler=IL_0188..IL_0196
  Finally try=IL_012E..IL_01A1 handler=IL_01A1..IL_01AF
  Catch try=IL_003D..IL_01B1 handler=IL_01B1..IL_01B4
  Finally try=IL_003D..IL_01B4 handler=IL_01B4..IL_01E0
IL_0000: ldarg.0        
IL_0001: ldc.i4.1       
IL_0002: stfld          System.Boolean Leqisoft.UI.Platform.frmAuxEdit::_isTextChanging
IL_0007: ldarg.0        
IL_0008: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_000D: call           System.Void Leqisoft.UI.Controls.ControlExtensions::SuspendDrawing(System.Windows.Forms.Control)
IL_0012: ldarg.0        
IL_0013: ldc.i4.1       
IL_0014: call           System.Void Leqisoft.UI.Platform.frmAuxEdit::set_QueryValueChanged(System.Boolean)
IL_0019: ldarg.0        
IL_001A: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_001F: callvirt       System.Int32 System.Windows.Forms.TextBoxBase::get_SelectionStart()
IL_0024: stloc.0        
IL_0025: ldarg.0        
IL_0026: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_002B: callvirt       System.Int32 System.Windows.Forms.TextBoxBase::get_SelectionLength()
IL_0030: stloc.1        
IL_0031: ldarg.0        
IL_0032: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0037: callvirt       System.Int32 System.Windows.Forms.RichTextBoxEx::get_VScrollPos()
IL_003C: stloc.2        
IL_003D: ldarg.0        
IL_003E: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0043: callvirt       System.String System.Windows.Forms.Control::get_Text()
IL_0048: newobj         System.Void Leqisoft.Model.FormulaDisplay::.ctor(System.String)
IL_004D: stloc.3        
IL_004E: ldarg.0        
IL_004F: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0054: callvirt       System.Void System.Windows.Forms.TextBoxBase::SelectAll()
IL_0059: ldarg.0        
IL_005A: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_005F: call           System.Drawing.Color System.Drawing.Color::get_Transparent()
IL_0064: callvirt       System.Void System.Windows.Forms.RichTextBox::set_SelectionBackColor(System.Drawing.Color)
IL_0069: ldarg.0        
IL_006A: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_006F: call           System.Drawing.Color System.Drawing.Color::get_Black()
IL_0074: callvirt       System.Void System.Windows.Forms.RichTextBox::set_SelectionColor(System.Drawing.Color)
IL_0079: ldloc.3        
IL_007A: callvirt       System.Collections.Generic.List`1<System.Tuple`3<System.Int32,System.Int32,System.Drawing.Color>> Leqisoft.Model.FormulaDisplay::GetTokenColorIntervals()
IL_007F: callvirt       System.Collections.Generic.List`1/Enumerator<!0> System.Collections.Generic.List`1<System.Tuple`3<System.Int32,System.Int32,System.Drawing.Color>>::GetEnumerator()
IL_0084: stloc.s        V_5
IL_0086: br.s           IL_00bc: ldloca.s V_5
IL_0088: ldloca.s       V_5
IL_008A: call           !0 System.Collections.Generic.List`1/Enumerator<System.Tuple`3<System.Int32,System.Int32,System.Drawing.Color>>::get_Current()
IL_008F: stloc.s        V_6
IL_0091: ldarg.0        
IL_0092: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0097: ldloc.s        V_6
IL_0099: callvirt       !0 System.Tuple`3<System.Int32,System.Int32,System.Drawing.Color>::get_Item1()
IL_009E: ldloc.s        V_6
IL_00A0: callvirt       !1 System.Tuple`3<System.Int32,System.Int32,System.Drawing.Color>::get_Item2()
IL_00A5: callvirt       System.Void System.Windows.Forms.TextBoxBase::Select(System.Int32,System.Int32)
IL_00AA: ldarg.0        
IL_00AB: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_00B0: ldloc.s        V_6
IL_00B2: callvirt       !2 System.Tuple`3<System.Int32,System.Int32,System.Drawing.Color>::get_Item3()
IL_00B7: callvirt       System.Void System.Windows.Forms.RichTextBox::set_SelectionColor(System.Drawing.Color)
IL_00BC: ldloca.s       V_5
IL_00BE: call           System.Boolean System.Collections.Generic.List`1/Enumerator<System.Tuple`3<System.Int32,System.Int32,System.Drawing.Color>>::MoveNext()
IL_00C3: brtrue.s       IL_0088: ldloca.s V_5
IL_00C5: leave.s        IL_00d5: ldloc.3
IL_00C7: ldloca.s       V_5
IL_00C9: constrained.   System.Collections.Generic.List`1/Enumerator<System.Tuple`3<System.Int32,System.Int32,System.Drawing.Color>>
IL_00CF: callvirt       System.Void System.IDisposable::Dispose()
IL_00D4: endfinally     
IL_00D5: ldloc.3        
IL_00D6: call           Leqisoft.UI.Platform.MainForm Leqisoft.UI.Platform.Program::get_MainForm()
IL_00DB: callvirt       Leqisoft.UI.Platform.FormulaEditor Leqisoft.UI.Platform.MainForm::get_FormulaEditor()
IL_00E0: callvirt       Leqisoft.Model.FormulaContext Leqisoft.UI.Platform.FormulaEditor::get_Context()
IL_00E5: callvirt       System.Tuple`2<System.Collections.Generic.List`1<Leqisoft.Model.FormulaDisplayRef>,System.Drawing.Color> Leqisoft.Model.FormulaDisplay::GetReferences(Leqisoft.Model.FormulaContext)
IL_00EA: stloc.s        V_4
IL_00EC: call           Leqisoft.UI.Platform.MainForm Leqisoft.UI.Platform.Program::get_MainForm()
IL_00F1: callvirt       Leqisoft.UI.Platform.FormulaEditor Leqisoft.UI.Platform.MainForm::get_FormulaEditor()
IL_00F6: ldloc.s        V_4
IL_00F8: callvirt       !0 System.Tuple`2<System.Collections.Generic.List`1<Leqisoft.Model.FormulaDisplayRef>,System.Drawing.Color>::get_Item1()
IL_00FD: callvirt       System.Void Leqisoft.UI.Platform.FormulaEditor::set_RefIntervals(System.Collections.Generic.List`1<Leqisoft.Model.FormulaDisplayRef>)
IL_0102: call           Leqisoft.UI.Platform.MainForm Leqisoft.UI.Platform.Program::get_MainForm()
IL_0107: callvirt       Leqisoft.UI.Platform.FormulaEditor Leqisoft.UI.Platform.MainForm::get_FormulaEditor()
IL_010C: ldloc.s        V_4
IL_010E: callvirt       !1 System.Tuple`2<System.Collections.Generic.List`1<Leqisoft.Model.FormulaDisplayRef>,System.Drawing.Color>::get_Item2()
IL_0113: callvirt       System.Void Leqisoft.UI.Platform.FormulaEditor::set_NextColor(System.Drawing.Color)
IL_0118: call           Leqisoft.UI.Platform.MainForm Leqisoft.UI.Platform.Program::get_MainForm()
IL_011D: callvirt       Leqisoft.UI.Platform.FormulaEditor Leqisoft.UI.Platform.MainForm::get_FormulaEditor()
IL_0122: callvirt       System.Collections.Generic.List`1<Leqisoft.Model.FormulaDisplayRef> Leqisoft.UI.Platform.FormulaEditor::get_RefIntervals()
IL_0127: callvirt       System.Collections.Generic.List`1/Enumerator<!0> System.Collections.Generic.List`1<Leqisoft.Model.FormulaDisplayRef>::GetEnumerator()
IL_012C: stloc.s        V_7
IL_012E: br.s           IL_0196: ldloca.s V_7
IL_0130: ldloca.s       V_7
IL_0132: call           !0 System.Collections.Generic.List`1/Enumerator<Leqisoft.Model.FormulaDisplayRef>::get_Current()
IL_0137: stloc.s        V_8
IL_0139: ldloc.s        V_8
IL_013B: callvirt       System.Collections.Generic.List`1<System.Tuple`2<System.Int32,System.Int32>> Leqisoft.Model.FormulaDisplayRef::get_Intervals()
IL_0140: callvirt       System.Collections.Generic.List`1/Enumerator<!0> System.Collections.Generic.List`1<System.Tuple`2<System.Int32,System.Int32>>::GetEnumerator()
IL_0145: stloc.s        V_9
IL_0147: br.s           IL_017d: ldloca.s V_9
IL_0149: ldloca.s       V_9
IL_014B: call           !0 System.Collections.Generic.List`1/Enumerator<System.Tuple`2<System.Int32,System.Int32>>::get_Current()
IL_0150: stloc.s        V_10
IL_0152: ldarg.0        
IL_0153: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0158: ldloc.s        V_10
IL_015A: callvirt       !0 System.Tuple`2<System.Int32,System.Int32>::get_Item1()
IL_015F: ldloc.s        V_10
IL_0161: callvirt       !1 System.Tuple`2<System.Int32,System.Int32>::get_Item2()
IL_0166: callvirt       System.Void System.Windows.Forms.TextBoxBase::Select(System.Int32,System.Int32)
IL_016B: ldarg.0        
IL_016C: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0171: ldloc.s        V_8
IL_0173: callvirt       System.Drawing.Color Leqisoft.Model.FormulaDisplayRef::get_Color()
IL_0178: callvirt       System.Void System.Windows.Forms.RichTextBox::set_SelectionColor(System.Drawing.Color)
IL_017D: ldloca.s       V_9
IL_017F: call           System.Boolean System.Collections.Generic.List`1/Enumerator<System.Tuple`2<System.Int32,System.Int32>>::MoveNext()
IL_0184: brtrue.s       IL_0149: ldloca.s V_9
IL_0186: leave.s        IL_0196: ldloca.s V_7
IL_0188: ldloca.s       V_9
IL_018A: constrained.   System.Collections.Generic.List`1/Enumerator<System.Tuple`2<System.Int32,System.Int32>>
IL_0190: callvirt       System.Void System.IDisposable::Dispose()
IL_0195: endfinally     
IL_0196: ldloca.s       V_7
IL_0198: call           System.Boolean System.Collections.Generic.List`1/Enumerator<Leqisoft.Model.FormulaDisplayRef>::MoveNext()
IL_019D: brtrue.s       IL_0130: ldloca.s V_7
IL_019F: leave.s        IL_01af: leave.s IL_01e0
IL_01A1: ldloca.s       V_7
IL_01A3: constrained.   System.Collections.Generic.List`1/Enumerator<Leqisoft.Model.FormulaDisplayRef>
IL_01A9: callvirt       System.Void System.IDisposable::Dispose()
IL_01AE: endfinally     
IL_01AF: leave.s        IL_01e0: ret
IL_01B1: pop            
IL_01B2: leave.s        IL_01e0: ret
IL_01B4: ldarg.0        
IL_01B5: ldc.i4.0       
IL_01B6: stfld          System.Boolean Leqisoft.UI.Platform.frmAuxEdit::_isTextChanging
IL_01BB: ldarg.0        
IL_01BC: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_01C1: ldloc.0        
IL_01C2: ldloc.1        
IL_01C3: callvirt       System.Void System.Windows.Forms.TextBoxBase::Select(System.Int32,System.Int32)
IL_01C8: ldarg.0        
IL_01C9: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_01CE: ldloc.2        
IL_01CF: callvirt       System.Void System.Windows.Forms.RichTextBoxEx::set_VScrollPos(System.Int32)
IL_01D4: ldarg.0        
IL_01D5: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_01DA: call           System.Void Leqisoft.UI.Controls.ControlExtensions::ResumeDrawing(System.Windows.Forms.Control)
IL_01DF: endfinally     
IL_01E0: ret            

--- System.Boolean Leqisoft.UI.Platform.frmAuxEdit::UseWildcard() ---
HasBody=True RVA=0x00027FF0 IL=8
IL_0000: ldarg.0        
IL_0001: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0006: callvirt       System.String System.Windows.Forms.Control::get_Text()
IL_000B: ldarg.0        
IL_000C: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0011: callvirt       System.Int32 System.Windows.Forms.TextBoxBase::get_SelectionStart()
IL_0016: call           System.Boolean Leqisoft.UI.Platform.frmAuxEdit::UseWildcardImpl(System.String,System.Int32)
IL_001B: ret            

--- System.Boolean Leqisoft.UI.Platform.frmAuxEdit::UseWildcardImpl(System.String,System.Int32) ---
HasBody=True RVA=0x0002903C IL=69
Locals:
  [0] System.Char
  [1] System.Boolean
  [2] Leqisoft.Model.FormulaDisplay
  [3] System.Tuple`2<System.String,System.Int32>
  [4] System.Boolean
ExceptionHandlers:
  Catch try=IL_0028..IL_007B handler=IL_007B..IL_0081
IL_0000: ldarg.1        
IL_0001: brtrue.s       IL_0005: ldarg.0
IL_0003: ldc.i4.0       
IL_0004: ret            
IL_0005: ldarg.0        
IL_0006: ldarg.1        
IL_0007: ldc.i4.1       
IL_0008: sub            
IL_0009: callvirt       System.Char System.String::get_Chars(System.Int32)
IL_000E: stloc.0        
IL_000F: ldloc.0        
IL_0010: ldc.i4.s       61
IL_0012: beq.s          IL_0020: ldc.i4.1
IL_0014: ldloc.0        
IL_0015: ldc.i4.s       62
IL_0017: beq.s          IL_0020: ldc.i4.1
IL_0019: ldloc.0        
IL_001A: ldc.i4.s       60
IL_001C: ceq            
IL_001E: br.s           IL_0021: stloc.1
IL_0020: ldc.i4.1       
IL_0021: stloc.1        
IL_0022: ldloc.1        
IL_0023: brfalse.s      IL_0027: nop
IL_0025: ldc.i4.1       
IL_0026: ret            
IL_0027: nop            
IL_0028: ldarg.0        
IL_0029: newobj         System.Void Leqisoft.Model.FormulaDisplay::.ctor(System.String)
IL_002E: stloc.2        
IL_002F: ldloc.2        
IL_0030: ldarg.1        
IL_0031: callvirt       System.Tuple`2<System.String,System.Int32> Leqisoft.Model.FormulaDisplay::GetFuncNameAtPos(System.Int32)
IL_0036: stloc.3        
IL_0037: ldstr          "Select"
IL_003C: ldloc.3        
IL_003D: callvirt       !0 System.Tuple`2<System.String,System.Int32>::get_Item1()
IL_0042: ldc.i4.5       
IL_0043: call           System.Boolean System.String::Equals(System.String,System.StringComparison)
IL_0048: brfalse.s      IL_0059: ldstr "If"
IL_004A: ldloc.3        
IL_004B: callvirt       !1 System.Tuple`2<System.String,System.Int32>::get_Item2()
IL_0050: ldc.i4.2       
IL_0051: rem            
IL_0052: brtrue.s       IL_0059: ldstr "If"
IL_0054: ldc.i4.1       
IL_0055: stloc.s        V_4
IL_0057: leave.s        IL_0083: ldloc.s V_4
IL_0059: ldstr          "If"
IL_005E: ldloc.3        
IL_005F: callvirt       !0 System.Tuple`2<System.String,System.Int32>::get_Item1()
IL_0064: ldc.i4.5       
IL_0065: call           System.Boolean System.String::Equals(System.String,System.StringComparison)
IL_006A: brfalse.s      IL_0079: leave.s IL_0081
IL_006C: ldloc.3        
IL_006D: callvirt       !1 System.Tuple`2<System.String,System.Int32>::get_Item2()
IL_0072: brtrue.s       IL_0079: leave.s IL_0081
IL_0074: ldc.i4.1       
IL_0075: stloc.s        V_4
IL_0077: leave.s        IL_0083: ldloc.s V_4
IL_0079: leave.s        IL_0081: ldc.i4.0
IL_007B: pop            
IL_007C: ldc.i4.0       
IL_007D: stloc.s        V_4
IL_007F: leave.s        IL_0083: ldloc.s V_4
IL_0081: ldc.i4.0       
IL_0082: ret            
IL_0083: ldloc.s        V_4
IL_0085: ret            

--- System.Boolean Leqisoft.UI.Platform.frmAuxEdit::ValidateFormula(System.String&) ---
HasBody=True RVA=0x00028E7C IL=133
Locals:
  [0] Leqisoft.Model.FormulaDisplay
  [1] System.Boolean
  [2] Leqisoft.Model.FormulaSyntaxException
  [3] Leqisoft.Model.FormulaNotApplicableException
  [4] Leqisoft.Model.FormulaBadValueException
ExceptionHandlers:
  Catch try=IL_001C..IL_004B handler=IL_004B..IL_0066
  Catch try=IL_001C..IL_004B handler=IL_0066..IL_0081
  Catch try=IL_001C..IL_004B handler=IL_0081..IL_00C5
  Catch try=IL_001C..IL_004B handler=IL_00C5..IL_00DD
  Catch try=IL_001C..IL_004B handler=IL_00DD..IL_00F6
  Catch try=IL_001C..IL_004B handler=IL_00F6..IL_011B
  Catch try=IL_001C..IL_004B handler=IL_011B..IL_0133
  Catch try=IL_001C..IL_004B handler=IL_0133..IL_014B
IL_0000: ldarg.1        
IL_0001: ldsfld         System.String System.String::Empty
IL_0006: stind.ref      
IL_0007: ldarg.0        
IL_0008: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_000D: callvirt       System.String System.Windows.Forms.Control::get_Text()
IL_0012: call           System.Boolean System.String::IsNullOrWhiteSpace(System.String)
IL_0017: brfalse.s      IL_001b: nop
IL_0019: ldc.i4.1       
IL_001A: ret            
IL_001B: nop            
IL_001C: ldarg.0        
IL_001D: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_0022: callvirt       System.String System.Windows.Forms.Control::get_Text()
IL_0027: newobj         System.Void Leqisoft.Model.FormulaDisplay::.ctor(System.String)
IL_002C: stloc.0        
IL_002D: ldarg.1        
IL_002E: ldloc.0        
IL_002F: call           Leqisoft.UI.Platform.MainForm Leqisoft.UI.Platform.Program::get_MainForm()
IL_0034: callvirt       Leqisoft.UI.Platform.FormulaEditor Leqisoft.UI.Platform.MainForm::get_FormulaEditor()
IL_0039: callvirt       Leqisoft.Model.FormulaContext Leqisoft.UI.Platform.FormulaEditor::get_Context()
IL_003E: callvirt       System.String Leqisoft.Model.FormulaDisplay::ToFormula(Leqisoft.Model.FormulaContext)
IL_0043: stind.ref      
IL_0044: ldc.i4.1       
IL_0045: stloc.1        
IL_0046: leave          IL_014b: ldloc.1
IL_004B: pop            
IL_004C: ldc.i4.0       
IL_004D: ldstr          "函数参数数量错误"
IL_0052: ldc.i4.0       
IL_0053: ldstr          ""
IL_0058: ldc.i4.0       
IL_0059: call           System.Windows.Forms.DialogResult Leqisoft.UI.Controls.MessageBox::Show(System.Windows.Forms.MessageBoxIcon,System.String,System.Windows.Forms.MessageBoxButtons,System.String,System.Boolean)
IL_005E: pop            
IL_005F: ldc.i4.0       
IL_0060: stloc.1        
IL_0061: leave          IL_014b: ldloc.1
IL_0066: pop            
IL_0067: ldc.i4.0       
IL_0068: ldstr          "函数不存在"
IL_006D: ldc.i4.0       
IL_006E: ldstr          ""
IL_0073: ldc.i4.0       
IL_0074: call           System.Windows.Forms.DialogResult Leqisoft.UI.Controls.MessageBox::Show(System.Windows.Forms.MessageBoxIcon,System.String,System.Windows.Forms.MessageBoxButtons,System.String,System.Boolean)
IL_0079: pop            
IL_007A: ldc.i4.0       
IL_007B: stloc.1        
IL_007C: leave          IL_014b: ldloc.1
IL_0081: stloc.2        
IL_0082: ldc.i4.0       
IL_0083: ldstr          "语法错误。\n"
IL_0088: ldloc.2        
IL_0089: callvirt       System.String System.Exception::get_Message()
IL_008E: call           System.String System.String::Concat(System.String,System.String)
IL_0093: ldc.i4.0       
IL_0094: ldstr          ""
IL_0099: ldc.i4.0       
IL_009A: call           System.Windows.Forms.DialogResult Leqisoft.UI.Controls.MessageBox::Show(System.Windows.Forms.MessageBoxIcon,System.String,System.Windows.Forms.MessageBoxButtons,System.String,System.Boolean)
IL_009F: pop            
IL_00A0: ldarg.0        
IL_00A1: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_00A6: ldloc.2        
IL_00A7: callvirt       System.Int32 Leqisoft.Model.FormulaSyntaxException::get_CharPosition()
IL_00AC: ldc.i4.0       
IL_00AD: callvirt       System.Void System.Windows.Forms.TextBoxBase::Select(System.Int32,System.Int32)
IL_00B2: ldarg.0        
IL_00B3: ldfld          System.Windows.Forms.RichTextBoxEx Leqisoft.UI.Platform.frmAuxEdit::rtbDropInput
IL_00B8: callvirt       System.Boolean System.Windows.Forms.Control::Focus()
IL_00BD: pop            
IL_00BE: ldc.i4.0       
IL_00BF: stloc.1        
IL_00C0: leave          IL_014b: ldloc.1
IL_00C5: pop            
IL_00C6: ldc.i4.0       
IL_00C7: ldstr          "引用不存在"
IL_00CC: ldc.i4.0       
IL_00CD: ldstr          ""
IL_00D2: ldc.i4.0       
IL_00D3: call           System.Windows.Forms.DialogResult Leqisoft.UI.Controls.MessageBox::Show(System.Windows.Forms.MessageBoxIcon,System.String,System.Windows.Forms.MessageBoxButtons,System.String,System.Boolean)
IL_00D8: pop            
IL_00D9: ldc.i4.0       
IL_00DA: stloc.1        
IL_00DB: leave.s        IL_014b: ldloc.1
IL_00DD: stloc.3        
IL_00DE: ldc.i4.0       
IL_00DF: ldloc.3        
IL_00E0: callvirt       System.String System.Exception::get_Message()
IL_00E5: ldc.i4.0       
IL_00E6: ldstr          ""
IL_00EB: ldc.i4.0       
IL_00EC: call           System.Windows.Forms.DialogResult Leqisoft.UI.Controls.MessageBox::Show(System.Windows.Forms.MessageBoxIcon,System.String,System.Windows.Forms.MessageBoxButtons,System.String,System.Boolean)
IL_00F1: pop            
IL_00F2: ldc.i4.0       
IL_00F3: stloc.1        
IL_00F4: leave.s        IL_014b: ldloc.1
IL_00F6: stloc.s        V_4
IL_00F8: ldc.i4.0       
IL_00F9: ldstr          "值范围错误\n"
IL_00FE: ldloc.s        V_4
IL_0100: callvirt       System.String System.Exception::get_Message()
IL_0105: call           System.String System.String::Concat(System.String,System.String)
IL_010A: ldc.i4.0       
IL_010B: ldstr          ""
IL_0110: ldc.i4.0       
IL_0111: call           System.Windows.Forms.DialogResult Leqisoft.UI.Controls.MessageBox::Show(System.Windows.Forms.MessageBoxIcon,System.String,System.Windows.Forms.MessageBoxButtons,System.String,System.Boolean)
IL_0116: pop            
IL_0117: ldc.i4.0       
IL_0118: stloc.1        
IL_0119: leave.s        IL_014b: ldloc.1
IL_011B: pop            
IL_011C: ldc.i4.0       
IL_011D: ldstr          "参数类型错误"
IL_0122: ldc.i4.0       
IL_0123: ldstr          ""
IL_0128: ldc.i4.0       
IL_0129: call           System.Windows.Forms.DialogResult Leqisoft.UI.Controls.MessageBox::Show(System.Windows.Forms.MessageBoxIcon,System.String,System.Windows.Forms.MessageBoxButtons,System.String,System.Boolean)
IL_012E: pop            
IL_012F: ldc.i4.0       
IL_0130: stloc.1        
IL_0131: leave.s        IL_014b: ldloc.1
IL_0133: pop            
IL_0134: ldc.i4.0       
IL_0135: ldstr          "公式引用的列没有本表对应的行"
IL_013A: ldc.i4.0       
IL_013B: ldstr          ""
IL_0140: ldc.i4.0       
IL_0141: call           System.Windows.Forms.DialogResult Leqisoft.UI.Controls.MessageBox::Show(System.Windows.Forms.MessageBoxIcon,System.String,System.Windows.Forms.MessageBoxButtons,System.String,System.Boolean)
IL_0146: pop            
IL_0147: ldc.i4.0       
IL_0148: stloc.1        
IL_0149: leave.s        IL_014b: ldloc.1
IL_014B: ldloc.1        
IL_014C: ret            

