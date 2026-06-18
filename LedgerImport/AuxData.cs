public class AuxData
{
	public string AccountCode { get; set; }

	public string AuxType { get; set; }

	public string AuxCode { get; set; }

	public string AuxName { get; set; }

	public AuxData()
	{
	}

	public AuxData(string accountCode, string auxType, string auxCode, string auxName)
	{
		AccountCode = accountCode;
		AuxType = auxType;
		AuxCode = auxCode;
		AuxName = auxName;
	}
}
