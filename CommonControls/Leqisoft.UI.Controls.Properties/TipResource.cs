﻿using Leqisoft.Model;

namespace Leqisoft.UI.Controls.Properties;

public static class TipResource
{
	public static string Project管理窗体 = "[|]";

	public static string 新建Project按钮 = "新建" + StringConstBase.Current.Project + "功能说明[|]1、新建" + StringConstBase.Current.Project + "用于创建一个新" + StringConstBase.Current.Project + "，您创建的" + StringConstBase.Current.Project + "只有您具备删除的权限。\r\n2、创建" + StringConstBase.Current.Project + "时，您可以选择一个适合的" + StringConstBase.Current.Project + "模板，模板中的表格及文档将自动带入到您的新建" + StringConstBase.Current.Project + "中。\r\n3、创建" + StringConstBase.Current.Project + "时，您还可以选择您的同事参于到您的" + StringConstBase.Current.Project + "中来，进入一个多人协同作业的全新工作环境，前提是您需要点按“同事管理”创建或加入一个组织组织。";

	public static string 打开Project按钮 = "打开" + StringConstBase.Current.Project + "功能说明[|]您创建的" + StringConstBase.Current.Project + "或参与的" + StringConstBase.Current.Project + "，会显示在" + StringConstBase.Current.Project + "列表中，您有权打开" + StringConstBase.Current.Project + "并对" + StringConstBase.Current.Project + "中的表格及文档进行编辑，您未参与的" + StringConstBase.Current.Project + "，" + StringConstBase.Current.Project + "列表中不会显示。";

	public static string 修改Project按钮 = "修改" + StringConstBase.Current.Project + "功能说明[|]您可以对" + StringConstBase.Current.Project + "的编号、名称、" + StringConstBase.Current.Auditee + "、组织成员等信息进行二次修改。";

	public static string 删除Project按钮 = "删除" + StringConstBase.Current.Project + "功能说明[|]您可以删除您创建的" + StringConstBase.Current.Project + "，" + StringConstBase.Current.Project + "删除后，将无法恢复，因此建议您谨慎操作。";

	public static string 复制Project按钮 = "复制" + StringConstBase.Current.Project + "功能说明[|]当您觉得两个" + StringConstBase.Current.Project + "内容相近时，可以基于一个" + StringConstBase.Current.Project + "复制生成另一个新" + StringConstBase.Current.Project + "。例如：对同一个公司连续年度审计时，您可以直接对原" + StringConstBase.Current.Project + "进行复制，直接调用原来的年末数据改为年初数据。";

	public static string Project导出按钮 = StringConstBase.Current.Project + "导出功能说明[|]您可以将" + StringConstBase.Current.Project + "内容导出至本机存储，" + StringConstBase.Current.Project + "中的云表格会导出为Excel格式，云文档会导出为Word格式，组和文件夹会导出为文件夹。";

	public static string 另存模板按钮 = "另存模板功能说明[|]当某一类别的" + StringConstBase.Current.Project + "您完成后觉得很完善，可另存为模板，下次遇到同类新" + StringConstBase.Current.Project + "可直接基于模板创建。";

	public static string 模板管理按钮 = "模板管理功能说明[|]点击进入模板管理状态，可以新建、修改和删除模板。您可以将不同类型" + StringConstBase.Current.Project + "做成模板以供创建" + StringConstBase.Current.Project + "时直接调用。";

	public static string Project管理窗体_同事管理按钮 = "同事管理功能说明[|]1、您可以创建且仅能创建一个组织，因此创建一个组织一般由公司的系统管理员或职级较高的管理层人员创建。若您不是公司管理员，建议不要创建为好，公司管理员会将您加入到组织中。\r\n2、当您被加入一个组织后，就不能再被加入其他组织里，除非您选择退出原组织。";

	public static string 模板管理窗体 = "[|]";

	public static string 新建模板按钮 = "新建模板功能说明[|]1、新建模板用于创建一个新的空白模板，您创建的模板只有您具备删除的权限。\r\n2、创建模板时，您还可以选择您的同事参于到您的模板制作中来，前提是您需要在" + StringConstBase.Current.Project + "管理界面点按“同事管理”按钮创建或加入一个组织。";

	public static string 修改模板按钮 = "修改模板功能说明[|]您可以对" + StringConstBase.Current.Project + "的编号、名称、类别、模板成员、备注等信息进行二次修改。";

	public static string 删除模板按钮 = "删除模板功能说明[|]您可以将您创建的不需用的模板进行删除，删除后将无法恢复，因此建议您谨慎操作。";

	public static string 复制模板按钮 = "复制模板功能说明[|]当您觉得两个模板内容相近时，复制一下原模板进行部分内容的修改就可以快速生成一个新的模板。";

	public static string 新建Project窗体 = "[|]";

	public static string 新建Project窗体_窗体提示 = "[|]带“*”为必填项，请根据工作内容和工作习惯进行填写，否则无法完成" + StringConstBase.Current.Project + "建立。";

