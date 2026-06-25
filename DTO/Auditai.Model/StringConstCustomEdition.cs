namespace Auditai.Model;

public class StringConstCustomEdition : StringConstBase
{
	public string DisplayFormat_AppName { get; set; } = "{0}（{1}）";


	public string DisplayName_Auditee { get; set; } = "单位名称";


	public string DisplayName_TableNote { get; set; } = "表格尾注";


	public string DisplayName_Project { get; set; } = "系统";


	public string DisplayName_Manager { get; set; } = "系统负责人";


	public string DisplayName_Assistant { get; set; } = "系统协作人";


	public string DisplayName_Template { get; set; } = "模板";


	public string DisplayName_SelectTemplate { get; set; } = "选择模板";


	public string DisplayName_NotUseTemplate { get; set; } = "不使用模板";


	public int EditionCode { get; private set; }

	public override string AppName => DisplayFormat_AppName;

	public override string Auditee => DisplayName_Auditee;

	public override string TableNote => DisplayName_TableNote;

	public override string Project => DisplayName_Project;

	public override string Manager => DisplayName_Manager;

	public override string Assistant => DisplayName_Assistant;

	public override string Template => DisplayName_Template;

	public override string SelectTemplate => DisplayName_SelectTemplate;

	public override string NotUseTemplate => DisplayName_NotUseTemplate;

	public StringConstCustomEdition(int editionCode)
	{
		EditionCode = editionCode;
	}
}
