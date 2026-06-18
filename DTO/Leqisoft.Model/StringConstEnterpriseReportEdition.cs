namespace Leqisoft.Model;

public sealed class StringConstEnterpriseReportEdition : StringConstBase
{
	public override string AppName => "{0}（{1}）";

	public override string Auditee => "单位名称";

	public override string TableNote => "表格尾注";

	public override string Project => "表库";

	public override string Manager => "表库负责人";

	public override string Assistant => "表库协作人";

	public override string Template => "模板";

	public override string SelectTemplate => "选择模板";

	public override string NotUseTemplate => "不使用模板";
}