	public static string 上级Project = "上级" + StringConstBase.Current.Project + "选择说明[|]1、如果您的" + StringConstBase.Current.Project + "是子公司，请在这里选择母公司。\r\n2、如果母公司的" + StringConstBase.Current.Project + "尚未创建，您需要先创建母公司的" + StringConstBase.Current.Project + "再进行选择，也可现在不选上级" + StringConstBase.Current.Project + "，待母公司" + StringConstBase.Current.Project + "创建后，再在此设置上级" + StringConstBase.Current.Project + "。\r\n3、选择上级" + StringConstBase.Current.Project + "的意义在于，母公司可根据下级" + StringConstBase.Current.Project + "设置合并报表范围，之后配置合并报表参数后，可一键快速生成合并报表。";

	public static string 使用模板 = "使用模板注意事项[|]您可以在这里选择适合" + StringConstBase.Current.Project + "类型的模板，模板中的文档及表格将自动带入到您的新创" + StringConstBase.Current.Project + "中，如果您不选择模板，将会创建出一个无任何表格及文档的空白" + StringConstBase.Current.Project + "。";

	public static string Project成员 = StringConstBase.Current.Project + "成员添加说明[|]1、此处可为该" + StringConstBase.Current.Project + "添加组织成员，被添加的成员不但会在" + StringConstBase.Current.Project + "列表中看到您创建的该" + StringConstBase.Current.Project + "，且有权打开" + StringConstBase.Current.Project + "进行编辑作业。\r\n2、点击“用户”可出现下拉框，选择出现的组织成员进行添加，如果您想要添加的成员并未出现在下拉列表中，请先添加其进入团；\r\n3、点击“角色”下拉框可以选择该成员在此" + StringConstBase.Current.Project + "中所担当的职位或角色，具体包括：“" + StringConstBase.Current.Manager + "”、“" + StringConstBase.Current.Assistant + "”和“复核人”。\r\n4、右键可以新增或者删除该" + StringConstBase.Current.Project + "的参与成员。";

	public static string 新建Project窗体_所有成员可见 = "所有成员可见的作用[|]勾选后组织所有成员均可看到该" + StringConstBase.Current.Project + "，且均有进入" + StringConstBase.Current.Project + "并参与" + StringConstBase.Current.Project + "的编辑的权限。";

	public static string 新建模板窗体 = "[|]";

	public static string 新建模板窗体_窗体提示 = "[|]带“*”号为必填项，按照模板内容进行填写";

	public static string 模板成员 = "模板成员添加说明[|]1、此处可为该模板添加组织成员，被添加的成员会在模板列表中看到您创建的该模板。\r\n2、点击“用户”可出现下拉框，选择出现的组织成员进行添加，如果您想要添加的成员并未出现在下拉列表中，请先添加其进入组织中；\r\n3、点击“角色”下拉框可以指定该成员对模板的操作权限，其中：“可编辑的用户”具备打开模板并编辑操作的权限，“可使用的用户”具备看到此模板且可基于此模板创建" + StringConstBase.Current.Project + "的权限。\r\n4、右键可以新增或者删除该模板的参与成员。";

	public static string 新建模板窗体_所有成员可见 = "所有成员可见的作用[|]勾选后所有成员均可看到此模板，并可基于此模板创建" + StringConstBase.Current.Project + "。";

	public static string 同事管理窗体 = "[|]";

	public static string 同事管理界面 = "同事管理功能说明[|]1、若您是组织管理员，只有您具备新增或删除组织成员的权限，若要解散组织则需要将包括您在内的全部成员删除后，组织即会解散。\r\n2、若您仅是组织成员，您可以查看到所有成员列表，但无组织成员增减维护的权限；您具备自身退出组织的权限，但在实务工作中，退出组织的操作并不常用。";

	public static string Ribbon菜单 = "[|]";

	public static string Project管理菜单 = "[|]";

	public static string Project管理按钮 = StringConstBase.Current.Project + "管理功能说明[|]您可以打开" + StringConstBase.Current.Project + "管理窗体进行创建" + StringConstBase.Current.Project + "、删除" + StringConstBase.Current.Project + "、修改" + StringConstBase.Current.Project + "信息、切换" + StringConstBase.Current.Project + "等操作。";

	public static string Ribbon菜单_Project管理菜单_同步Project按钮 = "同步" + StringConstBase.Current.Project + "功能说明[|]同步" + StringConstBase.Current.Project + "是指使本机数据与云服务器数据保持一致性的下载和上传操作，是实现多人协同作业的实现方法，例如“用户A”在编辑完成一部分工作后，通过同步可将工作上传至云服务器，而“用户B”执行同步操作后，就可以下载到“用户A”编辑的那部分工作至其本地" + StringConstBase.Current.Project + "，从而实现协同作业，简单概括为：同步操作可以确保本机的数据始终是最新的。";

	public static string Ribbon菜单_Project管理菜单_同事管理按钮 = "同事管理功能说明[|]1、您可以创建且仅能创建一个组织，因此创建一个组织一般由公司的系统管理员或职级较高的管理层人员创建。若您不是公司管理员，建议不要创建为好，公司管理员会将您加入到组织组织中。\r\n2、当您被加入一个组织后，就不能再被加入其他组织里，除非您选择退出原组织。";

