using System.Text;

namespace Auditai.Model;

public static class StringUtils
{
	public static string ExtractText(this string strText, string splitChar = "|")
	{
		StringBuilder stringBuilder = new StringBuilder(strText.Length);
		int length = strText.Length;
		int num = -1;
		bool flag = true;
		for (int i = 0; i < length; i++)
		{
			char c = strText[i];
			bool flag2 = false;
			if (c >= '0' && c <= '9')
			{
				flag2 = true;
			}
			else
			{
				switch (c)
				{
				case '.':
					if (i > 0 && i + 1 < length)
					{
						char c5 = strText[i - 1];
						char c6 = strText[i + 1];
						if (c5 >= '0' && c5 <= '9' && c6 >= '0' && c6 <= '9')
						{
							flag2 = true;
						}
					}
					break;
				case '-':
					if (i + 1 < length)
					{
						char c4 = strText[i + 1];
						if (c4 >= '0' && c4 <= '9')
						{
							flag2 = true;
						}
					}
					break;
				case ',':
					if (i > 0 && i + 1 < length)
					{
						char c2 = strText[i - 1];
						char c3 = strText[i + 1];
						if (c2 >= '0' && c2 <= '9' && c3 >= '0' && c3 <= '9')
						{
							flag2 = true;
						}
					}
					break;
				}
			}
			if (flag2)
			{
				flag = true;
				continue;
			}
			if (flag)
			{
				num++;
				flag = false;
				if (num > 0 && splitChar.Length > 0)
				{
					stringBuilder.Append(splitChar);
				}
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	public static string ExtractNumber(this string strText, string splitChar = "|")
	{
		StringBuilder stringBuilder = new StringBuilder(strText.Length);
		int length = strText.Length;
		int num = -1;
		bool flag = true;
		for (int i = 0; i < length; i++)
		{
			char c = strText[i];
			bool flag2 = false;
			if (c >= '0' && c <= '9')
			{
				flag2 = true;
			}
			else
			{
				switch (c)
				{
				case '.':
					if (i > 0 && i + 1 < length)
					{
						char c6 = strText[i - 1];
						char c7 = strText[i + 1];
						if (c6 >= '0' && c6 <= '9' && c7 >= '0' && c7 <= '9')
						{
							flag2 = true;
						}
					}
					break;
				case '-':
					if (i + 1 < length)
					{
						char c4 = strText[i + 1];
						if (c4 >= '0' && c4 <= '9')
						{
							flag2 = true;
						}
					}
					if (i > 0)
					{
						char c5 = strText[i - 1];
						if (c5 >= '0' && c5 <= '9')
						{
							flag = true;
						}
					}
					break;
				case ',':
					if (i > 0 && i + 1 < length)
					{
						char c2 = strText[i - 1];
						char c3 = strText[i + 1];
						if (c2 >= '0' && c2 <= '9' && c3 >= '0' && c3 <= '9')
						{
							flag2 = true;
						}
					}
					break;
				}
			}
			if (!flag2)
			{
				flag = true;
				continue;
			}
			if (flag)
			{
				num++;
				flag = false;
				if (num > 0 && splitChar.Length > 0)
				{
					stringBuilder.Append(splitChar);
				}
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}
}
