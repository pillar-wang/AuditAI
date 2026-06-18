namespace Leqisoft.Model;

public sealed class StringConstTaxEdition : StringConstBase
{
	public override string AppName => "{0}（{1}）";

	public override string Auditee => "被审计单位名称";

	public override string TableNote => "审计说明";

	public override string Project => "项目";

	public override string Manager => "项目经理";

	public override string Assistant => "项目助理";

	public override string Template => "模板";

	public override string SelectTemplate => "选择模板";

	public override string NotUseTemplate => "不使用模板";
}