	public static string 用户资料按钮 = "用户资料修改[|]在这里您可以修改您的个人信息，如：修改您的用户名、姓名、性别、邮箱等，但用户名无法进行修改，建议您绑定您的手机号，以提升您账号的安全性。";

	public static string 密码修改按钮 = "密码的修改[|]在这里您可以修改您的用户登录密码，需要特别提示的是，为保障您账号的安全性，您应该将您的账号绑定到您的手机，这样就可以随时通过您的手机重置您的账号登录密码。";

	public static string Project管理_权限控制 = "权限控制功能说明[|]权限控制用于" + StringConstBase.Current.Manager + "分配" + StringConstBase.Current.Project + "中各文件的操作权限，包括查看权限、编辑权限和删除权限，便于划分和控制成员的职责分工。除此之外，在表格的右键菜单中，" + StringConstBase.Current.Manager + "还可分配表格行列的编辑权限。";

	public static string 历史版本功能说明 = "历史版本功能说明[|]您对云表格或云文档的历次保存，软件都会记录这些历史版本数据，在您需要时，可追溯回滚某时点数据，这意味着您对文件的编辑操作发生失误时，您可以轻松还原误操作前的历史数据。";

	public static string 回收文件功能说明 = "回收文件功能说明[|]您过往删除过的云表格或云文档，软件都会自动记录这些删除过的数据，在您需要时，您可以回收恢复这些删除过的文件，这看起来极象windows的回收站。";

	public static string 财务数据菜单 = "[|]";

	public static string 采集数据按钮 = "采集财务数据说明[|]软件支持多种主流财务软件的数据采集，您可以直接点击此按钮运行采数器进行网络采集，也可以点击“生成U盘采数器”按钮，生成U盘采数器后，再将带有采数器的U盘插入到客户机上进行采集。";

	public static string 生成U盘采数器按钮 = "生成U盘采数器说明[|]1、更多时候，您需要点击此按钮将采数器程序自动拷贝至您的U盘，U盘会生成“AuditAI采数器”文件夹，文件夹中可运行“AuditAI采数器.exe”程序进行采数。\r\n2、在用U盘采集财务数据时，您可以将U盘直接插入到财务数据服务器进行本地采集，也可以将U盘插入到可以连接财务数据服务器的电脑进行远程网络采集。";

	public static string 打开账套按钮 = "打开账套功能说明[|]1、用于打开采集的账套文件，账套文件名一般为：X公司_X年度.db。\r\n2、" + StringConstBase.Current.Project + "中的智能生成底稿功能需要打开账套方能执行。";

	public static string 打开序时账按钮 = "打开序时账功能说明[|]用于打开Excel格式的序时账，Excel序时账包含年初余额表和会计凭证库，但必须加工成软件识别的样式要求。";

	public static string 合并账套按钮 = "合并账套功能说明[|]用于将若干单年度账套合并生成连续年度账套，以方便跨年度查询财务数据。";

	public static string 我的关注按钮 = "我的关注功能说明[|]在查询明细账或会计凭证时，可右键对凭证条目标记成关注状态，此按钮用于集中查询被标记关注的凭证条目。";

	public static string 填充至底稿按钮 = "填充至底稿功能说明[|]用于将当前显示的科目余额表、明细账发送填充至相应的工作底稿中，在发送填充时，软件将自动识别相匹配的工作底稿。";

	public static string 显示设置菜单 = "[|]";

	public static string 表格样式 = "丰富的表格样式[|]您可以根据需要选择适用的表格样式，表格样式主要表现为表格线的不同风格。";

	public static string 显示零值 = "零值的不同显示方式[|]这里您可以选择零值的显示方式，在设置时，你必须选择一个或若干个单元格。";

	public static string 显示设置菜单_索引号 = "显示或隐藏文件导航树的索引号[|]点击此处可显示或隐藏左侧文件导航树的所有表格或文档的索引号。";

	public static string Ribbon菜单_显示设置菜单_表格标题 = "显示或隐藏表格的主副标题[|]点击此处可显示或隐藏表格的主副标题，隐藏主副标题便于您腾出屏幕空间专注于表格内容的编辑。";

	public static string 显示设置菜单_表底尾注 = "显示或隐藏表底尾注[|]点击此处，可显示或隐藏表格的表底尾注区域，表底尾注一般用于报表的表底签名等功能。";

	public static string 底稿说明 = "显示或隐藏" + StringConstBase.Current.TableNote + "[|]点击此处可显示或隐藏表格底部的" + StringConstBase.Current.TableNote + "，" + StringConstBase.Current.TableNote + "区域支持的内容格式非常丰富，您除了可对字体样式、颜色、大小、段落格式等内容进行编辑设置外，您还可以插入表格、图片、文本框等特殊对象。";

	public static string 校验公式 = "显示或隐藏校验公式[|]点击此处可显示或隐藏表格的校验公式，校验公式与运算公式不同，校验公式专门用于检查表格内或表格间的逻辑关系是否正确。";

	public static string 显示设置菜单_校验结果 = "显示或隐藏校验结果[|]点击此处可显示或隐藏表格的校验结果，包括反映校验结果状态的单元格背景色和校验结果提示框。";

	public static string 显示设置菜单_动态提示 = "显示或隐藏动态提示[|]点击此处可显示或隐藏动态提示，动态提示可引导您正确地编辑、追踪、校验表格或文档。";

