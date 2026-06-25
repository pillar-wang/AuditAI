using System;

namespace Auditai.DTO;

public class CacheValidate
{
	public DateTime GenerateTime { get; set; }

	public string Code { get; set; }

	public int attempTimes { get; set; }

	public int GetTimes { get; set; }

	public bool Valid { get; set; }

	public string PhoneNumber { get; set; }

	public CacheValidate()
	{
		GenerateTime = DateTime.Now;
		GetTimes = 1;
		attempTimes = 0;
		Valid = true;
	}

	public void Reset()
	{
		GenerateTime = DateTime.Now;
		attempTimes = 0;
		Valid = true;
	}
}
