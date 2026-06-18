using System.Linq;

namespace Leqisoft.Model;

public class ControlFormulaManager
{
	private readonly Project _project;

	public ControlFormulaManager(Project project)
	{
		_project = project;
	}

	public void CalculateAll()
	{
		foreach (Table item in from n in _project.GetAllTableNodes()
			select n.Table into t
			where t._loaded
			select t)
		{
			string controlFormula = item.ControlFormula;
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(controlFormula);
		}
	}
}