	public static string 显示设置菜单_软件向导 = "显示或隐藏本提示框[|]点击此处可显示或隐藏本软件向导提示框，软件向导可帮助您快速熟悉本软件的功能操作。";

	public static string 表格编辑菜单 = "[|]";

	public static string 锁定表格 = "锁定表格功能说明[|]当您希望当前表格为自己独占编辑，不希望他人协同编辑时，您可以锁定该表格。\r\n当您只需要锁定表格中的部分内容时，您可以选择锁定行、列或者单元格，锁定行、列或者单元格的功能在右键快捷菜单中，您可以轻易找到。";

	public static string 辅助编辑 = "辅助编辑功能说明[|]1、辅助编辑是为帮助您更快速、更准确的录入数据而设计的功能，包括：下拉列表、编辑注释、默认内容。\r\n2、下拉列表的功能：当您希望单元格或整列只能约束录入固定的几个列举值时，您可把这几个列举值定义到下拉列表中，这样您在录入时只能下拉选取这几个列举值，这有利于规范数据的录入。\r\n3、编辑注释的功能：当您希望用户在录入单元格或某列时，希望弹出提示性或注释性文字，以引导或提示用户时，您可为单元格或一整列定义注释文字，该功能多在模板定制环节中使用。\r\n4、默认内容的功能：当一个表格中的单元格或某列在多数情况下录入内容相对固定时，您可以为单元格或整列定义默认内容，当某单元格设置了默认内容，在其内容为空时，您可以通过右键菜单“运算表格”将默认内容自动填充到单元格中。";

	public static string 高级功能菜单 = "[|]";

	public static string 高级功能_样式刷 = "样式刷功能说明[|]样式刷用于将当前表格的标题样式、表体字体样式、表格线样式、打印设置样式批量赋予其他表格，这便于批量统一表格的外观样式。";

	public static string 列对应采数设置按钮 = "列对应采数设置功能说明[|]1、列对应采数设置适用于表格以整列为单元进行批量采数的情形，多用于明细表或审定表的数据采集。\r\n2、多数情况下，您无须做当前表的整表采数设置，软件会根据当前表格的表名及结构自动识别其与账套中数据的采集填充关系，从而智能配置并填充。只有在软件有时无法自动识别当前表格与账务数据的填充关系时，才须点此按钮手工配置填充关系。";

	public static string 单元格采数设置按钮 = "单元格采数功能说明[|]1、单元格采数设置适用于表格单元格的采数情形，多用于会计报表中的数据采集。如：《资产负债表》的“货币资金”项目的期末余额可以对账套的库存现金+银行存款+其他货币资金进行取数。\r\n2、多数情况下，您无须做当前表的单元格采数设置，软件会根据当前表格的表名及结构自动识别相应单元格与账套中数据的采集填充关系，从而智能配置并填充。只有在软件有时无法自动识别当前表格与账务数据的填充关系时，才须点此按钮手工配置填充关系。";

	public static string 采数填充按钮 = "采账填充功能说明[|]采账填充用于将账务中的数据自动填充到当前表格的相应列或单元格中，有时全自动智能填充可能因表格定制不规范，无法保证完全准确和完整时，您可以点击整表采数设置或单元格采数设置进行手工补充或校正。";

	public static string 一键批量填充按钮 = "一键批量填充功能说明[|]一键批量填充将对当前" + StringConstBase.Current.Project + "中所有表格进行智能识别并填充，有些表格会智能识别为抽样填充，您可以在批量填充前，在“系统设置”窗体中设置样本比例。";

	public static string 高级功能菜单_表格_生成函证 = "生成函证功能说明[|]生成函证用于以当前文档作为函证模板批量生成函证文档。";

	public static string 合并设置按钮 = "合并报表设置功能说明[|]合并报表设置用于指定当前表格做为合并报表，并指定当前表由来源单体哪些报表进行合并而来。在合并报表设置时，您需指定当前表格的合并维度列和合并金额列，并选择各来源单体的来源报表、合并维度、合并金额、持股比例和内部交易列。";

	public static string 合并报表按钮 = "合并报表结果功能说明[|]合并报表结果用于将来源单体相关报表数据自动汇总合并至当前表格，前提是您需要先进行“合并报表设置”操作，将合并范围、各来源单体报表、各报表合并维度和合并金额提前配置好后，即可实现“一键合并报表”。";

	public static string 编制签名按钮 = "编制签名功能说明[|]编制签名是指将当前用户指定为底稿编制人，并生成编制人签名，系统默认的编制签名位置在副标题区第1行最右侧位置。若您需要修改默认的签名格式及位置，可在“系统设置-&gt;签名设置”中进行修改。";

	public static string 复核签名按钮 = "复核签名功能说明[|]复核签名是指将当前用户指定为底稿复核人，并生成复核人签名，系统默认的复核签名位置在副标题区第2行最右侧位置。若您需要修改默认的签名格式及位置，可在“系统设置-&gt;签名设置”中进行修改。";

