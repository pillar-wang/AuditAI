public class AccData
{
	public string AccCode { get; set; }

	public string AccName { get; set; }

	public AccData()
	{
	}

	public AccData(string accCode, string accName)
	{
		AccCode = accCode;
		AccName = accName;
	}
}