	public static string 变量管理按钮 = "变量管理功能说明[|]1、变量管理用于定义" + StringConstBase.Current.Project + "中的变量，以供在表格或文档中引用，软件中内置了若干" + StringConstBase.Current.Project + "变量，如：[当前用户姓名]、[当前" + StringConstBase.Current.Project + "名称]等。\r\n2、变量主要是为在表格或文档中引用，引用功能的好处是：当变量被表格或文档引用后，若变量的值发生变化，表格或文档中的引用均可联动刷新。如，我们可在" + StringConstBase.Current.Project + "模板中定义变量[审计报告日期]，然后将这个变量引用到审计报告的正文模板中，在具体" + StringConstBase.Current.Project + "中，您设置了[审计报告日期]的值后，则审计报告的日期可跟随刷新。变量引用得当，可提高表格或文档的编辑或生成效率，也可最大限度避免手工录入的错误。";

	public static string 引用表格按钮 = "引用表格功能说明[|]引用表格是指在云文档中可以插入任意指定的云表格，且表格的源数据若发生变动，文档中的该表格亦可通过刷新操作保持联动，以确保数据的一致性。这个功能的意义极为重要，这意味着，对于“财务报表附注”这样文表混排的大文档生成将变得极为智能和快捷。";

	public static string 高级功能菜单_文档_变量管理 = "变量管理功能说明[|]1、变量管理用于定义" + StringConstBase.Current.Project + "中的变量，以供在表格或文档中引用，软件中内置了若干" + StringConstBase.Current.Project + "变量，如：[当前用户姓名]、[当前" + StringConstBase.Current.Project + "名称]等。\r\n2、变量主要是为在表格或文档中引用，引用功能的好处是：当变量被表格或文档引用后，若变量的值发生变化，表格或文档中的引用均可联动刷新。如，我们可在" + StringConstBase.Current.Project + "模板中定义变量[审计报告日期]，然后将这个变量引用到审计报告的正文模板中，在具体" + StringConstBase.Current.Project + "中，您设置了[审计报告日期]的值后，则审计报告的日期可跟随刷新。变量引用得当，可提高表格或文档的编辑或生成效率，也可最大限度避免手工录入的错误。";

	public static string 表格刷新按钮 = "表格刷新功能说明[|]此按钮可对当前文档中的选中表格进行内容刷新，以保持与源表格内容的一致性。";

	public static string 全文刷新按钮 = "全文刷新功能说明[|]此按钮可对当前文档中的所有引用表格和变量进行刷新，刷新过程可能需要一些时间。";

	public static string 高级功能菜单_文档_函证设置 = "函证设置功能说明[|]函证设置用于以当前文档作为函证模板，将函证模板中的变量信息关联到某表格，该表格将作为函证数据源。";

	public static string 高级功能菜单_文档_生成函证 = "生成函证功能说明[|]生成函证用于以当前表格作为函证数据源批量生成函证文档。";

	public static string 运算校验菜单 = "[|]";

	public static string 当前表运算 = "当前表运算功能说明[|]当前表运算用于对当前表格中的所有运算公式执行运算，当前表格中的公式一般情况下在编辑过程中是自动运算的，个别情况下未实时运算时，您可以点击这里执行运算。";

	public static string 关联表运算 = "关联表运算功能说明[|]关联表运算会将与当期表存在运算关系的表格执行一遍运算，多在当前表修改数据后，其他关联表的公式若引用到当前表的情况下才需要执行。";

	public static string 全部表运算 = "全部表运算功能说明[|]全部表运算会一次性将当期" + StringConstBase.Current.Project + "的全部表格执行一遍运算，多在跨表公式未能实时运算的情况下才需要执行。\r\n注：打开" + StringConstBase.Current.Project + "后首次执行全部表运算速度会略缓。";

	public static string 当前表校验 = "当前表校验功能说明[|]当前表校验用于对当前表格的逻辑关系执行检查，当存在逻辑错误的数据时，该错误区域会以红色背景标识。\r\n注：表格执行校验需要您提前定义校验公式。";

	public static string 全部表校验 = "全部表校验功能说明[|]全部表校验用于对当前打开" + StringConstBase.Current.Project + "的所有表格执行一遍校验，当存在逻辑错误的数据时，表格的错误区域会以红色背景标识。\r\n注：表格执行校验需要您提前定义校验公式。";

	public static string 当前文档校验 = "当前文档校验功能说明[|]当前文档校验用于对当前文档的引用表格和变量执行一致性检查，当引用表格或变量与数据源不一致时会予以提示。";

	public static string 上一个错误 = "上一个错误[|]点击定位表格或文档的上一个错误区域。";

	public static string 下一个错误 = "下一个错误[|]点击定位表格或文档的下一个错误区域。";

	public static string 打印输出菜单 = "[|]";

	public static string excel文件按钮 = "导出excel文件功能说明[|]当云表格导出为Excel文件时，云表格的主副标题、表体内容、表底文字说明等构成元素将全部完整的导出到Excel工作表中，除此之外，云表格的运算公式也将完整导出，确保导出的Excel文件仍保存有单元格间的运算关系。";

	public static string 设置选项菜单 = "[|]";

	public static string 系统设置 = "系统设置说明[|]这里的设置包括了基本设置，菜单设置，账簿设置，表格样式，文档设置等内容。";

	public static string 帮助 = "查看帮助[|]查看帮助文件可帮您更系统、更详细的了解和学习软件功能，以便使您快速掌握软件的使用。";

	public static string 检查更新 = "检查更新说明[|]每次启动软件时会自检查更新，也可以选择在这里手动检查软件更新情况。";

	public static string 关于 = "关于软件[|]关于我们，了解更多软件的开发信息。";

	public static string 主窗体右上角配置栏 = "[|]";

	public static string 保存Project按钮 = "保存" + StringConstBase.Current.Project + "功能说明[|]此按钮为保存" + StringConstBase.Current.Project + "快捷按钮，功能为将您编辑修改的内容保存至本地，但并未保存到云端，网络状态不好时，建议使用此方法保证您的工作内容得以保存。";

	public static string Ribbon菜单_主窗体右上角配置栏_同步Project按钮 = "同步" + StringConstBase.Current.Project + "功能说明[|]此按钮为同步" + StringConstBase.Current.Project + "快捷按钮，功能为将您的工作内容上传至云端，同时将和您协同工作的同事们的工作内容下载到本地，实现" + StringConstBase.Current.Project + "数据的交换统一。";

	public static string 全屏按钮 = "全屏模式[|]支持全屏模式，再次点击可退出全屏模式。";

	public static string 主题设置按钮 = "更换主题[|]主题设置可以选择您喜欢的主题应用到软件界面。";

	public static string Ribbon菜单_主窗体右上角配置栏_即时讨论 = "即使讨论[|]点击此处可弹出即时讨论窗体，可与您的同事们实时沟通、发送弹幕、推送文件等。";

	public static string Ribbon菜单_主窗体右上角配置栏_软件向导 = "显示或隐藏本提示框[|]点击此处可显示或隐藏本软件向导提示框，软件向导可帮助您快速熟悉本软件的功能操作。";

	public static string Ribbon菜单_主窗体右上角配置栏_动态提示 = "显示或隐藏动态提示[|]点击此处可显示或隐藏动态提示，动态提示可引导您正确地编辑、追踪、校验表格或文档。";

	public static string Ribbon菜单_主窗体右上角配置栏_回退 = "回退功能说明[|]点击此处可回退到上一个表格或文档。";

	public static string Ribbon菜单_主窗体右上角配置栏_前进 = "前进功能说明[|]点击此处可前进到下一个表格或文档。";

	public static string Ribbon菜单_主窗体右上角配置栏_重新载入 = "重新载入功能说明[|]此按钮为文件的重新载入快捷按钮，用于重新打开当前文件，一般在拟放弃当前文件的编辑修改时使用。";

	public static string Ribbon菜单_主窗体右上角配置栏_增减成员 = "增减项目成员[|]点击此处可弹出项目成员窗体，可以向当前项目中添加新成员或者删除现有成员。";

	public static string Ribbon菜单_主窗体右上角配置栏_联系方式 = "联系方式[|]点击此处可以查看 AuditAI 的联系方式。";

	public static string 其他 = "[|]";

	public static string 主标题菜单标题栏 = "此处为Ribbon菜单功能区[|]此处双击可以显示/隐藏菜单功能区，当您认为菜单功能区占用屏幕空间时，您可以双击隐藏，隐藏后当您认为需要时，可再次双击显示。";

	public static string Project树 = "[|]";

	public static string 空outbar = "此处为文件导航面板[|]如果您为新建的空白" + StringConstBase.Current.Project + "，那么请点击右键“追加组”开启" + StringConstBase.Current.Project + "编辑作业的第一步。";

	public static string 组按钮 = "此处为文件分组页签[|]您可以右键增加、删除、重命名组，组本质上是一个一级文件夹，是为更方便组织和管理云文件而设计。";

	public static string 树 = "此处为文件导航树[|]1、您可以右键新建文件夹、新建表格或文档、复制表格或文档、剪切表格或文档，导入文件夹或文件等操作。\r\n2、您还可以右键选择“编辑索引号”对表格或文档的索引号进行编辑。编辑后只有在显示索引号模式下，索引号才会显示出来，您可点击“显示设置->索引号”按钮显示或隐藏索引号。\r\n3、导入文件夹或文件支持导入Excel、Word等格式文件，导入时将智能识别公式、格式等内容。这样可以快速的将您的Excel、Word模板导入转换成软件的" + StringConstBase.Current.Project + "模板。";

	public static string 财务数据窗体 = "[|]";

	public static string 科目余额表区域 = "科目余额表操作说明[|]1、您可以双击直接打开某科目的总账或明细账。\r\n2、您可以右键选择科目余额表的显示级次，如：“仅显示一级科目”、“仅显示末级科目”等。\r\n3、您可以右键对科目余额表进行数据的筛选，筛选功能丰富、智能且人性化。其实，软件中的任何表格均支持筛选功能，掌握筛选功能，您的工作将事半功倍。";

	public static string 总账区域 = "总账操作说明[|]1、您可以点击账页左上角的返回按钮返回科目余额表。\r\n2、您可以双击直接打开某科目的明细账。\r\n3、您可以右键选择总账的显示模式，如：“显示合计累计”、“仅显示合计”、“仅显示累计”。";

	public static string 明细账区域 = "明细账操作说明[|]1、您可以点击账页左上角的返回按钮返回科目余额表。\r\n2、您可以双击某明细条目打开相应的会计凭账。\r\n3、您可以右键选择明细账的显示模式，“显示合计累计”、“仅显示合计”、“仅显示累计”、“不显示合计累计”。\r\n3、右键菜单中的“方向调整”是指将某笔科目发生额调整至反方向，并同时将其金额调整为相反数。这样的调整的好处是便于对损益类科目进行分析和自动填充底稿，因为，有时记账人员未将损益类科目只记在一方发生，这会导致编制会计报表时无法直接取发生额。\r\n4、右键菜单中的“标记关注”是指当您认为某笔科目发生额存在异常或需额外关注时，可标记关注予以高亮显示。";

	public static string 会计凭证区域_指明细账的下方 = "会计凭证操作说明[|]您可以双击某凭证条目打开相应的科目明细账。";

	public static string 主窗体内容区域 = "[|]";

	public static string 主窗体运算公式文本框 = "此处为运算公式编辑框[|]1、公式定义方法：选中某单元格或某列，再点击此文本框即进入单元格公式编辑状态或列公式编辑状态，在编辑输入公式时您也可直接点击单元格或列自动生成公式元素。调用函数可直接点击前面的“函数”按钮，选取适用的函数进行编辑。在编辑公式时不用加入“=”，编辑完成后按Enter键公式即可完成生效。\r\n2、公式的保护：定义好的单元格公式或列公式，在您对表格进行正常的编辑操作时都不会对公式造成任何意外破坏，这点区别于Excel。\r\n3、几个特色函数：此处着重强调几个特色函数，极贴近于审计工作：分别为：（1）LqSumIf的典型应用场景为对调整分录明细表、合并抵消分录明细表的条件汇总；（2）LqDistinct用于去重分组，更方便工作底稿的跨表自动分类汇总；（3）LqVlookUp用于在列中检索符合条件的行次，并得到相应单元格的值。\r\n4、运算公式的意义：若您定制的公式体系严谨且完整，可搭建出自动且高效的数据流运算体系，不但可极大提升工作效率，也可有效避免数据逻辑错误的发生。";

	public static string 右边空白 = "此处为表格或文档的显示区域[|]1、您现在未选择打开任何一个表格或文档，当您选择打开一个表格或文档后，此处会显示表格或文档的内容。\r\n2、您可以将鼠标移至窗体最左侧边缘，然后点击，软件将切换至账务查询窗体界面。";

	public static string 主窗体云表格区域 = "[|]";

	public static string 主窗体内容区域_主窗体云表格区域_表格标题 = "此处为云表格标题区[|]1、标题区的构成：此处为表格的标题区，标题由主标题和副标题构成。其中主标题为1行1列，且无法删除；副标题为N行3列，行次可定制增减。\r\n2、标题区的应用：标题区域可引用变量，变量名须用“[]”括起来，如副标题第1行最左侧可编辑录入为：" + StringConstBase.Current.Auditee + "：[" + StringConstBase.Current.Auditee + "]。如果当前" + StringConstBase.Current.Project + "的“" + StringConstBase.Current.Auditee + "”的变量值为“AuditAI”，则副标题第1行最左侧将会自动显示为：“" + StringConstBase.Current.Auditee + "：AuditAI”。\r\n3、副标题的增减行：鼠标右键可增加、删除副标题行。";

	public static string 表格列头 = "此处为云表格的列头[|]1、双击此处可编辑列头文字\r\n2、列头的两种模式：此处为表格的列头，列头有两种显示模式，分别为自定制列头模式和字母序列头模式，自定制列头模式下的表格可任意修改列头名称，且在打印时该列头将做为表格的构成部分予以打印；字母序列头模式为类似Excel样式的A、B、C列头模式，此模式下的列头无法编辑，且在打印时隐藏。\r\n3、列头模式的切换：在表格的左上角点击鼠标右键可在两种列头模式间灵活切换。\r\n4、多层列头的实现：双击可以进入列头编辑状态，如果需要多层列头时，用英文状态下的下划线“_”进行连接，“_”会将列头分隔为两层。\r\n5、列公式的定义：先单击某列的列头选中该整列，再在运算公式编辑框中点击下鼠标，即进入列公式的定义状态。列公式在Excel中无法实现，为本软件的创新功能，且在实际工作中，列公式的使用占比远远高于单元格公式，建议您第一时间掌握。\r\n6、表格结构的建议：我们强列建议您在定制表格结构时，遵循数据库表的定制规范，将表格定制为规则的二维表格，规则的二维表格的特征是各列有明确的数据类型和列运算关系。";

	public static string 表格表体 = "云表格的功能说明[|]1、云表格的元素构成：一个云表格的整体界面元素分为标题区、表体区、表底区三个分区，分离三个区的意义在于：表体部分更方便定制为规则的二维表格，规则的二维表格在编辑录入、筛选查询及定义公式方面会极为高效。\r\n2、云表格的多人协同：一个云表格可以支持多人在线同时操作，实现审计作业的协同化，这将极大提升工作效率。多人在对表格进行编辑时，无论是增删行列、修改数据、修改公式等任何操作，表格同步后，这些操作将合并起效，您将可以看到其他人的编辑结果。\r\n3、云表格的快捷菜单：云表格的右键菜单放置了很多快捷菜单项，在不同的位置点击鼠标右键将得到不同的菜单项，操作快捷便利。我们在快捷菜单中集成了功能强大的数据筛选器，操作简单且高效，会给您带来超爽的使用体验。";

	public static string 主窗体状态栏中间按钮 = "校验公式、" + StringConstBase.Current.TableNote + "切换按钮[|]点击此处，可弹出表格的" + StringConstBase.Current.TableNote + "区域，您可以在此处对表格进行文字性的补充说明，再次点击可弹出校验公式编辑框，您可以为表格定制校验公式。";

	public static string 主窗体状态栏_上一个错误 = "上一个错误[|]点击定位表格或文档的上一个错误区域。";

	public static string 主窗体状态栏_下一个错误 = "下一个错误[|]点击定位表格或文档的下一个错误区域。";

	public static string 校验公式文本框 = "此处为校验公式编辑框[|]1、公式定义方法：校验公式的定义操作与运算公式基本相同，所不同的是，校验公式可以为一个单元格定义多条校验公式。一条校验用公式分为：公式说明、校验对象、关系、校验公式四部分组成，其中：校验对象多应定义为某一个单元格，而校验公式多应定义为一个表达式，如校验对象定义为：{资产负债表}[期末余额,1],而校验公式定义为：sum({货币资金明细表}[期末余额]),则表示资产负债表的货币资金期末数应等于货币资金明细表的期末余额列合计。\r\n2、校验公式的意义：面对庞大的表格系统时，事前定制相对严谨且完整的校验公式，将可实时发现和追踪错误数据，其远远高于人工排错的效率和效果，为保证您的作业质量保驾护航。";

	public static string 底稿说明文本框 = "此处为底稿说明文本框[|]该区域为一个文本编辑框，您可以在此处对表格进行文字性的说明，比较典型的应用如：" + StringConstBase.Current.TableNote + "及审计结论的编辑。文字说明支持的内容格式非常丰富，您除了可对字体样式、颜色、大小、段落格式等内容进行编辑设置外，您还可以插入表格、图片、文本框等特殊对象。";

	public static string 主窗体云文档区域 = "[|]";

	public static string 云文档区域 = "云文档的功能说明[|]1、云文档的基本功能：云文档从视觉及操作上极象Word,您可以在文档中自由编辑文字、段落、表格、图片、文本框等，也可对文档分节，还可分节编辑页眉页脚等，多数操作与Word基本相同。\r\n2、云文档的多人协同：一个云文档可以支持多人在线同时编辑，实现审计作业的协同化，这将极大提升工作效率。多人在编辑时只要不是对同一段落进行编辑，那么文档的增删改操作将合并起效，您将可以看到其他人的编辑结果。多人同时编辑文档的情形下，应避免同时编辑一个段落，若多人同时编辑一个段落，先同步者的编辑操作将被忽略不予保存生效。\r\n3、云文档的表格引用：表格引用是指在云文档中可以插入任意指定的云表格，且表格的源数据若发生变动，文档中的该表格亦可通过刷新后保持联动，以确保数据的一致性。这个功能的意义极为重大，这意味着，对于“财务报表附注”这样文表混排的大文档生成将变得极为智能和快捷。\r\n4、云文档的变量引用：变量引用可用在云文档中的任意位置处（包括页眉和页脚），当文档中插入的任意变量其值发生变动时，您可选中变量所在段落或区域进行刷新，新的变量文本内容将自动刷新生成。\r\n5、云文档的检查校验：文档校验用于对文档中的引用表格和变量执行一致性检查，当引用表格或变量与数据源不一致时会予以提示。";

	public static string 云表格_填表提示 = "填表提示功能说明[|]填表提示用于显示当前表格主标题的编辑注释，主标题的编辑注释支持自定义。";

	public static string 云表格_表底签名 = "表底签名功能说明[|]点击此处可显示或隐藏表底签名行。" + StringConstBase.Current.TableNote + "的向导提示：点击此处可显示或隐藏表格下方的" + StringConstBase.Current.TableNote + "编辑区。";

	public static string 云文档_侧边栏_编辑提示 = "编辑提示功能说明[|]编辑提示用于显示当前文档首段的段落注释，文档中的段落注释支持自定义。";

	public static string 云文档_侧边栏_追踪引用 = "追踪引用功能说明[|]追踪引用用于追踪当前引用表格的引用源。";

	public static string 云表格_侧边栏_关联表格 = "关联表格功能说明[|]关联表格用于显示当前表格运算公式中引用的其他表格。";

	public static string Ribbon菜单_主窗体右上角配置栏_流程图 = "显示数据流路线图[|] 点击此处可全景显示" + StringConstBase.Current.Project + "中数据流转的路线图。";

	public static string Project管理菜单_查看备份 = "查看备份功能说明[|]用于查看当前文件的office备份，只有在“系统设置”中勾选了“保存{SCB.Current.Project}时强制备份office文件”，才会生成office备份。";
}
